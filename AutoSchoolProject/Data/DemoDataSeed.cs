using AutoSchoolProject.Models;
using AutoSchoolProject.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace AutoSchoolProject.Data
{
    public static class DemoDataSeed
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var courses = await context.Courses.OrderBy(c => c.Id).ToListAsync();
            var courseB = courses.FirstOrDefault(c => c.Name == "Категория B");
            var courseC = courses.FirstOrDefault(c => c.Name == "Категория C");
            var courseD = courses.FirstOrDefault(c => c.Name == "Категория D");

            if (courseB == null || courseC == null || courseD == null)
            {
                throw new Exception("Не са намерени категориите B, C и D.");
            }

            await AssignCoursesToProfilesAsync(context, courseB.Id, courseC.Id, courseD.Id);
            await SeedTheorySessionsAsync(context, new[] { courseB, courseC, courseD });
            await SeedPracticeLessonsAsync(context, courseB.Id, courseC.Id, courseD.Id);
            await SeedTestResultsAsync(context);

            await context.SaveChangesAsync();
        }

        private static async Task AssignCoursesToProfilesAsync(ApplicationDbContext context, int courseBId, int courseCId, int courseDId)
        {
            var instructors = await context.Instructors.Include(i => i.User).ToListAsync();
            var students = await context.Students.Include(s => s.User).ToListAsync();

            var instructorCourseMap = new Dictionary<string, (int CourseId, string CarModel)>
            {
                ["nikolay.petrov@autoschool.bg"] = (courseBId, "Volkswagen Golf 7"),
                ["georgi.iliev@autoschool.bg"] = (courseBId, "Skoda Fabia"),
                ["dani.elenski@autoschool.bg"] = (courseCId, "MAN TGL"),
                ["borislav.stoyanov@autoschool.bg"] = (courseCId, "Mercedes Atego"),
                ["maria.dobreva@autoschool.bg"] = (courseDId, "Mercedes Sprinter"),
                ["tsvetelina.koleva@autoschool.bg"] = (courseDId, "Ford Transit"),
            };

            foreach (var instructor in instructors)
            {
                var email = instructor.User.Email ?? string.Empty;
                if (instructorCourseMap.TryGetValue(email, out var config))
                {
                    instructor.CourseId = config.CourseId;
                    instructor.CarModel = config.CarModel;
                }
            }

            var studentCourseMap = new Dictionary<string, int>
            {
                ["ivan.ivanov@student.bg"] = courseBId,
                ["petya.atanasova@student.bg"] = courseBId,
                ["martin.velikov@student.bg"] = courseBId,
                ["simona.pavlova@student.bg"] = courseBId,
                ["kristian.tanev@student.bg"] = courseCId,
                ["yoana.ruseva@student.bg"] = courseCId,
                ["vasil.marinov@student.bg"] = courseCId,
                ["desislava.boneva@student.bg"] = courseCId,
                ["radoslav.angelov@student.bg"] = courseDId,
                ["teodora.stefanova@student.bg"] = courseDId,
                ["svetlin.zhelev@student.bg"] = courseDId,
                ["mihaela.krasteva@student.bg"] = courseDId,
            };

            foreach (var student in students)
            {
                var email = student.User.Email ?? string.Empty;
                if (studentCourseMap.TryGetValue(email, out var courseId))
                {
                    student.CourseId = courseId;
                }
            }

            await context.SaveChangesAsync();
        }

        private static async Task SeedTheorySessionsAsync(ApplicationDbContext context, IEnumerable<Course> courses)
        {
            if (await context.TheorySessions.AnyAsync())
            {
                return;
            }

            var startDate = DateTime.Today.AddDays(1).Date.AddHours(18);

            foreach (var course in courses)
            {
                for (int i = 0; i < 6; i++)
                {
                    context.TheorySessions.Add(new TheorySession
                    {
                        CourseId = course.Id,
                        DateTime = startDate.AddDays(i * 2),
                        DurationMinutes = 90,
                        Topic = $"Теория {i + 1} - {course.Name}",
                        Location = "Учебна зала 1"
                    });
                }
            }
        }

        private static async Task SeedPracticeLessonsAsync(ApplicationDbContext context, int courseBId, int courseCId, int courseDId)
        {
            if (await context.PracticeLessons.AnyAsync())
            {
                return;
            }

            var instructors = await context.Instructors
                .Include(i => i.User)
                .Where(i => i.CourseId != null)
                .OrderBy(i => i.User.FirstName)
                .ThenBy(i => i.User.LastName)
                .ToListAsync();

            var students = await context.Students
                .Include(s => s.User)
                .Where(s => s.CourseId != null)
                .OrderBy(s => s.User.FirstName)
                .ThenBy(s => s.User.LastName)
                .ToListAsync();

            var studentsByCourse = students
                .GroupBy(s => s.CourseId!.Value)
                .ToDictionary(g => g.Key, g => g.ToList());

            foreach (var instructor in instructors)
            {
                if (!instructor.CourseId.HasValue || !studentsByCourse.TryGetValue(instructor.CourseId.Value, out var matchingStudents) || matchingStudents.Count == 0)
                {
                    continue;
                }

                for (int i = 0; i < 5; i++)
                {
                    var student = matchingStudents[i % matchingStudents.Count];
                    context.PracticeLessons.Add(new PracticeLesson
                    {
                        InstructorId = instructor.Id,
                        StudentId = student.Id,
                        CourseId = instructor.CourseId,
                        DateTime = DateTime.Today.AddDays(-(10 - i)).AddHours(9 + i),
                        DurationMinutes = 50,
                        Completed = true,
                        Status = LessonStatus.Approved,
                        Note = $"Проведен час {i + 1}"
                    });
                }

                for (int i = 0; i < 5; i++)
                {
                    var student = matchingStudents[(i + 1) % matchingStudents.Count];
                    context.PracticeLessons.Add(new PracticeLesson
                    {
                        InstructorId = instructor.Id,
                        StudentId = student.Id,
                        CourseId = instructor.CourseId,
                        DateTime = DateTime.Today.AddDays(2 + i).AddHours(10 + (i % 2)),
                        DurationMinutes = 50,
                        Completed = false,
                        Status = LessonStatus.Pending,
                        Note = "Заявка за практика"
                    });
                }

                for (int i = 0; i < 5; i++)
                {
                    var student = matchingStudents[(i + 2) % matchingStudents.Count];
                    context.PracticeLessons.Add(new PracticeLesson
                    {
                        InstructorId = instructor.Id,
                        StudentId = student.Id,
                        CourseId = instructor.CourseId,
                        DateTime = DateTime.Today.AddDays(10 + i).AddHours(8 + (i % 3)),
                        DurationMinutes = 50,
                        Completed = false,
                        Status = LessonStatus.Approved,
                        Note = "Одобрен предстоящ час"
                    });
                }

                for (int i = 0; i < 4; i++)
                {
                    context.PracticeLessons.Add(new PracticeLesson
                    {
                        InstructorId = instructor.Id,
                        StudentId = null,
                        CourseId = instructor.CourseId,
                        DateTime = DateTime.Today.AddDays(20 + i).AddHours(9 + i),
                        DurationMinutes = 50,
                        Completed = false,
                        Status = LessonStatus.Available,
                        Note = "Свободен слот"
                    });
                }
            }
        }

        private static async Task SeedTestResultsAsync(ApplicationDbContext context)
        {
            if (await context.TestResultListovki.AnyAsync())
            {
                return;
            }

            var students = await context.Students.ToListAsync();
            var random = new Random(42);

            foreach (var student in students)
            {
                context.TestResultListovki.Add(new TestResultListovki
                {
                    StudentId = student.Id,
                    Score = random.Next(74, 97),
                    Date = DateTime.Today.AddDays(-random.Next(2, 25))
                });
            }
        }
    }
}