namespace EmploAdImport.EmploApi.Models
{
    public class UpdateUnitResult
    {
        public string Message { get; set; }
        public bool IsError { get; set; }
        public int? UpdatedUnitId { get; set; }
        public int? OldParentId { get; set; }
        public int? NewParentId { get; set; }
    }
}