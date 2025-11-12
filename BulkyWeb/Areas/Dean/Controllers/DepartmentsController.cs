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

            // Get all Heads and Doctors
            var heads = await _userManager.GetUsersInRoleAsync("HeadOfDepartment");
            var doctors = await _userManager.GetUsersInRoleAsync("Doctor");

            // Merge into one list and tag their role
            var allStaff = heads.Select(u => new
            {
                u.FirstName,
                u.LastName,
                u.Email,
                u.ManagedDepartment,
                Role = "Head of Department"
            })
            .Concat(doctors.Select(u => new
            {
                u.FirstName,
                u.LastName,
                u.Email,
                u.ManagedDepartment,
                Role = "Doctor"
            }))
            .ToList();

            ViewBag.Staff = allStaff;
            return View(departments);
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

            if (dept.HeadUserID != null)
            {
                // Find the current head user
                var headUser = await _userManager.FindByIdAsync(dept.HeadUserID);

                if (headUser != null)
                {
                    // Remove HeadOfDepartment role
                    if (await _userManager.IsInRoleAsync(headUser, "HeadOfDepartment"))
                    {
                        await _userManager.RemoveFromRoleAsync(headUser, "HeadOfDepartment");
                    }

                    // Reassign as Doctor
                    if (!await _userManager.IsInRoleAsync(headUser, "Doctor"))
                    {
                        await _userManager.AddToRoleAsync(headUser, "Doctor");
                    }
                }

                // Remove head reference from department
                dept.HeadUserID = null;
                _context.Update(dept);
                await _context.SaveChangesAsync();

                TempData["msg"] = $"Head of Department '{headUser?.FirstName} {headUser?.LastName}' reverted to Doctor successfully.";
            }
            else
            {
                TempData["msg"] = "No Head assigned to this department.";
            }

            return RedirectToAction(nameof(Index));
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

            // Remove old head if exists
            if (department.HeadUserID != null)
            {
                var oldHead = await _userManager.FindByIdAsync(department.HeadUserID);
                if (oldHead != null)
                {
                    await _userManager.RemoveFromRoleAsync(oldHead, "HeadOfDepartment");
                    await _userManager.AddToRoleAsync(oldHead, "Doctor");
                }
            }

            // Assign this doctor as the new head
            department.HeadUserID = doctor.Id;
            _context.Update(department);
            await _context.SaveChangesAsync();

            // Update doctor’s role
            await _userManager.RemoveFromRoleAsync(doctor, "Doctor");
            await _userManager.AddToRoleAsync(doctor, "HeadOfDepartment");

            TempData["msg"] = $"{doctor.FirstName} {doctor.LastName} has been assigned as Head of {department.DepartmentName}.";
            return RedirectToAction(nameof(Index));
        }

    }

}
