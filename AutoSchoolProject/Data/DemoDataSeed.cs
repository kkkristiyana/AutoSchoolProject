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

            var courses = await context.Courses.ToListAsync();
            var courseB = courses.FirstOrDefault(c => c.Name == "Категория B");
            var courseC = courses.FirstOrDefault(c => c.Name == "Категория C");
            var courseD = courses.FirstOrDefault(c => c.Name == "Категория D");

            if (courseB == null || courseC == null || courseD == null)
                throw new Exception("Не са намерени курсовете B, C или D!");

            var instructors = await context.Instructors.Include(i => i.User).ToListAsync();
            if (instructors.Count >= 4)
            {
                instructors[0].CourseId = courseB.Id;
                instructors[1].CourseId = courseC.Id;
                instructors[2].CourseId = courseD.Id;
                instructors[3].CourseId = courseB.Id;
            }

            // Assign courses
            var students = await context.Students.Include(s => s.User).ToListAsync();
            for (int i = 0; i < students.Count; i++)
            {
                if (i % 3 == 0) students[i].CourseId = courseB.Id;
                else if (i % 3 == 1) students[i].CourseId = courseC.Id;
                else students[i].CourseId = courseD.Id;
            }

            await context.SaveChangesAsync();

            // PracticeLessons
            if (!context.PracticeLessons.Any())
            {
                int lessonNumber = 1;
                foreach (var student in students)
                {
                    var courseInstructors = instructors.Where(i => i.CourseId == student.CourseId).Take(2).ToList();
                    foreach (var instructor in courseInstructors)
                    {
                        context.PracticeLessons.Add(new PracticeLesson
                        {
                            StudentId = student.Id,
                            InstructorId = instructor.Id,
                            CourseId = student.CourseId,
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