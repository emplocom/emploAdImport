using System.Collections.Generic;

namespace EmploAdImport.EmploApi.Models
{
    public enum ImportStatusCode
    {
        Ok,
        WrongImportId,
        ImportIsFinished
    }

    public class ImportUsersResponseModel
    {
        public ImportStatusCode ImportStatusCode { get; set; }
        public string ImportId { get; set; }
        public List<ImportValidationSummaryRow> OperationResults { get; set; }
    }

    public enum ImportStatuses
    {
        Ok,
        MissingData,
        InvalidData,
        NotImplemented,
        ObjectAlreadyExists,
        Error,
        Skipped
    }

    public class ImportValidationSummaryRow
    {
        public ImportStatuses StatusCode { get; set; }
        public int? EmployeeId { get; set; }
        public string Employee { get; set; }
        public List<string> ErrorColumns { get; set; }
        public List<string> ChangedColumns { get; set; }
        public bool Created { get; set; }
        public string Message { get; set; }
    }
}
