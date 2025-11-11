using Microsoft.EntityFrameworkCore;
using mvcLab.Models;

namespace mvcLab.Repositories
{
    public interface IDepartmentRepository : IGenericRepository<Department>
    {
        Department GetDepartmentDetails(int id);
        IEnumerable<Course> GetDepartmentCourses(int departmentId);
        IEnumerable<Instructor> GetDepartmentInstructors(int departmentId);
        IEnumerable<Student> GetDepartmentStudents(int departmentId);
    }

    public class DepartmentRepository : GenericRepository<Department>, IDepartmentRepository
    {
        public DepartmentRepository(ApplicationDbContext context) : base(context) { }

        public Department GetDepartmentDetails(int id)
        {
            return _dbSet
                .Include(d => d.Courses)
                .Include(d => d.Instructors)
                .Include(d => d.Students)
                .FirstOrDefault(d => d.Id == id);
        }

        public IEnumerable<Course> GetDepartmentCourses(int departmentId)
        {
            return _context.Courses
                .Include(c => c.StudCourses)
                .Include(c => c.InstructorCourses)
                .Where(c => c.DepartmentId == departmentId)
                .ToList();
        }

        public IEnumerable<Instructor> GetDepartmentInstructors(int departmentId)
        {
            return _context.Instructors
                .Include(i => i.InstructorCourses)
                    .ThenInclude(ic => ic.Course)
                .Where(i => i.DepartmentId == departmentId)
                .ToList();
        }

        public IEnumerable<Student> GetDepartmentStudents(int departmentId)
        {
            return _context.Students
                .Include(s => s.StudCourses)
                    .ThenInclude(sc => sc.Course)
                .Where(s => s.DepartmentId == departmentId)
                .ToList();
        }
    }
}