using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ScannerModels.Model;

public class AttendanceLog
{
    [Key]
    public long AttendanceLogID { get; set; }

    // --- المفتاح الأجنبي للشُعبة ---

    public long CourseSectionID { get; set; }
    public virtual CourseSection CourseSection { get; set; }

    // --- المفتاح الأجنبي للطالب ---

    public string StudentUserID { get; set; } // يشير إلى AspNetUsers.Id
    public virtual ApplicationUser Student { get; set; }

    public DateTime AttendanceDate { get; set; }

    // (0=غائب، 1=حاضر،...)
    public byte PresenceStatus { get; set; }

    // (1=QR، 2=Excel، 3=يدوي)
    public byte AttendanceMethod { get; set; }

    // --- بيانات خاصة بـ QR Code ---
    public DateTime? ScanTimestamp { get; set; }

    // تحديد الدقة لبيانات GPS [12, 13]
    public decimal? ScanLatitude { get; set; }


    public decimal? ScanLongitude { get; set; }

    public string? ScanDeviceSignature { get; set; } // "بصمة" الجهاز [13]
    public string? LiveLocation { get; set; } // الموقع

    // --- بيانات خاصة بالإدخال اليدوي/Excel ---
    // يخزن هوية الدكتور الذي قام بالإدخال
    [ForeignKey("Verifier")]
    public string? VerifiedByUserID { get; set; } // يشير إلى AspNetUsers.Id
    public virtual ApplicationUser? Verifier { get; set; }
    public string? QrCode { get; set; }
}

public class AttendanceScanRequest
{
    public long SectionId { get; set; }
    public string StudentUserID { get; set; }
    public string Code { get; set; }
}

public class StudentAttendanceInput
{
    public string StudentUserID { get; set; }
    public bool IsPresent { get; set; }
}
