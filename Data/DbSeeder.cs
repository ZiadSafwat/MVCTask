using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using mvcLab.Models;

namespace mvcLab.Data
{
    public static class DbSeeder
    {
        public static async Task SeedRolesAndAdminAsync(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
            var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger("DbSeeder");
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();

            // Seeding order is important!
            await SeedRolesAsync(roleManager);
            await SeedDepartmentsAsync(context);
            await SeedCoursesAsync(context);
            await SeedAdminUserAsync(userManager, logger, configuration);
            await SeedStudentUsersAsync(userManager, context, logger, configuration);
            await SeedInstructorUsersAsync(userManager, context, logger, configuration);
        }

        private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            if (!await roleManager.RoleExistsAsync(UserRoles.Admin)) await roleManager.CreateAsync(new IdentityRole(UserRoles.Admin));
            if (!await roleManager.RoleExistsAsync(UserRoles.Student)) await roleManager.CreateAsync(new IdentityRole(UserRoles.Student));
            if (!await roleManager.RoleExistsAsync(UserRoles.Instructor)) await roleManager.CreateAsync(new IdentityRole(UserRoles.Instructor));
        }

        private static async Task SeedAdminUserAsync(UserManager<ApplicationUser> userManager, ILogger logger, IConfiguration configuration)
        {
            var adminEmail = "admin@mvclab.com";
            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                var admin = new ApplicationUser
                {
                    UserName = adminEmail, Email = adminEmail, FirstName = "System",
                    LastName = "Administrator", EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(admin, configuration["SeedUserPassword:AdminPassword"]!);
                if (result.Succeeded) await userManager.AddToRoleAsync(admin, UserRoles.Admin);
                else foreach (var error in result.Errors) logger.LogError("Error seeding admin user: {Error}", error.Description);
            }
        }

        private static async Task SeedStudentUsersAsync(UserManager<ApplicationUser> userManager, ApplicationDbContext context, ILogger logger, IConfiguration configuration)
        {
            var studentPassword = configuration["SeedUserPassword:StudentPassword"];
            var students = new List<(string email, string firstName, string lastName)> {
                ("student1@mvclab.com", "Student", "One"), ("student2@mvclab.com", "Student", "Two"),
                ("alice@mvclab.com", "Alice", "Wonderland"), ("ahmed.ali@mvclab.com", "Ahmed", "Ali"),
                ("fatima.zahra@mvclab.com", "Fatima", "Zahra"), ("omar.hassan@mvclab.com", "Omar", "Hassan"),
                ("aisha.khan@mvclab.com", "Aisha", "Khan"), ("youssef.mahmoud@mvclab.com", "Youssef", "Mahmoud"),
                ("layla.ibrahim@mvclab.com", "Layla", "Ibrahim"), ("khaled.said@mvclab.com", "Khaled", "Said"),
                ("nour.mohamed@mvclab.com", "Nour", "Mohamed"), ("tariq.abdullah@mvclab.com", "Tariq", "Abdullah"),
                ("mariam.hussein@mvclab.com", "Mariam", "Hussein")
            };

            var csDept = await context.Departments.FirstOrDefaultAsync(d => d.Name == "Computer Science");
            if (csDept == null) { logger.LogError("CS department not found."); return; }

            foreach (var (email, firstName, lastName) in students)
            {
                if (await userManager.FindByEmailAsync(email) == null)
                {
                    var user = new ApplicationUser { UserName = email, Email = email, FirstName = firstName, LastName = lastName, EmailConfirmed = true };
                    var result = await userManager.CreateAsync(user, studentPassword!);
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(user, UserRoles.Student);
                        var student = new Student { Name = $"{firstName} {lastName}", Email = email, Address = "123 Main St", Gender = "Male", Age = 20, DepartmentId = csDept.Id };
                        context.Students.Add(student);
                        await context.SaveChangesAsync();
                        user.StudentId = student.SSN;
                        await userManager.UpdateAsync(user);
                    }
                    else
                    {
                        foreach (var error in result.Errors) logger.LogError("Error seeding student {Email}: {Error}", email, error.Description);
                    }
                }
            }
        }

        private static async Task SeedInstructorUsersAsync(UserManager<ApplicationUser> userManager, ApplicationDbContext context, ILogger logger, IConfiguration configuration)
        {
            var instructorPassword = configuration["SeedUserPassword:InstructorPassword"];
            var instructors = new List<(string email, string firstName, string lastName)> {
                ("instructor1@mvclab.com", "Instructor", "One"), ("bob.ross@mvclab.com", "Bob", "Ross"),
                ("mohamed.elsayed@mvclab.com", "Mohamed", "El-Sayed"), ("hala.adel@mvclab.com", "Hala", "Adel"),
                ("ibrahim.khalil@mvclab.com", "Ibrahim", "Khalil"), ("rania.sobhi@mvclab.com", "Rania", "Sobhi"),
                ("karim.mostafa@mvclab.com", "Karim", "Mostafa"), ("dina.tarek@mvclab.com", "Dina", "Tarek"),
                ("amr.diab@mvclab.com", "Amr", "Diab"), ("faten.hamama@mvclab.com", "Faten", "Hamama"),
                ("adel.emam@mvclab.com", "Adel", "Emam"), ("soad.hosny@mvclab.com", "Soad", "Hosny")
            };
            
            var physicsDept = await context.Departments.FirstOrDefaultAsync(d => d.Name == "Physics");
            if (physicsDept == null) { logger.LogError("Physics department not found."); return; }

            foreach (var (email, firstName, lastName) in instructors)
            {
                if (await userManager.FindByEmailAsync(email) == null)
                {
                    var user = new ApplicationUser { UserName = email, Email = email, FirstName = firstName, LastName = lastName, EmailConfirmed = true };
                    var result = await userManager.CreateAsync(user, instructorPassword!);
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(user, UserRoles.Instructor);
                        var instructor = new Instructor { Name = $"{firstName} {lastName}", Email = email, Address = "456 University Ave", Age = 40, Salary = 50000, Degree = 85, DepartmentId = physicsDept.Id };
                        context.Instructors.Add(instructor);
                        await context.SaveChangesAsync(); 
                        user.InstructorId = instructor.Id;
                        await userManager.UpdateAsync(user);
                    }
                    else
                    {
                        foreach (var error in result.Errors) logger.LogError("Error seeding instructor {Email}: {Error}", email, error.Description);
                    }
                }
            }
        }

        private static async Task SeedDepartmentsAsync(ApplicationDbContext context)
        {
            if (!await context.Departments.AnyAsync())
            {
                await context.Departments.AddRangeAsync(new List<Department> {
                    new Department { Name = "Computer Science", Manager = "Dr. Smith", Location = "Building A" },
                    new Department { Name = "Mathematics", Manager = "Dr. Jones", Location = "Building B" },
                    new Department { Name = "Physics", Manager = "Dr. Brown", Location = "Building C" }
                });
                await context.SaveChangesAsync();
            }
        }

        private static async Task SeedCoursesAsync(ApplicationDbContext context)
        { 
            if (!await context.Courses.AnyAsync())
            {
                var csDept = await context.Departments.FirstOrDefaultAsync(d => d.Name == "Computer Science");
                var mathDept = await context.Departments.FirstOrDefaultAsync(d => d.Name == "Mathematics");
                var physicsDept = await context.Departments.FirstOrDefaultAsync(d => d.Name == "Physics");

                if (csDept != null && mathDept != null && physicsDept != null)
                {
                    await context.Courses.AddRangeAsync(new List<Course> {
                        new Course { Name = "Intro to Programming", Description = "C# basics", Degree = 100, MinDegree = 50, DepartmentId = csDept.Id },
                        new Course { Name = "Data Structures", Description = "Advanced data structures", Degree = 110, MinDegree = 55, DepartmentId = csDept.Id },
                        new Course { Name = "Calculus I", Description = "Limits and derivatives", Degree = 100, MinDegree = 50, DepartmentId = mathDept.Id },
                        new Course { Name = "Linear Algebra", Description = "Matrices and vectors", Degree = 110, MinDegree = 55, DepartmentId = mathDept.Id },
                        new Course { Name = "Classical Mechanics", Description = "Newtonian physics", Degree = 120, MinDegree = 60, DepartmentId = physicsDept.Id },
                        new Course { Name = "Electromagnetism", Description = "Maxwell's equations", Degree = 120, MinDegree = 60, DepartmentId = physicsDept.Id }
                    });
                    await context.SaveChangesAsync();
                }
            }
        }
    }
}