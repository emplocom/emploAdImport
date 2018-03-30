using System;
using System.Configuration;
using EmploAdImport.AttributeMappingsModel;
using EmploApiSDK.Logger;

namespace EmploAdImport.Validators
{
    class SettingsValidator
    {
        public static void ValidateSettings()
        {
            var isValid = true;
            var baseUrl = ConfigurationManager.AppSettings["EmploUrl"];
            if (String.IsNullOrEmpty(baseUrl))
            {
                LoggerFactory.Instance.WriteLine("EmploUrl is empty. Example url : https://example.emplo.com");
                isValid = false;
            }

            var emploLogin = ConfigurationManager.AppSettings["Login"];
            if (String.IsNullOrEmpty(emploLogin))
            {
                LoggerFactory.Instance.WriteLine("Login to emplo is empty");
                isValid = false;
            }

            var emploPassword = ConfigurationManager.AppSettings["Password"];
            if (String.IsNullOrEmpty(emploPassword))
            {
                LoggerFactory.Instance.WriteLine("Password to emplo is empty");
                isValid = false;
            }

            var endpointAddress = ConfigurationManager.AppSettings["EndpointAddress"];
            if (String.IsNullOrEmpty(endpointAddress))
            {
                LoggerFactory.Instance.WriteLine("EndpointAddress is empty");
                isValid = false;
            }

            var endpointPort = ConfigurationManager.AppSettings["EndpointPort"];
            if (String.IsNullOrEmpty(endpointPort))
            {
                LoggerFactory.Instance.WriteLine("EndpointPort is empty");
                isValid = false;
            }

            var sslConnectionRequired = ConfigurationManager.AppSettings["SslConnectionRequired"];
            if (String.IsNullOrEmpty(sslConnectionRequired))
            {
                LoggerFactory.Instance.WriteLine("SslConnectionRequired is empty");
                isValid = false;
            }

            var query = ConfigurationManager.AppSettings["Query"];
            if (String.IsNullOrEmpty(query))
            {
                LoggerFactory.Instance.WriteLine("Query is empty");
                isValid = false;
            }

            var importMode = ConfigurationManager.AppSettings["ImportMode"];
            if (String.IsNullOrEmpty(importMode))
            {
                LoggerFactory.Instance.WriteLine("ImportMode is empty. Available modes: CreateOnly, UpdateOnly,  CreateOrUpdate");
                isValid = false;
            }

            var claimConfigSection = ConfigurationManager.GetSection(AttributeMappingSection.SectionName) as AttributeMappingSection;
            if (claimConfigSection == null || claimConfigSection.Instances.Count < 5)
            {
                LoggerFactory.Instance.WriteLine("Configuration section " + AttributeMappingSection.SectionName + " does not contain at least 5 entries. Use this section for attributes mapping. NameId, Email, FirstName, LastName and Position are required");
                isValid = false;
            }

            if (!isValid)
            {
                LoggerFactory.Instance.WriteLine("Update config file and try again.");
                LoggerFactory.Instance.WriteLine("Emplo import stopped because of configuration errors");
                Environment.Exit(-1);
            }
        }
    }
}
