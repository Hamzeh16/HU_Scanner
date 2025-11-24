using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScannerDataAccess.Data;
using ScannerModels.Model;

namespace ScannerWeb.Areas.Dean.Controllers
{
    [Area("Dean")]
    [Authorize(Roles = "Dean")]
    public class DepartmentsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DepartmentsController(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var departments = await _context.Departments
                .Include(d => d.College)
                .Include(d => d.Head)
                .ToListAsync();

            var staff = await _userManager.Users
                .Where(u => u.TypeUser == "Doctor" || u.TypeUser == "HeadOfDepartment")
                .Select(u => new
                {
                    u.FirstName,
                    u.LastName,
                    u.Email,
                    u.TypeUser,
                    ManagedDepartment = u.ManagedDepartment,
                    Role = u.TypeUser == "HeadOfDepartment" ? "Head of Department" : "Doctor"
                })
                .ToListAsync();

            ViewBag.Staff = staff;
            ViewBag.Departments = departments;

            return View(departments);
        }


        [HttpPost]
        public async Task<IActionResult> AssignDoctorAsHead(string userEmail, int departmentId)
        {
            var doctor = await _userManager.FindByEmailAsync(userEmail);
            if (doctor == null)
            {
                TempData["msg"] = "Doctor not found.";
                return RedirectToAction(nameof(Index));
            }

            var department = await _context.Departments.FindAsync(departmentId);
            if (department == null)
            {
                TempData["msg"] = "Department not found.";
                return RedirectToAction(nameof(Index));
            }

            // ---- Remove previous head if exists ----
            if (department.HeadUserID != null)
            {
                var oldHead = await _userManager.FindByIdAsync(department.HeadUserID);
                if (oldHead != null)
                {
                    // Remove HeadOfDepartment role
                    await _userManager.RemoveFromRoleAsync(oldHead, "HeadOfDepartment");

                    // Add Doctor role
                    if (!await _userManager.IsInRoleAsync(oldHead, "Doctor"))
                        await _userManager.AddToRoleAsync(oldHead, "Doctor");

                    // Update TypeUser
                    oldHead.TypeUser = "Doctor";

                    // Remove his admin link to the department
                    oldHead.ManagedDepartment = null;
                    _context.Users.Update(oldHead);
                }
            }

            // ---- Assign new head ----
            department.HeadUserID = doctor.Id;
            _context.Update(department);

            // Update doctor roles
            await _userManager.RemoveFromRoleAsync(doctor, "Doctor");
            await _userManager.AddToRoleAsync(doctor, "HeadOfDepartment");

            // Update TypeUser
            doctor.TypeUser = "HeadOfDepartment";

            // Update navigation reference
            doctor.ManagedDepartment = department;

            _context.Users.Update(doctor);
            await _context.SaveChangesAsync();

            TempData["msg"] = $"{doctor.FirstName} {doctor.LastName} has been assigned as Head of {department.DepartmentName}.";

            return RedirectToAction(nameof(Index));
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveHead(int departmentId)
        {
            var dept = await _context.Departments.FindAsync(departmentId);
            if (dept == null)
            {
                TempData["msg"] = "Department not found.";
                return RedirectToAction(nameof(Index));
            }

            if (dept.HeadUserID == null)
            {
                TempData["msg"] = "No head assigned to this department.";
                return RedirectToAction(nameof(Index));
            }

            var headUser = await _userManager.FindByIdAsync(dept.HeadUserID);
            if (headUser != null)
            {
                // Update roles
                if (await _userManager.IsInRoleAsync(headUser, "HeadOfDepartment"))
                    await _userManager.RemoveFromRoleAsync(headUser, "HeadOfDepartment");

                if (!await _userManager.IsInRoleAsync(headUser, "Doctor"))
                    await _userManager.AddToRoleAsync(headUser, "Doctor");

                // Update TypeUser
                headUser.TypeUser = "Doctor";

                // Clear navigation link
                headUser.ManagedDepartment = null;

                _context.Users.Update(headUser);
            }

            // Remove head from the department
            dept.HeadUserID = null;
            _context.Update(dept);

            await _context.SaveChangesAsync();

            TempData["msg"] = $"Head of Department '{headUser?.FirstName} {headUser?.LastName}' reverted to Doctor.";

            return RedirectToAction(nameof(Index));
        }

    }

}
