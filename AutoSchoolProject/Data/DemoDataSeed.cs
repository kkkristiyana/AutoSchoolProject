using AutoSchoolProject.Models;


namespace AutoSchoolProject.Data
{
    public static class DemoDataSeed
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var instructors = context.Instructors.ToList();
            var students = context.Students.ToList();

            //PracticeLessons
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

            //TestResultListovki
            foreach (var student in students)
            {
                context.TestResultListovki.Add(new TestResultListovki
                {
                    StudentId = student.Id,
                    Score = new Random().Next(60, 100),
                    Date = DateTime.Today.AddDays(-new Random().Next(1, 15))
                });
            }

            await context.SaveChangesAsync();
        }
    }
}