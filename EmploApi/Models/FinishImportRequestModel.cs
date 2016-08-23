using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmploAdImport.EmploApi.Models
{
    public class FinishImportRequestModel
    {
        public FinishImportRequestModel()
        {
            BlockSkippedUsers = ConfigurationManager.AppSettings["BlockSkippedUsers"];
        }

        /// <summary>
        /// Id of the import which is being finished
        /// </summary>
        public string ImportId { get; set; }

        /// <summary>
        /// If set to true, all the users which were not present in the given import will be blocked
        /// </summary>
        public string BlockSkippedUsers { get; set; }  
    }
}
