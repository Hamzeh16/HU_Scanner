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
    public class DepartmentDashboardController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public DepartmentDashboardController(AppDbContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
        }
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

            ViewBag.DepartmentName = department.DepartmentName;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }
    }
}
