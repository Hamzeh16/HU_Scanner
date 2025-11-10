using System.ComponentModel.DataAnnotations;

namespace ScannerModels.Model;

// هذا هو جدول الربط (Junction Table) لعلاقة "كثير لكثير" [11, 9]
// يربط بين الطالب (ApplicationUser) والشُعبة (CourseSection)
public class StudentEnrollment
{
    [Key]
    public long StudentEnrollmentID { get; set; }

    // --- المفتاح الأجنبي للطالب ---

    public string StudentUserID { get; set; } // يشير إلى AspNetUsers.Id
    public virtual ApplicationUser Student { get; set; }

    // --- المفتاح الأجنبي للشُعبة ---

    public long CourseSectionID { get; set; }
    public virtual CourseSection CourseSection { get; set; }

    public DateTime EnrollmentDate { get; set; } = DateTime.UtcNow;
}

