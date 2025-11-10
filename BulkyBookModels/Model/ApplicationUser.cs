using Microsoft.AspNetCore.Identity;

namespace ScannerModels.Model;

// هذه الفئة توسع IdentityUser المدمج [2, 4]
public class ApplicationUser : IdentityUser
{
    // الحقول المخصصة التي أضفناها إلى جدول AspNetUsers [1]

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string? IDNumber { get; set; } // هذا هو الرقم الجامعي/الوظيفي

    public string? ProfilePictureUrl { get; set; }
    public DateTime DateJoined { get; set; } = DateTime.UtcNow;

    // --- خصائص التنقل (Navigation Properties) ---
    // هذه الخصائص تُعرف العلاقات (Relationships)

    // لربط العميد بالكلية التي يديرها (علاقة واحد لواحد)
    public virtual College? ManagedCollege { get; set; }

    // لربط رئيس القسم بالقسم الذي يديره (علاقة واحد لواحد)
    public virtual Department? ManagedDepartment { get; set; }

    // لربط الدكتور بالشُعب التي يدرسها (علاقة واحد لكثير)
    public virtual ICollection<CourseSection> TaughtSections { get; set; }

    // لربط الطالب بالشُعب المسجل بها (عبر جدول الربط)
    public virtual ICollection<StudentEnrollment> Enrollments { get; set; }

    // لربط الطالب بسجلات حضوره
    public virtual ICollection<AttendanceLog> AttendanceLogs { get; set; }
    public string? TypeUser { get; set; }
}
