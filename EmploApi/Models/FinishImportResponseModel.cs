using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmploAdImport.EmploApi.Models
{
    public class FinishImportResponseModel
    {
        public ImportStatusCode ImportStatusCode { get; set; }
        public List<int> BlockedUserIds { get; set; }
        public List<UpdateUnitResult> UpdateUnitResults { get; set; }
    }
}
