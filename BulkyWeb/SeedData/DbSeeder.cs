using global::ScannerDataAccess.Data;
using Microsoft.AspNetCore.Identity;
using ScannerModels.Model;

namespace ScannerWeb.SeedData
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            AppDbContext context)
        {
            // -----------------------------
            // 1) Roles
            // -----------------------------
            string[] roles = { "HR", "Dean", "HeadOfDepartment", "Doctor", "Student" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            // -----------------------------
            // 2) Create Users Helper
            // -----------------------------
            async Task<ApplicationUser> CreateUser(string email, string first, string last, string role)
            {
                var user = await userManager.FindByEmailAsync(email);
                if (user != null) return user;

                user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    FirstName = first,
                    LastName = last,
                    EmailConfirmed = true,
                    TypeUser = role
                };

                await userManager.CreateAsync(user, "Test@123");
                await userManager.AddToRoleAsync(user, role);

                return user;
            }

            // -----------------------------
            // 3) Fixed Staff + Doctors + Students
            // -----------------------------
            var dean = await CreateUser("dean@uni.com", "Ahmad", "Hassan", "Dean");

            var hodCS = await CreateUser("hod.cs@uni.com", "Sara", "Yousef", "HeadOfDepartment");
            var hodAI = await CreateUser("hod.ai@uni.com", "Othman", "Jaber", "HeadOfDepartment");

            var doc1 = await CreateUser("doctor1@uni.com", "Omar", "Ali", "Doctor");
            var doc2 = await CreateUser("doctor2@uni.com", "Hadeel", "Salem", "Doctor");
            var doc3 = await CreateUser("doctor3@uni.com", "Faisal", "Maher", "Doctor");
            var doc4 = await CreateUser("doctor4@uni.com", "Mona", "Tariq", "Doctor");

            // -----------------------------
            // 4) Fake Students (20)
            // -----------------------------
            List<ApplicationUser> fakeStudents = new();

            if (!context.Users.Any(u => u.Email.StartsWith("fake")))
            {
                string[] fNames = { "Lina","Rami","Maya","Omar","Hadi","Samer","Dalal","Tala","Yousef","Khaled",
                                    "Ruba","Dana","Saif","Fares","Noor","Aya","Bayan","Tariq","Nour","Ahmad" };

                string[] lNames = { "Hassan", "Odeh", "Salem", "Maher", "Tariq", "Khalil", "Haddad", "Masri", "Jaber", "Naser" };

                int index = 1;

                foreach (var fn in fNames)
                {
                    var st = new ApplicationUser
                    {
                        UserName = $"fake{index}@uni.com",
                        Email = $"fake{index}@uni.com",
                        FirstName = fn,
                        LastName = lNames[index % lNames.Length],
                        EmailConfirmed = true,
                        TypeUser = "Student"
                    };

                    await userManager.CreateAsync(st, "Test@123");
                    await userManager.AddToRoleAsync(st, "Student");

                    fakeStudents.Add(st);
                    index++;
                }
            }

            await context.SaveChangesAsync();

            // -----------------------------
            // 5) Colleges
            // -----------------------------
            if (!context.Colleges.Any())
            {
                context.Colleges.AddRange(
                    new College { CollegeName = "Engineering College", Building = "A1", DeanUserID = dean.Id },
                    new College { CollegeName = "IT College", Building = "B1", DeanUserID = dean.Id }
                );

                await context.SaveChangesAsync();
            }

            // -----------------------------
            // 6) Departments
            // -----------------------------
            if (!context.Departments.Any())
            {
                context.Departments.AddRange(
                    new Department { DepartmentName = "Computer Science", CollegeID = 1, HeadUserID = hodCS.Id },
                    new Department { DepartmentName = "Artificial Intelligence", CollegeID = 1, HeadUserID = hodAI.Id }
                );

                await context.SaveChangesAsync();
            }

            // -----------------------------
            // 7) Courses
            // -----------------------------
            if (!context.Courses.Any())
            {
                context.Courses.AddRange(
                    new Course { CourseCode = "CS101", CourseName = "Programming 1", DepartmentID = 1 },
                    new Course { CourseCode = "CS201", CourseName = "Algorithms", DepartmentID = 1 },
                    new Course { CourseCode = "AI101", CourseName = "Artificial Intelligence", DepartmentID = 2 },
                    new Course { CourseCode = "AI202", CourseName = "Machine Learning", DepartmentID = 2 }
                );

                await context.SaveChangesAsync();
            }

            // -----------------------------
            // 8) Sections
            // -----------------------------
            if (!context.CourseSections.Any())
            {
                context.CourseSections.AddRange(
                    new CourseSection { CourseID = 1, SemesterID = 1, SectionNumber = 1, DoctorUserID = doc1.Id, TotpSecretKey = "K1" },
                    new CourseSection { CourseID = 1, SemesterID = 1, SectionNumber = 2, DoctorUserID = doc2.Id, TotpSecretKey = "K2" },

                    new CourseSection { CourseID = 3, SemesterID = 1, SectionNumber = 1, DoctorUserID = doc3.Id, TotpSecretKey = "K3" },
                    new CourseSection { CourseID = 3, SemesterID = 1, SectionNumber = 2, DoctorUserID = doc4.Id, TotpSecretKey = "K4" }
                );

                await context.SaveChangesAsync();
            }

            // -----------------------------
            // 9) Enroll Students
            // -----------------------------
            if (!context.StudentEnrollments.Any())
            {
                var sections = context.CourseSections.ToList();
                var students = context.Users.Where(u => u.TypeUser == "Student").ToList();
                var random = new Random();

                foreach (var st in students)
                {
                    var sec = sections[random.Next(sections.Count)];

                    context.StudentEnrollments.Add(new StudentEnrollment
                    {
                        StudentUserID = st.Id,
                        CourseSectionID = sec.CourseSectionID
                    });
                }

                await context.SaveChangesAsync();
            }

            // DONE ✔
        }
    }
}
