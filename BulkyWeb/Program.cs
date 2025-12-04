//using Microsoft.AspNetCore.Identity;
//using Microsoft.AspNetCore.Identity.UI.Services;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.Extensions.FileProviders;
//using ScannerDataAccess.Data;
//using ScannerModels.Model;
//using ScannerUtility;
//using ScannerWeb.SeedData;

//var builder = WebApplication.CreateBuilder(args);
//var configuration = builder.Configuration;

//// Add services to the container.
//builder.Services.AddControllersWithViews();
//builder.Services.AddDbContext<AppDbContext>(options =>
//    options.UseSqlServer(builder.Configuration.GetConnectionString("MyConnection")));

//builder.Services.AddRazorPages();
//builder.Services.AddIdentity<ApplicationUser, IdentityRole>().AddEntityFrameworkStores<AppDbContext>().AddDefaultTokenProviders();

//builder.Services.ConfigureApplicationCookie(option =>
//{
//    option.LoginPath = $"/Identity/Account/Login";
//    option.LogoutPath = $"/Identity/Account/Logout";
//    option.AccessDeniedPath = $"/Identity/Account/AccessDenied";
//});
//builder.Services.AddScoped<IEmailSender, EmailSender>();

//// Sent Email
//var emailConfig = configuration.GetSection("EmailConfigration").Get<EmailConfigration>();
//builder.Services.AddSingleton(emailConfig);

//builder.Services.Configure<IdentityOptions>(options =>
//{
//    options.SignIn.RequireConfirmedEmail = false;
//});

//// إعداد الجلسة
//builder.Services.AddSession(options =>
//{
//    options.IdleTimeout = TimeSpan.FromMinutes(30);  // تحديد مدة صلاحية الجلسة
//    options.Cookie.HttpOnly = true;
//    options.Cookie.IsEssential = true;  // لجعل الكوكيز أساسي
//});

//builder.Services.AddSwaggerGen();

//var app = builder.Build();

//using (var scope = app.Services.CreateScope())
//{
//    var services = scope.ServiceProvider;
//    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
//    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
//    var context = services.GetRequiredService<AppDbContext>();

//    await DbSeeder.SeedAsync(userManager, roleManager, context);
//}

//// Configure the HTTP request pipeline.
//if (!app.Environment.IsDevelopment())
//{
//    app.UseExceptionHandler("/Home/Error");
//    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
//    app.UseHsts();
//}
//var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "uploads");

//// Add static file middleware for the uploads folder
//app.UseStaticFiles(new StaticFileOptions
//{
//    FileProvider = new PhysicalFileProvider(uploadsFolder),
//    RequestPath = "/uploads" // URL path prefix
//});


//app.UseHttpsRedirection();
//app.UseStaticFiles();
//app.UseSession();

//app.UseRouting();

//app.UseAuthentication();
//app.UseAuthorization();

//app.MapRazorPages();

//app.UseSwagger();
//app.UseSwaggerUI();

//app.MapControllerRoute(
//    name: "default",
//    pattern: "{area=Identity}/{controller=Account}/{action=Login}/{id?}"
//);

//app.MapFallback(context =>
//{
//    context.Response.Redirect("/Identity/Account/Login");
//    return Task.CompletedTask;
//});

//app.Run();

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using ScannerDataAccess.Data;
using ScannerModels.Model;
using ScannerUtility;
using ScannerWeb.SeedData;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// Add MVC Controllers
builder.Services.AddControllersWithViews();

// Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("MyConnection")));

builder.Services.AddRazorPages();
//builder.Services.AddIdentity<ApplicationUser, IdentityRole>().AddEntityFrameworkStores<AppDbContext>().AddDefaultTokenProviders();

// Identity (Login for Website)
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// Cookie Auth (Website)
builder.Services.ConfigureApplicationCookie(option =>
{
    option.LoginPath = "/Identity/Account/Login";
    option.LogoutPath = "/Identity/Account/Logout";
    option.AccessDeniedPath = "/Identity/Account/AccessDenied";
});

// Email Sender
builder.Services.AddScoped<IEmailSender, EmailSender>();
builder.Services.AddSingleton(configuration.GetSection("EmailConfigration").Get<EmailConfigration>());

// Disable Confirm Email
builder.Services.Configure<IdentityOptions>(options =>
{
    options.SignIn.RequireConfirmedEmail = false;
});

// SESSION
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// ------------------------------
// 🔐 JWT Authentication (For Mobile App + Swagger)
// ------------------------------
builder.Services.AddAuthentication()
    .AddJwtBearer("JWT", options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = configuration["Jwt:Issuer"],
            ValidAudience = configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(configuration["Jwt:Key"]))
        };
    });

// ------------------------------
// 📌 Swagger + JWT Button
// ------------------------------
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter JWT: Bearer {token}",
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Id = "Bearer",
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme
                }
            },
            new string[]{}
        }
    });
});

var app = builder.Build();

// Seed Database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var context = services.GetRequiredService<AppDbContext>();
    await DbSeeder.SeedAsync(userManager, roleManager, context);
}

// ------------------------------
// Pipeline
// ------------------------------
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Uploads folder
var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
if (!Directory.Exists(uploadsFolder))
    Directory.CreateDirectory(uploadsFolder);

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadsFolder),
    RequestPath = "/uploads"
});

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseSession();

app.UseRouting();

// AUTH (supports Cookie + JWT)
app.UseAuthentication();
app.UseAuthorization();

// Swagger
app.UseSwagger();
app.UseSwaggerUI();

// Default Route (Website)
app.MapRazorPages();

app.MapControllerRoute(
    name: "default",
    pattern: "{area=Identity}/{controller=Account}/{action=Login}/{id?}"
);

app.MapFallback(context =>
{
    context.Response.Redirect("/Identity/Account/Login");
    return Task.CompletedTask;
});

app.Run();
