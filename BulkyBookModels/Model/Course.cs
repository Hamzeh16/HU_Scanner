using System.ComponentModel.DataAnnotations;

namespace ScannerModels.Model;

public class Course
{
    [Key]
    public int CourseID { get; set; }



    public string? CourseCode { get; set; }



    public string? CourseName { get; set; }

    // --- علاقة المفتاح الأجنبي للقسم ---

    public int DepartmentID { get; set; }
    public virtual Department? Department { get; set; }
}

