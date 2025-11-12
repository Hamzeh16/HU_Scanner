using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ScannerModels.Model;

public class Department
{
    [Key]
    public int DepartmentID { get; set; }

    public string? DepartmentName { get; set; }

    // --- علاقة المفتاح الأجنبي للكلية ---
    [ForeignKey("College")]
    public int CollegeID { get; set; } // [7, 8]
    public virtual College? College { get; set; } // خاصية التنقل

    // --- علاقة المفتاح الأجنبي لرئيس القسم ---
    [ForeignKey("Head")]
    public string? HeadUserID { get; set; } // يشير إلى AspNetUsers.Id
    public virtual ApplicationUser? Head { get; set; } // خاصية التنقل
}

