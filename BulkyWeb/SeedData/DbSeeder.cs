using global::ScannerDataAccess.Data;
using Microsoft.AspNetCore.Identity;
using ScannerModels.Model;

namespace ScannerWeb.SeedData
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, AppDbContext context)
        {
            // Ensure Roles Exist
            string[] roles = { "HR", "Dean", "HeadOfDepartment", "Doctor", "Student" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            // --- Create Users ---
            var dean = new ApplicationUser
            {
                UserName = "dean1@university.com",
                Email = "dean1@university.com",
                FirstName = "Ahmed",
                LastName = "Hassan",
                IDNumber = "D001",
                TypeUser = "Dean",
                EmailConfirmed = true
            };
            var head = new ApplicationUser
            {
                UserName = "head1@university.com",
                Email = "head1@university.com",
                FirstName = "Sara",
                LastName = "Yousef",
                IDNumber = "H001",
                TypeUser = "HeadOfDepartment",
                EmailConfirmed = true
            };
            var doctor = new ApplicationUser
            {
                UserName = "doctor1@university.com",
                Email = "doctor1@university.com",
                FirstName = "Omar",
                LastName = "Ali",
                IDNumber = "DR001",
                TypeUser = "Doctor",
                EmailConfirmed = true
            };
            var student = new ApplicationUser
            {
                UserName = "student1@university.com",
                Email = "student1@university.com",
                FirstName = "Lina",
                LastName = "Tariq",
                IDNumber = "S001",
                TypeUser = "Student",
                EmailConfirmed = true
            };

            // Helper to create user if not exists
            async Task CreateUser(ApplicationUser user, string role)
            {
                if (await userManager.FindByEmailAsync(user.Email) == null)
                {
                    await userManager.CreateAsync(user, "Test@123"); // default password
                    await userManager.AddToRoleAsync(user, role);
                }
            }

            await CreateUser(dean, "Dean");
            await CreateUser(head, "HeadOfDepartment");
            await CreateUser(doctor, "Doctor");
            await CreateUser(student, "Student");

            // --- Colleges ---
            if (!context.Colleges.Any())
            {
                context.Colleges.Add(new College
                {
                    CollegeName = "Engineering College",
                    Building = "A1",
                    DeanUserID = (await userManager.FindByEmailAsync(dean.Email)).Id
                });
                await context.SaveChangesAsync();
            }

            // --- Departments ---
            if (!context.Departments.Any())
            {
                var college = context.Colleges.First();
                context.Departments.Add(new Department
                {
                    DepartmentName = "Computer Science",
                    CollegeID = college.CollegeID,
                    HeadUserID = (await userManager.FindByEmailAsync(head.Email)).Id
                });
                await context.SaveChangesAsync();
            }

            // --- Courses ---
            if (!context.Courses.Any())
            {
                var dept = context.Departments.First();
                context.Courses.Add(new Course
                {
                    CourseName = "Artificial Intelligence",
                    CourseCode = "AI101",
                    DepartmentID = dept.DepartmentID
                });
                await context.SaveChangesAsync();
            }

            // --- CourseSections ---
            if (!context.CourseSections.Any())
            {
                var course = context.Courses.First();
                context.CourseSections.Add(new CourseSection
                {
                    CourseID = course.CourseID,
                    DoctorUserID = (await userManager.FindByEmailAsync(doctor.Email)).Id,
                    SemesterID = 1,
                    SectionNumber = 1,
                    TotpSecretKey = "ABC123"
                });
                await context.SaveChangesAsync();
            }

            // --- Enroll Students ---
            if (!context.StudentEnrollments.Any())
            {
                var studentUser = await userManager.FindByEmailAsync(student.Email);
                var section = context.CourseSections.First();
                context.StudentEnrollments.Add(new StudentEnrollment
                {
                    StudentUserID = studentUser.Id,
                    CourseSectionID = section.CourseSectionID
                });
                await context.SaveChangesAsync();
            }

            // --- Attendance (optional fake) ---
            if (!context.AttendanceLogs.Any())
            {
                var studentUser = await userManager.FindByEmailAsync(student.Email);
                var section = context.CourseSections.First();
                context.AttendanceLogs.Add(new AttendanceLog
                {
                    CourseSectionID = section.CourseSectionID,
                    StudentUserID = studentUser.Id,
                    AttendanceDate = DateTime.UtcNow.Date,
                    PresenceStatus = 1,
                    AttendanceMethod = 3,
                    VerifiedByUserID = (await userManager.FindByEmailAsync(doctor.Email)).Id
                });
                await context.SaveChangesAsync();
            }
        }
    }
}
