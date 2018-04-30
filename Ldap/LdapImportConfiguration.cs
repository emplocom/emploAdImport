using System.Configuration;
using EmploApiSDK.Logger;
using EmploApiSDK.Logic.EmployeeImport;

namespace EmploAdImport.Ldap
{
    public class LdapImportConfiguration : BaseImportConfiguration
    {
        public LdapImportConfiguration(ILogger logger) : base(logger)
        {
            Query = ConfigurationManager.AppSettings["Query"];

            LdapAccountConfiguration = new LdapAccountConfiguration();
        }

        public string Query { get; private set; }
        public LdapAccountConfiguration LdapAccountConfiguration { get; private set; }
    }
}
