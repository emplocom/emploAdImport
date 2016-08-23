using System;
using System.Configuration;

namespace EmploAdImport.Ldap
{
    public class LdapAccountConfiguration
    {
        public LdapAccountConfiguration()
        {
            EndpointAddress = ConfigurationManager.AppSettings["EndpointAddress"];
            EndpointPort = int.Parse(ConfigurationManager.AppSettings["EndpointPort"]);
            SslConnectionRequired = Boolean.Parse(ConfigurationManager.AppSettings["SslConnectionRequired"]); 
        }

        public string EndpointAddress { get; set; }
        public int EndpointPort { get; set; }
        public bool SslConnectionRequired { get; set; }
    }
}
