using System.Configuration;

namespace EmploAdImport.EmploApi
{
    class ApiConfiguration
    {
        public static string EmploUrl
        {
            get { return ConfigurationManager.AppSettings["EmploUrl"]; }
        }

        public static string ApiPath
        {
            get { return ConfigurationManager.AppSettings["ApiPath"] ?? "apiv2"; }
        }

        public static string Login
        {
            get { return ConfigurationManager.AppSettings["Login"]; }
        }

        public static string Password
        {
            get { return ConfigurationManager.AppSettings["Password"]; }
        }

        public static string ImportUsersUrl = EmploUrl + "/" + ApiPath + "/Users/Import";
        public static string FinishImportUrl = EmploUrl + "/" + ApiPath + "/Users/FinishImport";
        public static string BlockUserUrl = EmploUrl + "/" + ApiPath + "/Users/Block";
        public static string TokenEndpoint = EmploUrl + "/identity/connect/token";
    }
}
