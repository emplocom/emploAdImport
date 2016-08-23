using System;
using System.Collections.Generic;
using System.Configuration;
using EmploAdImport.AttributeMappingsModel;
using EmploAdImport.Log;

namespace EmploAdImport.Ldap
{
    public class LdapImportConfiguration
    {
        public LdapImportConfiguration(ILogger logger)
        {
            Query = ConfigurationManager.AppSettings["Query"];

            LdapAccountConfiguration = new LdapAccountConfiguration();
            PropertyMappings = new List<PropertyMapping>();

            var claimConfigSection = ConfigurationManager.GetSection(AttributeMappingSection.SectionName) as AttributeMappingSection;
            if (claimConfigSection == null)
            {
                logger.WriteLine(String.Format("Attributes mapping in {0} is empty", AttributeMappingSection.SectionName));
                Environment.Exit(-1);
            }

            foreach (AttributeMappingElement e in claimConfigSection.Instances)
            {
                if (!String.IsNullOrEmpty(e.Value))
                {
                    PropertyMappings.Add(new PropertyMapping(e.Name, e.Value));
                }
            }
        }

        public string Query { get; private set; }
        public LdapAccountConfiguration LdapAccountConfiguration { get; private set; }
        public List<PropertyMapping> PropertyMappings { get; private set; }
    }
}
