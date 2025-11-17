namespace ScannerModels.Model
{
    public class SectionVM
    {
        public long SectionID { get; set; }
        public string CourseName { get; set; }
        public int SectionNumber { get; set; }

        public string? DoctorUserID { get; set; }

        public string? DoctorFullName { get; set; } // null = Not Assigned
    }

}
