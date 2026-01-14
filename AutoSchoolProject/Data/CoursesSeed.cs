using AutoSchoolProject.Models;

namespace AutoSchoolProject.Data
{
    public static class CoursesSeed
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            if (!context.Courses.Any())
            {
                context.Courses.AddRange(
                    new Course { Name = "Категория B", Price = 1200 },
                    new Course { Name = "Категория C", Price = 2500 },
                    new Course { Name = "Категория D", Price = 1800 }
                );

                await context.SaveChangesAsync();
            }
        }
    }
}