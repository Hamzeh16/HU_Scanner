using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScannerDataAccess.Data;
using ScannerModels.Model;

namespace ScannerWeb.Areas.HeadOfDepartment.Controllers
{
    [Area("HeadOfDepartment")]
    [Authorize(Roles = "HeadOfDepartment")]
    public class SectionsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public SectionsController(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        //public async Task<IActionResult> Index()
        //{
        //    var hod = await _userManager.GetUserAsync(User);

        //    // احضر القسم الذي يديره الرئيس
        //    var department = await _context.Departments
        //        .FirstOrDefaultAsync(d => d.HeadUserID == hod.Id);

        //    if (department == null)
        //    {
        //        TempData["msg"] = "You are not assigned to any department yet.";
        //        return RedirectToAction("Index", "Dashboard");
        //    }

        //    int departmentId = department.DepartmentID;

        //    // هنا المشكلة — يجب Include Doctor
        //    var sections = await _context.CourseSections
        //        .Include(s => s.Course)
        //        .Include(s => s.Doctor)
        //        .Where(s => s.Course.DepartmentID == departmentId)
        //        .ToListAsync();

        //    // احضر دكاترة نفس القسم فقط
        //    var doctors = await _userManager.Users
        //        .Include(u => u.ManagedDepartment)
        //        .Where(u =>
        //            u.TypeUser == "Doctor" ||
        //            u.TypeUser == "HeadOfDepartment"
        //        )
        //        .ToListAsync();

        //    ViewBag.Doctors = doctors;

        //    return View(sections);
        //}


        //// ------------------- ASSIGN DOCTOR -------------------
        //[HttpPost]
        //public async Task<IActionResult> AssignDoctor(long sectionId, string doctorId)
        //{
        //    var section = await _context.CourseSections
        //        .FirstOrDefaultAsync(s => s.CourseSectionID == sectionId);

        //    if (section == null)
        //    {
        //        TempData["msg"] = "Section not found.";
        //        return RedirectToAction("Index");
        //    }

        //    section.DoctorUserID = doctorId;
        //    await _context.SaveChangesAsync();

        //    // reload navigation
        //    await _context.Entry(section).Reference(s => s.Doctor).LoadAsync();

        //    TempData["msg"] = "Doctor assigned successfully.";
        //    return RedirectToAction("Index");
        //}


        //// ------------------- REMOVE DOCTOR -------------------
        //[HttpPost]
        //public async Task<IActionResult> RemoveDoctor(long sectionId)
        //{
        //    var section = await _context.CourseSections
        //        .FirstOrDefaultAsync(s => s.CourseSectionID == sectionId);

        //    if (section == null)
        //    {
        //        TempData["msg"] = "Section not found.";
        //        return RedirectToAction("Index");
        //    }

        //    section.DoctorUserID = null;
        //    await _context.SaveChangesAsync();

        //    TempData["msg"] = "Doctor removed from section.";
        //    return RedirectToAction("Index");
        //}

        public async Task<IActionResult> Index()
        {
            var hod = await _userManager.GetUserAsync(User);

            var department = await _context.Departments
                .FirstOrDefaultAsync(d => d.HeadUserID == hod.Id);

            if (department == null)
            {
                TempData["msg"] = "You are not assigned to any department.";
                return RedirectToAction("Index", "Dashboard");
            }

            int deptId = department.DepartmentID;

            // Get sections from database
            var sections = await _context.CourseSections
                .Include(s => s.Course)
                .Where(s => s.Course.DepartmentID == deptId)
                .ToListAsync();

            // Build ViewModel
            List<SectionVM> model = new();

            foreach (var sec in sections)
            {
                ApplicationUser? doctor = null;

                if (!string.IsNullOrEmpty(sec.DoctorUserID))
                {
                    doctor = await _userManager.FindByIdAsync(sec.DoctorUserID);
                }

                model.Add(new SectionVM
                {
                    SectionID = sec.CourseSectionID,
                    CourseName = sec.Course.CourseName,
                    SectionNumber = sec.SectionNumber,
                    DoctorUserID = sec.DoctorUserID,
                    DoctorFullName = doctor == null ? null : $"{doctor.FirstName} {doctor.LastName}"
                });
            }

            // Get all doctors normally
            var doctors = await _userManager.Users
                .Where(u => u.TypeUser == "Doctor" || u.TypeUser == "HeadOfDepartment")
                .ToListAsync();

            ViewBag.Doctors = doctors;

            return View(model);
        }




        [HttpPost]
        public async Task<IActionResult> AssignDoctor(long sectionId, string doctorId)
        {
            var section = await _context.CourseSections
                .Include(s => s.Doctor)
                .FirstOrDefaultAsync(s => s.CourseSectionID == sectionId);

            if (section == null)
            {
                TempData["msg"] = "Section not found.";
                return RedirectToAction("Index");
            }

            section.DoctorUserID = doctorId;
            await _context.SaveChangesAsync();

            await _context.Entry(section).Reference(s => s.Doctor).LoadAsync();

            TempData["msg"] = "Doctor assigned successfully.";
            return RedirectToAction("Index");
        }



        [HttpPost]
        public async Task<IActionResult> RemoveDoctor(long sectionId)
        {
            var section = await _context.CourseSections
                .Include(s => s.Doctor)
                .FirstOrDefaultAsync(s => s.CourseSectionID == sectionId);

            if (section == null)
            {
                TempData["msg"] = "Section not found.";
                return RedirectToAction("Index");
            }

            section.DoctorUserID = null;
            await _context.SaveChangesAsync();

            TempData["msg"] = "Doctor removed.";
            return RedirectToAction("Index");
        }


    }
}
