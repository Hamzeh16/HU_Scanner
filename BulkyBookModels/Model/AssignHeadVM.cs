namespace ScannerModels.Model
{
    public class AssignHeadVM
    {
        public int DepartmentID { get; set; }
        public string? HeadUserID { get; set; }
        public IEnumerable<ApplicationUser> AvailableHeads { get; set; }
    }

}
