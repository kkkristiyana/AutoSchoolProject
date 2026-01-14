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

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Course>()
                .Property(c => c.Price)
                .HasPrecision(18, 2); // 18 цифри общо, 2 след десетичната запетая

            // Връзка между ApplicationUser и Instructor
            builder.Entity<Instructor>()
                .HasOne(i => i.User)
                .WithOne(u => u.Instructor)
                .HasForeignKey<Instructor>(i => i.UserId);

            // Връзка между ApplicationUser и Student
            builder.Entity<Student>()
                .HasOne(s => s.User)
                .WithOne(u => u.Student)
                .HasForeignKey<Student>(s => s.UserId);
        }
    }
}