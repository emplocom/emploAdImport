using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using EmploApiSDK.Logger;
using System.Security.Principal;
using EmploApiSDK.ApiModels.Employees;
using EmploApiSDK.Logic.EmployeeImport;

namespace EmploAdImport.Ldap
{
    public class LdapClient
    {
        private readonly ILogger _logger;

        public LdapClient(ILogger logger)
        {
            _logger = logger;
        }

        public List<UserDataRow> GetDataToImportFromLdap()
        {
            try
            {
                var ldapImportConfiguration = new LdapImportConfiguration(_logger);
                var dataRowsFromLdap = ImportDataRowsFromLdap(ldapImportConfiguration);
                return dataRowsFromLdap.ToList();
            }
            catch (EmploApiClientFatalException)
            {
                Environment.Exit(-1);
                return null;
            }
        }

        private IEnumerable<UserDataRow> ImportDataRowsFromLdap(LdapImportConfiguration importConfig)
        {
            using (var startingPoint = GetRootDirectoryEntry(importConfig.LdapAccountConfiguration))
            {
                using (var search = new DirectorySearcher(startingPoint) { Filter = importConfig.Query })
                {
                    foreach (var mapping in importConfig.PropertyMappings.Select(m => m.ExternalPropertyName))
                    {
                        search.PropertiesToLoad.Add(mapping);
                    }

                    if (importConfig.PropertyMappings.Select(m => m.ExternalPropertyName).Any(m =>
                        new List<string> { SpecialProperties.FirstOrganizationalUnit, SpecialProperties.OrganizationalUnitsList }.Contains(m)))
                    {
                        search.PropertiesToLoad.Add("distinguishedName");
                    }

                    _logger.WriteLine("Getting users' data from Active Directory...");
                    search.PageSize = 1000;
                    var resultCollection = search.SafeFindAll();
                    var importedRows = new List<UserDataRow>();
                    
                    foreach (SearchResult searchResult in resultCollection)
                    {
                        var importedEmployeeRow = new UserDataRow();

                        foreach (var mapping in importConfig.PropertyMappings)
                        {
                            switch (mapping.ExternalPropertyName)
                            {
                                case SpecialProperties.FirstOrganizationalUnit:
                                    FirstOrganizationalUnitMappingOperation(importedEmployeeRow, searchResult, mapping);
                                    break;
                                case SpecialProperties.OrganizationalUnitsList:
                                    OrganizationalUnitsListMappingOperation(importedEmployeeRow, searchResult, mapping);
                                    break;
                                default:
                                    DefaultRowMappingOperation(importedEmployeeRow, searchResult, mapping);
                                    break;
                            }
                        }

                        if (importedEmployeeRow.ContainsKey("NameId") && !String.IsNullOrWhiteSpace(importedEmployeeRow["NameId"]))
                        {
                            importedRows.Add(importedEmployeeRow);
                        }
                        else
                        {
                            _logger.WriteLine(String.Format("User skipped: EMPTY NameId. Record path in AD: {0}", searchResult.Path));
                        }
                    }

                    _logger.WriteLine(String.Format("Got {0} users from Active Directory", importedRows.Count));

                    return importedRows;
                }
            }
        }

        private void OrganizationalUnitsListMappingOperation(UserDataRow importedEmployeeRow, SearchResult searchResult, PropertyMapping mapping)
        {
            string distinguishedName;
            var property = searchResult.Properties["distinguishedName"][0];
            if (property is byte[])
            {
                distinguishedName = new SecurityIdentifier((byte[])property, 0).ToString();
            }
            else
            {
                distinguishedName = property.ToString();
            }

            //Example: CN=Arlene Huff,OU=Admin (HR-PR department),OU=Admin (HR department),OU=Admin (administration department),DC=ad-master,DC=emplo-master,DC=local
            var organizationalUnitCollection =
                    distinguishedName.Split(',')
                    .Select(pathNode => pathNode.Split('='))
                    .Where(splitPathNode => splitPathNode[0].Equals("OU"))
                    .Select(splitPathNode => splitPathNode[1]).ToList();

            if (organizationalUnitCollection.Any())
            {
                importedEmployeeRow.Add(mapping.EmploPropertyName, string.Join(",", organizationalUnitCollection));
            }
            else
            {
                importedEmployeeRow.Add(mapping.EmploPropertyName, string.Empty);
            }
        }

        private void FirstOrganizationalUnitMappingOperation(UserDataRow importedEmployeeRow, SearchResult searchResult, PropertyMapping mapping)
        {
            string distinguishedName;
            var property = searchResult.Properties["distinguishedName"][0];
            if (property is byte[])
            {
                distinguishedName = new SecurityIdentifier((byte[])property, 0).ToString();
            }
            else
            {
                distinguishedName = property.ToString();
            }

            //Example: CN=Arlene Huff,OU=Admin (HR-PR department),OU=Admin (HR department),OU=Admin (administration department),DC=ad-master,DC=emplo-master,DC=local
            var organizationalUnitsList = 
                    distinguishedName.Split(',')
                    .Select(pathNode => pathNode.Split('='))
                    .Where(splitPathNode => splitPathNode[0].Equals("OU"))
                    .Select(splitPathNode => splitPathNode[1]).ToList();

            if (organizationalUnitsList.Any())
            {
                importedEmployeeRow.Add(mapping.EmploPropertyName, organizationalUnitsList.First());
            }
            else
            {
                importedEmployeeRow.Add(mapping.EmploPropertyName, string.Empty);
            }
        }

        private void DefaultRowMappingOperation(UserDataRow importedEmployeeRow, SearchResult searchResult, PropertyMapping mapping)
        {
            // mapping is defined (value is expected), but there is no value in AD, 
            // send info about "no value" to emplo
            if (!searchResult.Properties.Contains(mapping.ExternalPropertyName))
            {
                importedEmployeeRow.Add(mapping.EmploPropertyName, "");
            }

            // Not always string valus will be sent from LDAP, for example 'whenCreated' is DateTime
            // That's why we cannot use Cast instead of loop
            foreach (var property in searchResult.Properties[mapping.ExternalPropertyName])
            {
                // guid / security identifier is send as array of bytes 
                if (property is byte[])
                {
                    byte[] valueInBytes = (byte[])property;
                    string valueAsString = null;

                    if (valueInBytes.Length == 16)
                    {
                        valueAsString = new Guid(valueInBytes).ToString();
                    }
                    else
                    {
                        valueAsString = new SecurityIdentifier(valueInBytes, 0).ToString();
                    }

                    importedEmployeeRow.Add(mapping.EmploPropertyName, valueAsString);
                    continue;
                }

                importedEmployeeRow.Add(mapping.EmploPropertyName, property.ToString());
            }
        }

        /// <summary>
        /// Gets the LDAP connection string.
        /// </summary>
        private string GetLdapConnectionString(LdapAccountConfiguration configuration, bool withPrefix = true)
        {
            string format = withPrefix ? "LDAP://{0}:{1}" : "{0}:{1}";
            return string.Format(format, configuration.EndpointAddress, configuration.EndpointPort);
        }

        /// <summary>
        /// Getting root DirectoryEntry for more oldfashioned way of accesing LDAP service
        /// </summary>
        private DirectoryEntry GetRootDirectoryEntry(LdapAccountConfiguration configuration)
        {
            var path = GetLdapConnectionString(configuration);
            _logger.WriteLine(String.Format("Connecting to Active Directory {0}...", path));
            var root = new DirectoryEntry(path);

            if (configuration.SslConnectionRequired)
            {
                root.AuthenticationType = AuthenticationTypes.SecureSocketsLayer;
            }

            return root;
        }
    }


}
