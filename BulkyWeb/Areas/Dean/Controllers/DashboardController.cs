using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ScannerDataAccess.Data;
using ScannerModels.Model;

namespace ScannerWeb.Areas.Dean.Controllers
{
    [Area("Dean")]
    [Authorize(Roles = "Dean")]
    public class DashboardController : Controller
    {
        private readonly AppDbContext _context;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public DashboardController(AppDbContext context, SignInManager<ApplicationUser> signInManager)
        {
            _context = context;
            _signInManager = signInManager;
        }

        public IActionResult Index()
        {
            var model = new DeanDashboardVM
            {
                DepartmentsCount = _context.Departments.Count(),
                HeadsCount = _context.Departments.Count(d => d.HeadUserID != null),
                DoctorsCount = _context.Users.Count(u => u.TypeUser == "Doctor"),
                StudentsCount = _context.Users.Count(u => u.TypeUser == "Student")
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }

    }

}
