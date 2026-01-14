using AutoSchoolProject.Models;
using Microsoft.AspNetCore.Identity;

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

            if (await userManager.FindByEmailAsync("admin@school.local") == null)
            {
                var admin = new ApplicationUser
                {
                    UserName = "admin@school.local",
                    Email = "admin@school.local",
                    EmailConfirmed = true,
                    FirstName = "Admin",
                    LastName = "Demo"
                };
                await userManager.CreateAsync(admin, "Admin123!");
                await userManager.AddToRoleAsync(admin, "Admin");
            }

            var instructorsData = new[]
            {
                ("ivan@gmail.com", "Ivan"),
                ("petar@gmail.com", "Petar"),
                ("maria@gmail.com", "Maria"),
                ("stoyan@gmail.com", "Stoyan")
            };

            foreach (var (email, firstName) in instructorsData)
            {
                if (await userManager.FindByEmailAsync(email) == null)
                {
                    var inst = new ApplicationUser
                    {
                        UserName = email,
                        Email = email,
                        EmailConfirmed = true,
                        FirstName = firstName,
                        LastName = "Demo"
                    };
                    await userManager.CreateAsync(inst, "Instructor123!");
                    await userManager.AddToRoleAsync(inst, "Instructor");

                    context.Instructors.Add(new Instructor { UserId = inst.Id });
                }
            }

            var studentsData = new[]
            {
                ("georgi@gmail.com", "Georgi"),
                ("ana@gmail.com", "Ana"),
                ("dimitar@gmail.com", "Dimitar"),
                ("elena@gmail.com", "Elena"),
                ("viktor@gmail.com", "Viktor"),
                ("sofia@gmail.com", "Sofia")
            };

            foreach (var (email, firstName) in studentsData)
            {
                if (await userManager.FindByEmailAsync(email) == null)
                {
                    var stud = new ApplicationUser
                    {
                        UserName = email,
                        Email = email,
                        EmailConfirmed = true,
                        FirstName = firstName,
                        LastName = "Demo"
                    };
                    await userManager.CreateAsync(stud, "Student123!");
                    await userManager.AddToRoleAsync(stud, "Student");

                    context.Students.Add(new Student { UserId = stud.Id });
                }
            }

            await context.SaveChangesAsync();
        }
    }
}