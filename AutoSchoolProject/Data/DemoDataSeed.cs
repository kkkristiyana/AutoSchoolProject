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
            var students = new List<(Student studentEntity, ApplicationUser user)>();

            // PracticeLessons
            int lessonNumber = 1;

            foreach (var (studentEntity, user) in students)
            {
                foreach (var instructor in instructors.Take(2))
                {
                    context.PracticeLessons.Add(new PracticeLesson
                    {
                        StudentId = studentEntity.Id,
                        InstructorId = instructor.Id,
                        DateTime = DateTime.Today.AddDays(lessonNumber),
                        Completed = false,
                        Note = $"Урок {lessonNumber} за {user.FirstName}"
                    });

                    lessonNumber++;
                }
            }
            // TestResultListovki
            var rand = new Random();

            foreach (var (studentEntity, user) in students)
            {
                context.TestResultListovki.Add(new TestResultListovki
                {
                    StudentId = studentEntity.Id,
                    Score = rand.Next(60, 100),
                    Date = DateTime.Today.AddDays(-rand.Next(1, 15))
                });
            }

            await context.SaveChangesAsync();
        }
    }
}