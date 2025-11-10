using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using ScannerDataAccess.Data;
using ScannerModels.Model;
using ScannerUtility;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("MyConnection")));

//builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddIdentity<ApplicationUser, IdentityRole>().AddEntityFrameworkStores<AppDbContext>().AddDefaultTokenProviders();
//builder.Services.ConfigureApplicationCookie(options => {
//    options.LoginPath = $"/Identity/Account/Login";
//    options.LogoutPath = $"/Identity/Account/Logout";
//    options.AccessDeniedPath = $"/Identity/Account/AccessDenied";
//});
builder.Services.ConfigureApplicationCookie(option =>
{
    option.LoginPath = $"/Identity/Account/Login";
    option.LogoutPath = $"/Identity/Account/Logout";
    option.AccessDeniedPath = $"/Identity/Account/AccessDenied";
});
//builder.Services.AddScoped<IUnitOfWorkRepositray, UnitOfWorkRepositray>();
builder.Services.AddScoped<IEmailSender, EmailSender>();
//builder.Services.AddScoped<IEmailService, EmailService>();

// Sent Email
var emailConfig = configuration.GetSection("EmailConfigration").Get<EmailConfigration>();
builder.Services.AddSingleton(emailConfig);

builder.Services.Configure<IdentityOptions>(options =>
{
    options.SignIn.RequireConfirmedEmail = false;
});

// إعداد الجلسة
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);  // تحديد مدة صلاحية الجلسة
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;  // لجعل الكوكيز أساسي
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "uploads");

// Add static file middleware for the uploads folder
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadsFolder),
    RequestPath = "/uploads" // URL path prefix
});

//builder.Services.AddCors(options =>
//{
//    options.AddPolicy("App",
//        builder =>
//        {
//            builder.WithOrigins("http://localhost:3000")
//                   .AllowAnyMethod()
//                   .AllowAnyHeader();
//        });
//});

//app.UseCors("App");

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseSession();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages();
app.MapControllerRoute(
    name: "default",
    pattern: "{area=Student}/{controller=Home}/{action=Index}/{id?}");

app.Run();

//using BulkyBookDataAccess.Data;
//using BulkyBookDataAccess.Repositray;
//using BulkyBookDataAccess.Repositray.IRepositray;
//using BulkyBookUtility;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.AspNetCore.Identity.UI.Services;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.AspNetCore.SpaServices.Extensions;
//using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;

//var builder = WebApplication.CreateBuilder(args);

//Add services to the container.
// إضافة الخدمات يجب أن تتم قبل استدعاء Build

//builder.Services.AddControllersWithViews();
//builder.Services.AddDbContext<AppDbContext>(options =>
//    options.UseSqlServer(builder.Configuration.GetConnectionString("MyConnection")));

//إضافة صفحات Razor
//builder.Services.AddRazorPages();

//إضافة خدمات الهوية باستخدام قاعدة البيانات
//builder.Services.AddIdentity<IdentityUser, IdentityRole>()
//    .AddEntityFrameworkStores<AppDbContext>()
//    .AddDefaultTokenProviders();

//تكوين ملفات تعريف الارتباط (Cookies)
//builder.Services.ConfigureApplicationCookie(option =>
//{
//    option.LoginPath = "/Identity/Account/Login";
//option.LogoutPath = "/Identity/Account/Logout";
//option.AccessDeniedPath = "/Identity/Account/AccessDenied";
//});

//إضافة خدمات Unit of Work Repository و Email Sender
//builder.Services.AddScoped<IUnitOfWorkRepositray, UnitOfWorkRepositray>();
//builder.Services.AddScoped<IEmailSender, EmailSender>();

//إضافة إعدادات CORS
//builder.Services.AddCors(options =>
//{
//    options.AddPolicy("App",
//       corsBuilder =>
//        {
//            corsBuilder.WithOrigins("http://localhost:3000")
//                       .AllowAnyMethod()
//                       .AllowAnyHeader();
//        });
//});

//بعد إضافة جميع الخدمات نقوم ببناء التطبيق
//var app = builder.Build();

//Configure the HTTP request pipeline.
//if (!app.Environment.IsDevelopment())
//{
//    app.UseExceptionHandler("/Home/Error");
//استخدام HSTS لتحسين الأمان في الإنتاج
//    app.UseHsts();
//}

// إعدادات الأمان والأساسيات للتطبيق
//app.UseHttpsRedirection();
//app.UseStaticFiles();
//app.UseRouting();
//app.UseCors("App");
//app.UseAuthentication();
//app.UseAuthorization();

//إعداد توجيه صفحات Razor وتحديد توجيه التحكم الافتراضي
//app.MapRazorPages();
//app.MapControllerRoute(
//    name: "default",
//    pattern: "{area=Customer}/{controller=Home}/{action=Index}/{id?}");

//إعداد استخدام SPA لتطبيق React
//app.UseSpa(spa =>
//{
//    spa.Options.SourcePath = "App"; // المسار الذي يحتوي على تطبيق React (تأكد من أنك عدلت ClientApp إلى المسار الصحيح)

//if (app.Environment.IsDevelopment())
//{
//    spa.UseReactDevelopmentServer(npmScript: "start");
//}
//});

//app.Run();
