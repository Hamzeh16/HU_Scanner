using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScannerDataAccess.Data;
using ScannerModels.Model;

namespace ScannerWeb.Controllers.Api
{
    [ApiController]
    [Route("api/student")]
    //[Authorize(Roles = "Student")]
    public class StudentController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public StudentController(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ============================================
        // 1) Get my course sections
        // ============================================
        [HttpGet("sections")]
        public async Task<IActionResult> GetMySections()
        {
            var student = await _userManager.GetUserAsync(User);

            var sections = await _context.StudentEnrollments
                .Where(e => e.StudentUserID == student.Id)
                .Select(e => new
                {
                    e.CourseSection.CourseSectionID,
                    e.CourseSection.SectionNumber,
                    CourseName = e.CourseSection.Course.CourseName,
                    Department = e.CourseSection.Course.Department.DepartmentName,
                    DoctorName = e.CourseSection.Doctor.FirstName + " " + e.CourseSection.Doctor.LastName
                })
                .ToListAsync();

            return Ok(sections);
        }


        // ============================================
        // 2) Get attendance summary for a section
        // ============================================
        [HttpGet("attendance/{sectionId:long}")]
        public async Task<IActionResult> GetAttendance(long sectionId)
        {
            var student = await _userManager.GetUserAsync(User);

            var logs = await _context.AttendanceLogs
                .Where(a => a.CourseSectionID == sectionId && a.StudentUserID == student.Id)
                .OrderBy(a => a.AttendanceDate)
                .ToListAsync();

            if (!logs.Any())
            {
                return Ok(new
                {
                    totalDays = 0,
                    absences = 0,
                    attendancePercent = 0,
                    details = new List<object>()
                });
            }

            int totalDays = logs.Select(a => a.AttendanceDate.Date).Distinct().Count();
            int absences = logs.Count(a => a.PresenceStatus == 0);
            int presents = logs.Count(a => a.PresenceStatus == 1);

            int attendancePercent = (int)((presents / (double)totalDays) * 100);

            var details = logs.Select(a => new
            {
                date = a.AttendanceDate.ToString("yyyy-MM-dd"),
                isPresent = a.PresenceStatus == 1,
                isExcused = a.IsExcused,
                file = a.ExcuseDocumentPath
            });

            return Ok(new
            {
                totalDays,
                absences,
                attendancePercent,
                details
            });
        }


        // ============================================
        // 3) Mark attendance using QR (from app)
        // ============================================
        [HttpPost("mark-attendance")]
        [AllowAnonymous]  // إذا التطبيق بدون Login
        public async Task<IActionResult> MarkAttendanceByQr([FromBody] AttendanceScanRequest model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.Code))
                return BadRequest("Invalid request");

            // Validate enrollment
            bool enrolled = await _context.StudentEnrollments
                .AnyAsync(e => e.CourseSectionID == model.SectionId &&
                               e.StudentUserID == model.StudentUserID);

            if (!enrolled)
                return Unauthorized("Student not enrolled.");

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
                    AttendanceMethod = 1,
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
            }

            await _context.SaveChangesAsync();

            return Ok(new { Message = "Attendance marked", Status = 1 });
        }


        // ============================================
        // 4) Upload excuse file (image/pdf)
        // ============================================
        [HttpPost("upload-excuse")]
        public async Task<IActionResult> UploadExcuse([FromForm] ExcuseUploadRequest model)
        {
            if (model == null || model.File == null)
                return BadRequest("Invalid file");

            bool enrolled = await _context.StudentEnrollments
                .AnyAsync(e => e.CourseSectionID == model.SectionId &&
                               e.StudentUserID == model.StudentUserID);

            if (!enrolled)
                return Unauthorized("Not enrolled");

            var log = await _context.AttendanceLogs
                .Where(a => a.CourseSectionID == model.SectionId &&
                            a.StudentUserID == model.StudentUserID &&
                            a.PresenceStatus == 0)
                .OrderByDescending(a => a.AttendanceDate)
                .FirstOrDefaultAsync();

            if (log == null)
                return BadRequest("No absence found");

            string uploads = Path.Combine("wwwroot", "uploads", "excuses");
            Directory.CreateDirectory(uploads);

            string ext = Path.GetExtension(model.File.FileName).ToLower();
            var allowed = new[] { ".pdf", ".jpg", ".jpeg", ".png" };
            if (!allowed.Contains(ext))
                return BadRequest("Invalid file type");

            string fileName = $"{Guid.NewGuid()}{ext}";
            string path = Path.Combine(uploads, fileName);

            using (var stream = new FileStream(path, FileMode.Create))
                await model.File.CopyToAsync(stream);

            log.IsExcused = true;
            log.ExcuseDocumentPath = $"/uploads/excuses/{fileName}";

            await _context.SaveChangesAsync();

            return Ok(new { Message = "Uploaded", File = log.ExcuseDocumentPath });
        }


        // ============================================
        // 5) Get absence details
        // ============================================
        [HttpGet("details")]
        public async Task<IActionResult> GetAbsenceDetails(string studentId, long sectionId)
        {
            var logs = await _context.AttendanceLogs
                .Where(a => a.StudentUserID == studentId && a.CourseSectionID == sectionId)
                .OrderByDescending(a => a.AttendanceDate)
                .ToListAsync();

            return Ok(logs);
        }


        // ============================================
        // 6) Reset password (API for student)
        // ============================================
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDTO model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.Email) || string.IsNullOrWhiteSpace(model.NewPassword))
                return BadRequest("Invalid request.");

            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null || user.TypeUser != "Student")
                return NotFound("Student not found.");

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            var result = await _userManager.ResetPasswordAsync(user, token, model.NewPassword);

            if (!result.Succeeded)
                return BadRequest("Error resetting password.");

            return Ok(new
            {
                Message = "Password reset successfully",
                User = user.Email
            });
        }

    }
}
