using System.Collections.Generic;
using System.Configuration;

namespace EmploAdImport.EmploApi.Models
{
    public class ImportUsersRequestModel
    {
        public ImportUsersRequestModel()
        {
            Rows = new List<UserDataRow>();
            Mode = ConfigurationManager.AppSettings["ImportMode"];
            RequireRegistrationForNewEmployees = ConfigurationManager.AppSettings["RequireRegistrationForNewEmployees"];
        }

        public string ImportId { get; set; }

        /// <summary>
        /// Data to import
        /// </summary>
        public List<UserDataRow> Rows { get; set; }

        /// <summary>
        /// Indicates if data should be created, inserted or both
        /// </summary>
        public string Mode { get; set; }

        public string RequireRegistrationForNewEmployees { get; set; }
    }
}
