using AutoSchoolProject.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AutoSchoolProject.Data
{
    public static class IdentitySeed
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            string[] roles = { "Admin", "Instructor", "Student" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            await EnsureAdminAsync(userManager);
            await EnsureInstructorsAsync(userManager, context);
            await EnsureStudentsAsync(userManager, context);

            await context.SaveChangesAsync();
        }

        private static async Task EnsureAdminAsync(UserManager<ApplicationUser> userManager)
        {
            const string email = "admin@school.local";
            var existing = await userManager.FindByEmailAsync(email);
            if (existing != null)
            {
                if (!await userManager.IsInRoleAsync(existing, "Admin"))
                {
                    await userManager.AddToRoleAsync(existing, "Admin");
                }
                return;
            }

            var admin = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                FirstName = "Администратор",
                LastName = "Маринов",
                PhoneNumber = "+359888100100"
            };

            await userManager.CreateAsync(admin, "Admin123!");
            await userManager.AddToRoleAsync(admin, "Admin");
        }

        private static async Task EnsureInstructorsAsync(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            var instructors = new[]
            {
                new SeedUser("nikolay.petrov@autoschool.bg", "Николай", "Петров", "+359888101001"),
                new SeedUser("georgi.iliev@autoschool.bg", "Георги", "Илиев", "+359888101002"),
                new SeedUser("dani.elenski@autoschool.bg", "Даниел", "Еленски", "+359888101003"),
                new SeedUser("borislav.stoyanov@autoschool.bg", "Борислав", "Стоянов", "+359888101004"),
                new SeedUser("maria.dobreva@autoschool.bg", "Мария", "Добрева", "+359888101005"),
                new SeedUser("tsvetelina.koleva@autoschool.bg", "Цветелина", "Колева", "+359888101006")
            };

            foreach (var item in instructors)
            {
                var user = await userManager.FindByEmailAsync(item.Email);
                if (user == null)
                {
                    user = new ApplicationUser
                    {
                        UserName = item.Email,
                        Email = item.Email,
                        EmailConfirmed = true,
                        FirstName = item.FirstName,
                        LastName = item.LastName,
                        PhoneNumber = item.PhoneNumber
                    };

                    await userManager.CreateAsync(user, "Instructor123!");
                }

                if (!await userManager.IsInRoleAsync(user, "Instructor"))
                {
                    await userManager.AddToRoleAsync(user, "Instructor");
                }

                var exists = await context.Instructors.AnyAsync(i => i.UserId == user.Id);
                if (!exists)
                {
                    context.Instructors.Add(new Instructor { UserId = user.Id });
                }
            }
        }

        private static async Task EnsureStudentsAsync(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            var students = new[]
            {
                new SeedUser("ivan.ivanov@student.bg", "Иван", "Иванов", "+359888201001"),
                new SeedUser("petya.atanasova@student.bg", "Петя", "Атанасова", "+359888201002"),
                new SeedUser("martin.velikov@student.bg", "Мартин", "Великов", "+359888201003"),
                new SeedUser("simona.pavlova@student.bg", "Симона", "Павлова", "+359888201004"),
                new SeedUser("kristian.tanev@student.bg", "Кристиан", "Танев", "+359888201005"),
                new SeedUser("yoana.ruseva@student.bg", "Йоана", "Русева", "+359888201006"),
                new SeedUser("vasil.marinov@student.bg", "Васил", "Маринов", "+359888201007"),
                new SeedUser("desislava.boneva@student.bg", "Десислава", "Бонева", "+359888201008"),
                new SeedUser("radoslav.angelov@student.bg", "Радослав", "Ангелов", "+359888201009"),
                new SeedUser("teodora.stefanova@student.bg", "Теодора", "Стефанова", "+359888201010"),
                new SeedUser("svetlin.zhelev@student.bg", "Светлин", "Желев", "+359888201011"),
                new SeedUser("mihaela.krasteva@student.bg", "Михаела", "Кръстева", "+359888201012")
            };

            foreach (var item in students)
            {
                var user = await userManager.FindByEmailAsync(item.Email);
                if (user == null)
                {
                    user = new ApplicationUser
                    {
                        UserName = item.Email,
                        Email = item.Email,
                        EmailConfirmed = true,
                        FirstName = item.FirstName,
                        LastName = item.LastName,
                        PhoneNumber = item.PhoneNumber
                    };

                    await userManager.CreateAsync(user, "Student123!");
                }

                if (!await userManager.IsInRoleAsync(user, "Student"))
                {
                    await userManager.AddToRoleAsync(user, "Student");
                }

                var exists = await context.Students.AnyAsync(s => s.UserId == user.Id);
                if (!exists)
                {
                    context.Students.Add(new Student { UserId = user.Id });
                }
            }
        }

        private sealed record SeedUser(string Email, string FirstName, string LastName, string PhoneNumber);
    }
}
