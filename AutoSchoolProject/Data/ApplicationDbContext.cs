using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using AutoSchoolProject.Models;

namespace AutoSchoolProject.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Student> Students { get; set; }
        public DbSet<Instructor> Instructors { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<PracticeLesson> PracticeLessons { get; set; }
        public DbSet<TestResultListovki> TestResultListovki { get; set; }
        public DbSet<EnrollmentRequest> EnrollmentRequests { get; set; }
        public DbSet<TheorySession> TheorySessions { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Course>()
                .Property(c => c.Price)
                .HasPrecision(18, 2); //18 цифри общо,2 след запетаята

            //Връзка между ApplicationUser и Instructor
            builder.Entity<Instructor>()
                .HasOne(i => i.User)
                .WithOne(u => u.Instructor)
                .HasForeignKey<Instructor>(i => i.UserId);

            //Връзка между ApplicationUser и Student
            builder.Entity<Student>()
                .HasOne(s => s.User)
                .WithOne(u => u.Student)
                .HasForeignKey<Student>(s => s.UserId);

            builder.Entity<EnrollmentRequest>()
               .HasOne(r => r.Course)
               .WithMany()
               .HasForeignKey(r => r.CourseId)
               .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<TheorySession>()
                .HasOne(t => t.Course)
                .WithMany()
                .HasForeignKey(t => t.CourseId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}