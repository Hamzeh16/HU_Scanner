using ScannerModels.Model;
using System.ComponentModel.DataAnnotations;

namespace ScannerModels.Model;

public class College
{
    [Key] // يُعرف هذا كمفتاح أساسي (Primary Key)
    public int CollegeID { get; set; }



    public string? CollegeName { get; set; }


    public string? Building { get; set; }

    // --- علاقة المفتاح الأجنبي (Foreign Key) للعميد ---
    // هذا هو المفتاح الأجنبي الذي يشير إلى جدول AspNetUsers [5, 6]

    public string? DeanUserID { get; set; }

    // خاصية التنقل (Navigation Property) لـ EF Core
    public virtual ApplicationUser? Dean { get; set; }
}

