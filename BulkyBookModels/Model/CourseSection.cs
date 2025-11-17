using ScannerModels.Model;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ScannerModels.Model;

public class CourseSection
{
    [Key]
    public long CourseSectionID { get; set; }

    // --- علاقة المفتاح الأجنبي للمادة ---
    [ForeignKey("Course")]
    public int CourseID { get; set; }
    public virtual Course Course { get; set; }

    // (ستحتاج أيضًا إلى نموذج 'Semester' و 'Classroom' بنفس الطريقة)
    public int SemesterID { get; set; }
    // public virtual Semester Semester { get; set; }

    public int? ClassroomID { get; set; }
    // public virtual Classroom Classroom { get; set; }

    public int SectionNumber { get; set; }

    // --- علاقة المفتاح الأجنبي للدكتور ---

    public string? DoctorUserID { get; set; } // يشير إلى AspNetUsers.Id
    public virtual ApplicationUser? Doctor { get; set; }

    // المفتاح السري لتوليد TOTP لـ QR Code
    public string? TotpSecretKey { get; set; }

    // خاصية التنقل لعلاقة "كثير لكثير" (Many-to-Many) [9, 10]
    //public virtual ICollection<StudentEnrollment> Enrollments { get; set; }
    public virtual ICollection<StudentEnrollment> Enrollments { get; set; } = new List<StudentEnrollment>();

}

