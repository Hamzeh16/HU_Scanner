using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ScannerModels.Model;

namespace ScannerDataAccess.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<College> Colleges { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<CourseSection> CourseSections { get; set; }
        public DbSet<StudentEnrollment> StudentEnrollments { get; set; }
        public DbSet<AttendanceLog> AttendanceLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // --- التكوين باستخدام Fluent API (اختياري ولكنه موصى به) ---

            // تكوين علاقة واحد لواحد (One-to-One) بين العميد والكلية
            builder.Entity<ApplicationUser>()
               .HasOne(u => u.ManagedCollege)
               .WithOne(c => c.Dean)
               .HasForeignKey<College>(c => c.DeanUserID); // [5]

            // تكوين علاقة واحد لواحد (One-to-One) بين رئيس القسم والقسم
            builder.Entity<ApplicationUser>()
               .HasOne(u => u.ManagedDepartment)
               .WithOne(d => d.Head)
               .HasForeignKey<Department>(d => d.HeadUserID);

            // تكوين القيد الفريد (UNIQUE Constraint) في جدول الحضور [14]
            // لمنع تسجيل الطالب مرتين في نفس اليوم لنفس الشعبة
            builder.Entity<AttendanceLog>()
               .HasIndex(al => new { al.CourseSectionID, al.StudentUserID, al.AttendanceDate })
               .IsUnique();

            // 1. تحديد علاقة "الطالب" (Student)
            // نوضح أن ICollection<AttendanceLog> في ApplicationUser
            // مرتبطة بـ AttendanceLog.Student
            builder.Entity<AttendanceLog>()
               .HasOne(al => al.Student) // سجل الحضور له طالب واحد
               .WithMany(u => u.AttendanceLogs) // والمستخدم (الطالب) لديه سجلات حضور كثيرة
               .HasForeignKey(al => al.StudentUserID) // المفتاح الأجنبي هو StudentUserID
               .OnDelete(DeleteBehavior.Cascade); // اختياري: حذف سجلات الطالب عند حذفه

            // 2. تحديد علاقة "المحقق" (Verifier)
            // نوضح أن علاقة AttendanceLog.Verifier ليس لها مجموعة (Collection)
            // مقابلة في ApplicationUser (لأننا لم ننشئ واحدة)
            builder.Entity<AttendanceLog>()
               .HasOne(al => al.Verifier) // سجل الحضور له محقق واحد
               .WithMany() // ApplicationUser ليس لديه مجموعة مقابلة لـ "VerifiedLogs"
               .HasForeignKey(al => al.VerifiedByUserID) // المفتاح الأجنبي هو VerifiedByUserID
               .OnDelete(DeleteBehavior.Restrict); // منع حذف الدكتور إذا كان قد تحقق من سجلات

            SeedRoles(builder);
            //SeedTestData(builder);
        }

        private static void SeedRoles(ModelBuilder builder)
        {
            builder.Entity<IdentityRole>().HasData
                (
            new IdentityRole() { Name = "HR", ConcurrencyStamp = "1", NormalizedName = "HR" },
            new IdentityRole() { Name = "Dean", ConcurrencyStamp = "2", NormalizedName = "Dean" },
            new IdentityRole() { Name = "HeadOfDepartment", ConcurrencyStamp = "3", NormalizedName = "HeadOfDepartment" },
            new IdentityRole() { Name = "Docter", ConcurrencyStamp = "4", NormalizedName = "Doctor" },
            new IdentityRole() { Name = "Student", ConcurrencyStamp = "5", NormalizedName = "Student" }
                );
        }

        private void SeedTestData(ModelBuilder builder)
        {
            // ====== 1. Seed a Department ======
            builder.Entity<Department>().HasData(
                new Department
                {
                    DepartmentID = 1,
                    DepartmentName = "Computer Science",
                    CollegeID = 1,
                    HeadUserID = null
                }
            );

            // ====== 2. Seed College ======
            builder.Entity<College>().HasData(
                new College
                {
                    CollegeID = 1,
                    CollegeName = "Engineering College",
                    Building = "Building A",
                    DeanUserID = null
                }
            );

            // ====== 3. Seed a Doctor User ======
            builder.Entity<ApplicationUser>().HasData(
                new ApplicationUser
                {
                    Id = "doctor1",                     // MUST be string
                    UserName = "doctor1@university.com",
                    NormalizedUserName = "DOCTOR1@UNIVERSITY.COM",
                    Email = "doctor1@university.com",
                    NormalizedEmail = "DOCTOR1@UNIVERSITY.COM",
                    EmailConfirmed = true,
                    FirstName = "Omar",
                    LastName = "Ali",
                    TypeUser = "Doctor",
                    SecurityStamp = Guid.NewGuid().ToString("D"),
                    PasswordHash = "AQAAAAEAACcQAAAAENj2V9vYpWkz..." // Optional (if you want real login)
                }
            );

            // ====== 4. Seed a Course ======
            builder.Entity<Course>().HasData(
                new Course
                {
                    CourseID = 1,
                    CourseCode = "CS101",
                    CourseName = "Programming Fundamentals",
                    DepartmentID = 1
                }
            );

            // ====== 5. Seed a CourseSection ======
            builder.Entity<CourseSection>().HasData(
                new CourseSection
                {
                    CourseSectionID = 1,
                    CourseID = 1,
                    SemesterID = 1,
                    ClassroomID = 101,
                    SectionNumber = 1,
                    DoctorUserID = "doctor1",      // Assign Doctor to teach this
                    TotpSecretKey = "TEST123456"   // static for testing
                }
            );
        }
    }
}