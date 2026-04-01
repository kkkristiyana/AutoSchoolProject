using AutoSchoolProject.Data;
using AutoSchoolProject.Models;
using AutoSchoolProject.Services;
using AutoSchoolProject.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;

namespace AutoSchoolProject
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));

            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.SignIn.RequireConfirmedAccount = false;
                options.Password.RequiredLength = 6;
            })
            .AddErrorDescriber<BulgarianIdentityErrorDescriber>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

            builder.Services.AddScoped<IEmailSender, DummyEmailSender>();
            builder.Services.AddScoped<IFileStorageService, FileStorageService>();
            builder.Services.AddScoped<StudentService>();
            builder.Services.AddScoped<IStudentService, StudentService>();
            builder.Services.AddScoped<InstructorService>();

            builder.Services.AddControllersWithViews(options =>
            {
                var provider = options.ModelBindingMessageProvider;

                provider.SetAttemptedValueIsInvalidAccessor((value, fieldName) =>
                    $"Стойността '{value}' не е валидна за полето {fieldName}.");
                provider.SetMissingBindRequiredValueAccessor(fieldName =>
                    $"Полето {fieldName} е задължително.");
                provider.SetMissingKeyOrValueAccessor(() =>
                    "Липсва ключ или стойност.");
                provider.SetMissingRequestBodyRequiredValueAccessor(() =>
                    "Липсват изпратени данни.");
                provider.SetNonPropertyAttemptedValueIsInvalidAccessor(value =>
                    $"Стойността '{value}' не е валидна.");
                provider.SetNonPropertyUnknownValueIsInvalidAccessor(() =>
                    "Подадената стойност не е валидна.");
                provider.SetNonPropertyValueMustBeANumberAccessor(() =>
                    "Стойността трябва да е число.");
                provider.SetUnknownValueIsInvalidAccessor(fieldName =>
                    $"Подадената стойност не е валидна за {fieldName}.");
                provider.SetValueIsInvalidAccessor(value =>
                    $"Стойността '{value}' не е валидна.");
                provider.SetValueMustBeANumberAccessor(fieldName =>
                    $"Полето {fieldName} трябва да е число.");
                provider.SetValueMustNotBeNullAccessor(fieldName =>
                    $"Полето {fieldName} е задължително.");
            });

            builder.Services.AddRazorPages();

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "areas",
                pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=IndexForViewerOnly}/{id?}");

            app.MapRazorPages();

            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                await db.Database.MigrateAsync();

                await IdentitySeed.SeedAsync(app.Services);
                await CoursesSeed.SeedAsync(app.Services);
                await DemoDataSeed.SeedAsync(app.Services);
            }

            app.Run();
        }
    }
}
