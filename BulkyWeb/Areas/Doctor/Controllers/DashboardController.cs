using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScannerDataAccess.Data;
using ScannerModels.Model;
using System.Text;

namespace ScannerWeb.Areas.Doctor.Controllers
{
    [Area("Doctor")]
    //[Authorize(Roles = "Doctor")]
    [Authorize(Roles = "Doctor,HeadOfDepartment")]
    public class DashboardController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AppDbContext _context;

        public DashboardController(UserManager<ApplicationUser> userManager, AppDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        // ===================== 1) LIST SECTIONS =====================
        public async Task<IActionResult> Index()
        {
            var doctor = await _userManager.GetUserAsync(User);

            var sections = await _context.CourseSections
                .Include(s => s.Course)
                    .ThenInclude(c => c.Department)
                .Where(s => s.DoctorUserID == doctor.Id)
                .ToListAsync();

            return View(sections);
        }

        // ===================== 2) SECTION DETAILS =====================
        public async Task<IActionResult> Section(long id)
        {
            var doctor = await _userManager.GetUserAsync(User);

            var section = await _context.CourseSections
                .Include(s => s.Course)
                    .ThenInclude(c => c.Department)
                .FirstOrDefaultAsync(s => s.CourseSectionID == id && s.DoctorUserID == doctor.Id);

            if (section == null)
            {
                TempData["msg"] = "Section not found or not assigned to you.";
                return RedirectToAction(nameof(Index));
            }

            // ❗ جلب الطلاب من جدول StudentEnrollments
            var enrolledStudents = await _context.StudentEnrollments
                .Where(e => e.CourseSectionID == id)
                .ToListAsync();

            // ❗ حل المشكلة: جلب الطالب يدوياً باستخدام StudentUserID
            foreach (var item in enrolledStudents)
            {
                item.Student = await _context.Users
                    .FirstOrDefaultAsync(u => u.Id == item.StudentUserID);
            }

            // حساب الغيابات
            var absenceCounts = await _context.AttendanceLogs
                .Where(a => a.CourseSectionID == id && a.PresenceStatus == 0)
                .GroupBy(a => a.StudentUserID)
                .ToDictionaryAsync(g => g.Key, g => g.Count());

            ViewBag.AbsenceCounts = absenceCounts;
            ViewBag.EnrolledStudents = enrolledStudents;

            return View(section);
        }



        // ===================== 3) SAVE ATTENDANCE (checkbox) =====================
        [HttpPost]
        public async Task<IActionResult> SaveAttendance(long sectionId, List<StudentAttendanceInput> students)
        {
            var doctor = await _userManager.GetUserAsync(User);

            var section = await _context.CourseSections
                .FirstOrDefaultAsync(s => s.CourseSectionID == sectionId && s.DoctorUserID == doctor.Id);

            if (section == null)
            {
                TempData["msg"] = "Section not found or not assigned to you.";
                return RedirectToAction(nameof(Index));
            }

            var today = DateTime.UtcNow.Date;

            foreach (var s in students)
            {
                // 1 = حاضر, 0 = غائب
                byte presence = s.IsPresent ? (byte)1 : (byte)0;

                var existing = await _context.AttendanceLogs
                    .FirstOrDefaultAsync(a =>
                        a.CourseSectionID == sectionId &&
                        a.StudentUserID == s.StudentUserID &&
                        a.AttendanceDate == today);

                if (existing == null)
                {
                    var log = new AttendanceLog
                    {
                        CourseSectionID = sectionId,
                        StudentUserID = s.StudentUserID,
                        AttendanceDate = today,
                        PresenceStatus = presence,
                        AttendanceMethod = 3, // يدوي / checkbox
                        VerifiedByUserID = doctor.Id,
                        ScanTimestamp = DateTime.UtcNow
                    };

                    _context.AttendanceLogs.Add(log);
                }
                else
                {
                    existing.PresenceStatus = presence;
                    existing.AttendanceMethod = 3;
                    existing.VerifiedByUserID = doctor.Id;
                    existing.ScanTimestamp = DateTime.UtcNow;

                    _context.AttendanceLogs.Update(existing);
                }
            }

            await _context.SaveChangesAsync();

            TempData["msg"] = "Attendance saved successfully.";
            return RedirectToAction(nameof(Section), new { id = sectionId });
        }


        // ===================== 4) DOWNLOAD "EXCEL" (CSV) =====================
        [HttpGet]
        public async Task<IActionResult> DownloadExcel(long id)
        {
            // 1) جلب بيانات الشعبة
            var section = await _context.CourseSections
                .Include(s => s.Course)
                .FirstOrDefaultAsync(s => s.CourseSectionID == id);

            if (section == null)
                return NotFound();

            // 2) جلب الطلاب من StudentEnrollments
            var enrolledStudents = await _context.StudentEnrollments
                .Where(e => e.CourseSectionID == id)
                .ToListAsync();

            // 3) جلب بيانات الطالب من AspNetUsers
            foreach (var e in enrolledStudents)
            {
                e.Student = await _context.Users
                    .FirstOrDefaultAsync(u => u.Id == e.StudentUserID);
            }

            // 4) حساب الغيابات
            var absenceCounts = await _context.AttendanceLogs
                .Where(a => a.CourseSectionID == id && a.PresenceStatus == 0)
                .GroupBy(a => a.StudentUserID)
                .ToDictionaryAsync(g => g.Key, g => g.Count());

            // 5) جلب حضور اليوم
            var today = DateTime.UtcNow.Date;
            var todayAttendance = await _context.AttendanceLogs
                .Where(a => a.CourseSectionID == id && a.AttendanceDate == today)
                .ToDictionaryAsync(a => a.StudentUserID, a => a.PresenceStatus);

            // 6) بناء ملف CSV
            var sb = new StringBuilder();
            sb.AppendLine("ID,Name,IDNumber,Email,Absences,Present");

            foreach (var e in enrolledStudents)
            {
                if (e.Student == null)
                    continue;

                // الغياب
                int abs = absenceCounts.ContainsKey(e.StudentUserID)
                    ? absenceCounts[e.StudentUserID]
                    : 0;

                // حضور اليوم
                byte present = todayAttendance.ContainsKey(e.StudentUserID)
                    ? todayAttendance[e.StudentUserID]
                    : (byte)0;

                sb.AppendLine(
                    $"{e.StudentUserID}," +
                    $"{e.Student.FirstName} {e.Student.LastName}," +
                    $"{e.Student.IDNumber}," +
                    $"{e.Student.Email}," +
                    $"{abs}," +
                    $"{present}");
            }

            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            var fileName = $"Section_{id}_Students.csv";

            return File(bytes, "text/csv", fileName);
        }


        [HttpPost]
        public async Task<IActionResult> UploadExcel(long id, IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                TempData["msg"] = "Please upload a valid CSV file.";
                return RedirectToAction("Section", new { id });
            }

            var section = await _context.CourseSections
                .Include(s => s.Enrollments)
                .FirstOrDefaultAsync(s => s.CourseSectionID == id);

            if (section == null)
            {
                TempData["msg"] = "Section not found.";
                return RedirectToAction("Index");
            }

            using var stream = new StreamReader(file.OpenReadStream());
            int lineNo = 0;
            var today = DateTime.UtcNow.Date;

            while (!stream.EndOfStream)
            {
                var line = await stream.ReadLineAsync();
                lineNo++;

                if (lineNo == 1) continue; // skip header

                var data = line.Split(',');
                if (data.Length < 6) continue;

                string studentId = data[0];

                // Absences
                int absenceCount = int.TryParse(data[4], out var absVal) ? absVal : 0;

                // Present (0 or 1)
                byte present = byte.TryParse(data[5], out var pVal) ? pVal : (byte)0;

                // Check student is enrolled
                var studentEnrollment = section.Enrollments
                    .FirstOrDefault(e => e.StudentUserID == studentId);

                if (studentEnrollment == null)
                    continue;

                // --- remove old logs ---
                var logs = await _context.AttendanceLogs
                    .Where(a => a.CourseSectionID == id && a.StudentUserID == studentId)
                    .ToListAsync();

                _context.AttendanceLogs.RemoveRange(logs);

                // --- insert absences ---
                for (int x = 0; x < absenceCount; x++)
                {
                    _context.AttendanceLogs.Add(new AttendanceLog
                    {
                        CourseSectionID = id,
                        StudentUserID = studentId,
                        PresenceStatus = 0,
                        AttendanceDate = DateTime.UtcNow.Date.AddDays(-(x + 1)), // الغيابات ليست اليوم
                        AttendanceMethod = 4,
                        ScanTimestamp = DateTime.UtcNow
                    });
                }

                // --- insert today's attendance ---
                _context.AttendanceLogs.Add(new AttendanceLog
                {
                    CourseSectionID = id,
                    StudentUserID = studentId,
                    PresenceStatus = present,  // 1 حاضر - 0 غائب
                    AttendanceDate = today,
                    AttendanceMethod = 4,
                    ScanTimestamp = DateTime.UtcNow
                });

                await _context.SaveChangesAsync();
            }

            TempData["msg"] = "Excel uploaded and attendance updated.";
            return RedirectToAction("Section", new { id });
        }

        // ===================== 5) GENERATE QR (GUID) =====================
        [HttpGet]
        public IActionResult GenerateQr(long sectionId)
        {
            // payload that student app will read from QR
            var payload = new
            {
                SectionId = sectionId,
                Code = Guid.NewGuid().ToString(),
                GeneratedAt = DateTime.UtcNow
            };

            return Json(payload);
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("api/attendance/mark")]
        public async Task<IActionResult> MarkAttendanceByQr([FromBody] AttendanceScanRequest model)
        {
            if (model == null)
                return BadRequest("Invalid request");

            if (string.IsNullOrWhiteSpace(model.Code))
                return BadRequest("Invalid QR code");

            // Check if student is enrolled 
            var isEnrolled = await _context.StudentEnrollments
                .AnyAsync(e =>
                    e.CourseSectionID == model.SectionId &&
                    e.StudentUserID == model.StudentUserID);

            if (!isEnrolled)
                return Unauthorized("Student not enrolled in this section.");

            var today = DateTime.UtcNow.Date;

            var existing = await _context.AttendanceLogs
                .FirstOrDefaultAsync(a =>
                    a.CourseSectionID == model.SectionId &&
                    a.StudentUserID == model.StudentUserID &&
                    a.AttendanceDate == today);

            if (existing == null)
            {
                _context.AttendanceLogs.Add(new AttendanceLog
                {
                    CourseSectionID = model.SectionId,
                    StudentUserID = model.StudentUserID,
                    AttendanceDate = today,
                    PresenceStatus = 1,
                    AttendanceMethod = 1, // mobile scan
                    ScanTimestamp = DateTime.UtcNow,
                    QrCode = model.Code
                });
            }
            else
            {
                existing.PresenceStatus = 1;
                existing.AttendanceMethod = 1;
                existing.ScanTimestamp = DateTime.UtcNow;
                existing.QrCode = model.Code;
                _context.AttendanceLogs.Update(existing);
            }

            await _context.SaveChangesAsync();

            return Ok(new { Message = "Attendance marked", Status = 1 });
        }

        [HttpPost]
        [Route("api/attendance/upload-excuse")]
        [AllowAnonymous]
        public async Task<IActionResult> UploadExcuse([FromForm] ExcuseUploadRequest model)
        {
            if (model == null || model.File == null)
                return BadRequest("Invalid request");

            // 1) التأكد أن الطالب مسجل في الشعبة
            bool enrolled = await _context.StudentEnrollments
                .AnyAsync(e => e.CourseSectionID == model.SectionId &&
                               e.StudentUserID == model.StudentUserID);

            if (!enrolled)
                return Unauthorized("Student not enrolled in this section.");

            // 2) البحث عن آخر يوم غياب للطالب في هذه الشعبة (يوم واحد أو أكثر)
            var log = await _context.AttendanceLogs
                .Where(a => a.CourseSectionID == model.SectionId &&
                            a.StudentUserID == model.StudentUserID &&
                            a.PresenceStatus == 0) // 0 = absent
                .OrderByDescending(a => a.AttendanceDate)
                .FirstOrDefaultAsync();

            if (log == null)
                return BadRequest("No absence record found. You can only upload excuse for an absence.");

            // 3) تجهيز مسار التخزين الجديد
            string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");

            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            string extension = Path.GetExtension(model.File.FileName).ToLower();
            var allowed = new[] { ".pdf", ".png", ".jpg", ".jpeg" };

            if (!allowed.Contains(extension))
                return BadRequest("Only PDF or image files are allowed.");

            string fileName = $"{Guid.NewGuid()}{extension}";
            string savePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(savePath, FileMode.Create))
            {
                await model.File.CopyToAsync(stream);
            }

            // 4) تعديل السجل الموجود مسبقاً
            log.ExcuseDocumentPath = $"/uploads/{fileName}";
            log.IsExcused = true;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = "Excuse uploaded successfully",
                Status = 1,
                FilePath = log.ExcuseDocumentPath
            });
        }

        [HttpGet]
        [Route("api/attendance/details")]
        public async Task<IActionResult> GetAbsenceDetails(string studentId, long sectionId)
        {
            if (string.IsNullOrWhiteSpace(studentId))
                return BadRequest("Invalid student ID");

            // 1) Logs list
            var logs = await _context.AttendanceLogs
                .Where(a => a.StudentUserID == studentId && a.CourseSectionID == sectionId)
                .OrderByDescending(a => a.AttendanceDate)
                .Select(a => new
                {
                    a.AttendanceLogID,
                    a.AttendanceDate,
                    a.PresenceStatus,
                    a.AttendanceMethod,
                    a.QrCode,
                    a.ScanTimestamp,
                    a.IsExcused,
                    a.ExcuseDocumentPath
                })
                .ToListAsync();

            // 2) Student info
            var student = await _context.Users
                .Where(u => u.Id == studentId)
                .Select(u => new
                {
                    u.Id,
                    u.FirstName,
                    u.LastName,
                    u.Email,
                    u.IDNumber
                })
                .FirstOrDefaultAsync();

            if (student == null)
                return NotFound("Student not found.");

            // 3) Section info
            var section = await _context.CourseSections
                .Include(s => s.Course)
                .Where(s => s.CourseSectionID == sectionId)
                .Select(s => new
                {
                    s.CourseSectionID,
                    s.SectionNumber,
                    CourseName = s.Course.CourseName,
                    DepartmentName = s.Course.Department.DepartmentName
                })
                .FirstOrDefaultAsync();

            if (section == null)
                return NotFound("Section not found.");

            return Ok(new
            {
                Student = student,
                Section = section,
                AttendanceLogs = logs
            });
        }


        public async Task<IActionResult> AbsenceDetails(string studentId, long sectionId)
        {
            var logs = await _context.AttendanceLogs
                .Where(a => a.StudentUserID == studentId && a.CourseSectionID == sectionId)
                .OrderByDescending(a => a.AttendanceDate)
                .ToListAsync();

            ViewBag.Student = await _context.Users.FirstOrDefaultAsync(u => u.Id == studentId);
            ViewBag.Section = await _context.CourseSections
                .Include(s => s.Course)
                .FirstOrDefaultAsync(s => s.CourseSectionID == sectionId);

            return View(logs);
        }

        [HttpPost]
        [Area("Doctor")]
        public async Task<IActionResult> UpdateExcuses(string studentId, long sectionId, List<AttendanceLog> logs)
        {
            foreach (var log in logs)
            {
                var record = await _context.AttendanceLogs
                    .FirstOrDefaultAsync(a => a.AttendanceLogID == log.AttendanceLogID);

                if (record != null)
                {
                    record.IsExcused = log.IsExcused;
                }
            }

            await _context.SaveChangesAsync();

            TempData["msg"] = "Excuse status updated successfully!";
            return RedirectToAction("AbsenceDetails", new { studentId = studentId, sectionId = sectionId });
        }

    }
}
