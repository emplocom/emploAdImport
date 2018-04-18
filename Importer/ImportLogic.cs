using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using EmploAdImport.Ldap;
using Newtonsoft.Json;
using EmploApiSDK.Logger;
using EmploApiSDK;
using EmploApiSDK.Models;

namespace EmploAdImport.Importer
{
    public class ImportLogic
    {
        private readonly ILogger _logger;
        private readonly ApiClient _apiClient;
        private readonly ApiConfiguration _apiConfiguration;

        public ImportLogic(ILogger logger)
        {
            _logger = logger;

            _apiConfiguration = new ApiConfiguration()
            {
                EmploUrl = ConfigurationManager.AppSettings["EmploUrl"],
                ApiPath = ConfigurationManager.AppSettings["ApiPath"] ?? "apiv2",
                Login = ConfigurationManager.AppSettings["Login"],
                Password = ConfigurationManager.AppSettings["Password"],
            };

            _apiClient = new ApiClient(_logger, _apiConfiguration);
        }
         
        public void ImportUsers()
        {
            try
            {
                var importMode = ConfigurationManager.AppSettings["ImportMode"];
                var requireRegistrationForNewEmployees = ConfigurationManager.AppSettings["RequireRegistrationForNewEmployees"];

                ImportUsersRequestModel importUsersRequestModel = new ImportUsersRequestModel(importMode, requireRegistrationForNewEmployees);

                string importFilePath = ConfigurationManager.AppSettings["ImportFromFilePath"];
                if (!string.IsNullOrWhiteSpace(importFilePath))
                {
                    if (!File.Exists(importFilePath))
                    {
                        _logger.WriteLine("File specified in ImportFromFilePath does not exists: " + importFilePath);
                        _logger.WriteLine("Correct the path or leave the path empty to import from AD");
                        Environment.Exit(-1);
                    }
                    _logger.WriteLine("Import is run from file: " + importFilePath + ". Active Directory won't be used");

                    var fileContent = File.ReadAllText(importFilePath);
                    importUsersRequestModel.Rows = JsonConvert.DeserializeObject<List<UserDataRow>>(fileContent);
                }
                else
                {
                    var importLogic = new LdapClient(_logger);
                    importUsersRequestModel.Rows = importLogic.GetDataToImportFromLdap();
                }

                bool dryRun;
                if (bool.TryParse(ConfigurationManager.AppSettings["DryRun"], out dryRun) && dryRun)
                {
                    _logger.WriteLine("Importer is in DryRun mode, data retrieved from Active Directory will be printed to log, but it won't be send to emplo.");
                    var serializedData = JsonConvert.SerializeObject(importUsersRequestModel.Rows);
                    _logger.WriteLine(serializedData);
                    Environment.Exit(0);
                }

                int chunkSize = GetChunkSize();
                _logger.WriteLine(String.Format("Sending employee data to emplo (in chunks in size of {0})", chunkSize));

                ImportUsersRequestModel importModel = new ImportUsersRequestModel(importMode, requireRegistrationForNewEmployees);

                // first, send data without superiors
                foreach (var chunk in importUsersRequestModel.Rows.Chunk(chunkSize))
                {
                    importModel.Rows = chunk.ToList();
                    var serializedData = JsonConvert.SerializeObject(importModel);
                    var importValidationSummary = _apiClient.SendPost<ImportUsersResponseModel>(serializedData, _apiConfiguration.ImportUsersUrl);
                    if (importValidationSummary.ImportStatusCode != ImportStatusCode.Ok)
                    {
                        _logger.WriteLine("Import action returned error status: " + importValidationSummary.ImportStatusCode);
                        Environment.Exit(-1);
                    }
                    importModel.ImportId = importValidationSummary.ImportId;
                    SaveImportValidationSummaryLog(importValidationSummary);
                }

                if (importUsersRequestModel.Rows.Any())
                {
                    _logger.WriteLine("Finishing import...");
                    FinishImportRequestModel requestModel = new FinishImportRequestModel(ConfigurationManager.AppSettings["BlockSkippedUsers"]);
                    requestModel.ImportId = importModel.ImportId;
                    var serializedData = JsonConvert.SerializeObject(requestModel);
                    var finishImportResponse = _apiClient.SendPost<FinishImportResponseModel>(serializedData, _apiConfiguration.FinishImportUrl);
                    if (finishImportResponse.ImportStatusCode != ImportStatusCode.Ok)
                    {
                        _logger.WriteLine("FinishImport action returned error status: " + finishImportResponse.ImportStatusCode);
                        Environment.Exit(-1);
                    }
                    else
                    {
                        if (finishImportResponse.BlockedUserIds != null && finishImportResponse.BlockedUserIds.Any())
                        {
                            _logger.WriteLine("Blocked user id's: " + String.Join(", ", finishImportResponse.BlockedUserIds));
                        }

                        if (finishImportResponse.UpdateUnitResults != null && finishImportResponse.UpdateUnitResults.Any())
                        {
                            _logger.WriteLine("Units tree was updated:");
                            foreach (var message in finishImportResponse.UpdateUnitResults)
                            {
                                if (message.IsError)
                                {
                                    _logger.WriteLine(String.Format("Unit update error: {0}", message.Message));
                                }
                                else
                                {
                                    _logger.WriteLine(String.Format("Unit updated: unit {0} was updated, old parent={1}, new parent={2}, message: {3}",
                                        message.UpdatedUnitId, message.OldParentId, message.NewParentId, message.Message));
                                }
                            }
                        }
                    }
                }

                _logger.WriteLine("Import has finished successfully");
            }
            catch (Exception ex)
            {
                _logger.WriteLine(ex.Message);
                throw ex;
            }
        }

        public void BlockUser(string nameId)
        {
            _logger.WriteLine(String.Format("Sending request to block user {0}", nameId));
            var statusCode = _apiClient.SendPost<HttpStatusCode>(JsonConvert.SerializeObject(nameId), _apiConfiguration.BlockUserUrl);
            _logger.WriteLine("Response: " + statusCode);
        }

        private void SaveImportValidationSummaryLog(ImportUsersResponseModel importValidationSummary)
        {
            if (!importValidationSummary.OperationResults.Any())
            {
                _logger.WriteLine("Result is empty.");
            }

            foreach (var result in importValidationSummary.OperationResults)
            {
                string employeeHeader = String.Format("{0} (id={1})", result.Employee, result.EmployeeId);

                if (result.StatusCode == ImportStatuses.Skipped)
                {
                    _logger.WriteLine(String.Format("{0} SKIPPED - {1}", employeeHeader, result.Message));
                    continue;
                }

                if (result.StatusCode != ImportStatuses.Ok)
                {
                    string error = String.Format("Status: {0}, ErrorColumns: {1}, Message: {2}", result.StatusCode, String.Join(",", result.ErrorColumns), result.Message);
                    _logger.WriteLine(String.Format("{0} ERROR - {1}", employeeHeader, error));
                    continue;
                }

                string changedColumns = String.Join(", ", result.ChangedColumns);
                if (result.Created)
                {
                    _logger.WriteLine(String.Format("{0} CREATED - Changed columns: {1}", employeeHeader, changedColumns));
                }
                else
                {
                    if (changedColumns.Any())
                    {
                        _logger.WriteLine(String.Format("{0} UPDATED - Updated columns: {1}", employeeHeader, changedColumns));
                    }
                    else
                    {
                        _logger.WriteLine(String.Format("{0} NO CHANGES", employeeHeader));
                    }
                }
            }
        }

        private int GetChunkSize()
        {
            string sizeString = ConfigurationManager.AppSettings["ChunkSize"] ?? "";
            int size;
            if (Int32.TryParse(sizeString, out size))
                return size;
            return 5;
        }
    }
}
