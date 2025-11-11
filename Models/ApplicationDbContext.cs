using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace mvcLab.Models
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Student> Students { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<StudCourse> StudCourses { get; set; }
        public DbSet<Instructor> Instructors { get; set; }
        public DbSet<InstructorCourse> InstructorCourses { get; set; }
        public DbSet<Department> Departments { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            
            builder.Entity<Course>()
                .HasIndex(c => new { c.Name, c.DepartmentId })
                .IsUnique();

            builder.Entity<Student>()
                .HasIndex(s => s.Email)
                .IsUnique();

            builder.Entity<Instructor>()
                .HasIndex(i => i.Email)
                .IsUnique();

            
            builder.Entity<ApplicationUser>()
                .HasOne(u => u.Student)
                .WithOne()
                .HasForeignKey<ApplicationUser>(u => u.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ApplicationUser>()
                .HasOne(u => u.Instructor)
                .WithOne()
                .HasForeignKey<ApplicationUser>(u => u.InstructorId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}