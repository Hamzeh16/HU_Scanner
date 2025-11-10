// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

//using BulkyBookUtility;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.WebUtilities;
using ScannerModels.Model;
using ScannerUtility;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;

namespace BulkyWeb.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserStore<ApplicationUser> _userStore;
        private readonly IUserEmailStore<ApplicationUser> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;

        public RegisterModel(
            UserManager<ApplicationUser> userManager,
            IUserStore<ApplicationUser> userStore,
            RoleManager<IdentityRole> roleManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender)
        {
            _userManager = userManager;
            _userStore = userStore;
            _roleManager = roleManager;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {

            [EmailAddress]

            public string Email { get; set; }





            public string Password { get; set; }




            public string ConfirmPassword { get; set; }

            // هذا الحقل لـ Dropdown وهو صحيح
            public string Role { get; set; }
            [ValidateNever]
            public IEnumerable<SelectListItem> RoleList { get; set; }

            // --- الخصائص المخصصة التي نجمعها من النموذج ---
            // هذه صحيحة 100%

            // يجب إضافة Required
            public string FirstName { get; set; }

            // يجب إضافة Required
            public string LastName { get; set; }

            // يجب إضافة Required
            public string IDNumber { get; set; } // هذا هو الرقم الجامعي/الوظيفي
            public string PhoneNumber { get; set; }
            // هذا صحيح لرفع الملف
            public IFormFile? ProfilePictureUrl { get; set; }

            // هذه لا نحتاجها في النموذج، سيتم تعيينها تلقائياً
            // public DateTime DateJoined { get; set; } = DateTime.UtcNow;

            // 
            //!!! قم بإزالة كل ما يلي من InputModel!!!
            // هذه خصائص قاعدة بيانات، وليست خصائص نموذج تسجيل
            //
            // public virtual College ManagedCollege { get; set; }
            // public virtual Department ManagedDepartment { get; set; }
            // public virtual ICollection<CourseSection> TaughtSections { get; set; }
            // public virtual ICollection<StudentEnrollment> Enrollments { get; set; }
            // public virtual ICollection<AttendanceLog> AttendanceLogs { get; set; }
        }


        public async Task OnGetAsync(string returnUrl = null)
        {
            if (!_roleManager.RoleExistsAsync(SD.Role_Docter).GetAwaiter().GetResult())
            {
                await _roleManager.CreateAsync(new IdentityRole(SD.Role_HR));
                await _roleManager.CreateAsync(new IdentityRole(SD.Role_Dean));
                await _roleManager.CreateAsync(new IdentityRole(SD.Role_HeadofDepartment));
                await _roleManager.CreateAsync(new IdentityRole(SD.Role_Docter));
                await _roleManager.CreateAsync(new IdentityRole(SD.Role_Student));
            }

            Input = new()
            {
                RoleList = _roleManager.Roles.Select(x => x.Name).Select(i => new SelectListItem
                {
                    Text = i,
                    Value = i,
                })
            };

            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            // أعد ملء قائمة الأدوار في حالة فشل الإرسال
            Input.RoleList = _roleManager.Roles.Select(x => x.Name).Select(i => new SelectListItem
            {
                Text = i,
                Value = i,
            });

            if (ModelState.IsValid)
            {
                var user = CreateUser();

                // --- منطق رفع الصورة (صحيح) ---
                // تم تعديله قليلاً للتحقق من وجود ملف
                if (Input.ProfilePictureUrl != null)
                {
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");

                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    // استخدام اسم ملف فريد لتجنب التعارض
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(Input.ProfilePictureUrl.FileName);
                    var filePath = Path.Combine(uploadsFolder, fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await Input.ProfilePictureUrl.CopyToAsync(stream);
                    }

                    var imageUrl = Path.Combine("/uploads", fileName).Replace("\\", "/");
                    user.ProfilePictureUrl = imageUrl;
                }

                await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);

                // --- نقل البيانات من InputModel إلى ApplicationUser (صحيح 100%) ---
                user.FirstName = Input.FirstName;
                user.LastName = Input.LastName;
                user.Email = Input.Email;
                user.IDNumber = Input.IDNumber;
                user.PhoneNumber = Input.PhoneNumber;
                user.DateJoined = DateTime.UtcNow; // قم بتعيينها هنا، وليس من النموذج
                user.TypeUser = Input.Role;

                //
                //!!! الأسطر التي كانت هنا (الأسطر المعلقة) يتم حذفها.
                // لا نحتاج إلى تعيين TaughtSections أو Enrollments هنا.
                //

                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                    if (!string.IsNullOrEmpty(Input.Role))
                    {
                        await _userManager.AddToRoleAsync(user, Input.Role);
                    }
                    else
                    {
                        // تعيين دور افتراضي إذا لم يتم اختيار أي شيء
                        await _userManager.AddToRoleAsync(user, SD.Role_Student);
                    }

                    var userId = await _userManager.GetUserIdAsync(user);
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
                        protocol: Request.Scheme);

                    await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                        $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
                    }
                    else
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return LocalRedirect(returnUrl);
                    }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // إذا وصلنا إلى هنا، فشل شيء ما، أعد عرض النموذج
            return Page();
        }

        private ApplicationUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<ApplicationUser>();
            }
            catch
            {
                // قم بتغيير nameof(IdentityUser) إلى nameof(ApplicationUser) هنا
                throw new InvalidOperationException($"Can't create an instance of '{nameof(ApplicationUser)}'. " +
                    $"Ensure that '{nameof(ApplicationUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }

        private IUserEmailStore<ApplicationUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<ApplicationUser>)_userStore;
        }
    }
}
