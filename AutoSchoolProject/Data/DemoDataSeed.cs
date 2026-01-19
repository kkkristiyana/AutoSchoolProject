using AutoSchoolProject.Models;
using Microsoft.EntityFrameworkCore;


namespace AutoSchoolProject.Data
{
    public static class DemoDataSeed
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var instructors = context.Instructors.ToList();
            var students = context.Students
            .Include(s => s.User)
            .ToList();

            var courses = context.Courses.ToList();
            // Assign courses
            foreach (var student in students)
            {
                student.CourseId = courses.First().Id;
            }

            // PracticeLessons
            if (!context.PracticeLessons.Any())
            {
                int lessonNumber = 1;

                foreach (var student in students)
                {
                    foreach (var instructor in instructors.Take(2))
                    {
                        context.PracticeLessons.Add(new PracticeLesson
                        {
                            StudentId = student.Id,
                            InstructorId = instructor.Id,
                            DateTime = DateTime.Today.AddDays(lessonNumber),
                            Completed = false,
                            Note = $"Урок {lessonNumber} за {student.User.FirstName}"
                        });

                        lessonNumber++;
                    }
                }
            }


            // TestResultListovki
            if (!context.TestResultListovki.Any())
            {
                var rand = new Random();

                foreach (var student in students)
                {
                    context.TestResultListovki.Add(new TestResultListovki
                    {
                        StudentId = student.Id,
                        Score = rand.Next(60, 97),
                        Date = DateTime.Today.AddDays(-rand.Next(1, 15))
                    });
                }
            }

            await context.SaveChangesAsync();
        }
    }
}