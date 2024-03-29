using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using EmploAdImport.Ldap;
using Newtonsoft.Json;
using EmploApiSDK.Logger;
using EmploApiSDK.ApiModels.Employees;
using EmploApiSDK.Client;
using EmploApiSDK.Logic.EmployeeImport;
using System.Text.RegularExpressions;

namespace EmploAdImport.Importer
{
    public class EmployeeAdImportLogic
    {
        private readonly ILogger _logger;
        private readonly ApiClient _apiClient;
        private readonly ApiConfiguration _apiConfiguration;
        private readonly ImportLogic _importLogic;

        public EmployeeAdImportLogic(ILogger logger)
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
            _importLogic = new ImportLogic(logger);
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

                TransformFields(importUsersRequestModel);
                var result = _importLogic.ImportEmployees(importUsersRequestModel).Result;
                if (result == -1)
                {
                    Environment.Exit(-1);
                }
            }
            catch (Exception ex)
            {
                _logger.WriteLine(ex.Message);
                throw;
            }
        }

        public void BlockUser(string nameId)
        {
            _logger.WriteLine(String.Format("Sending request to block user {0}", nameId));
            var statusCode = _apiClient.SendPost<HttpStatusCode>(JsonConvert.SerializeObject(nameId), _apiConfiguration.BlockUserUrl);
            _logger.WriteLine("Response: " + statusCode);
        }

        public void TransformFields(ImportUsersRequestModel model)
        {
            foreach (var row in model.Rows)
            {
                TransformOfficeAddress(row);
                TransformEmployeeExternalSystems(row);
            }
        }

        private void TransformOfficeAddress(UserDataRow row)
        {
            string[] officeAddresses = new string[4];

            row.TryGetValue("OfficeAddress", out officeAddresses[0]);
            for (int i = 1; i < 4; i++)
            {
                string address = null;
                if (row.TryGetValue($"OfficeAddress_{i}", out address))
                {
                    row.Remove($"OfficeAddress_{i}");
                }
                officeAddresses[i] = address;
            }

            if (officeAddresses.Any(x => x != null && x != ""))
            {
                var officeAddress = string.Join(" ", officeAddresses.Where(x => x != null));
                row["OfficeAddress"] = officeAddress.Trim();
            }
        }

        private void TransformEmployeeExternalSystems(UserDataRow row)
        {
            var externalSystemIds = row.Keys.ToList().Where(x => x.StartsWith("ExternalSystemId_"));
            var entries = new List<ExternalSystemIdModel>();
            foreach(var x in externalSystemIds)
            {
                var externalSystemId = x.Replace("ExternalSystemId_", "");
                var externalId = row[x];
                row.Remove(x);

                var entry = new ExternalSystemIdModel
                {
                    ExternalId = externalId,
                    ExternalSystemId = Int32.Parse(externalSystemId)
                };

                entries.Add(entry);
                
            }

            row["ExternalSystemIds"] = JsonConvert.SerializeObject(entries);
        }

    }
}
