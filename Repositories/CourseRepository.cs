using Microsoft.EntityFrameworkCore;
using mvcLab.Models;

namespace mvcLab.Repositories
{
    public interface ICourseRepository : IGenericRepository<Course>
    {
        IEnumerable<Course> GetCoursesWithDepartment();
        Course GetCourseDetails(int id);
        bool IsNameUniqueInDepartment(string name, int departmentId, int? excludeCourseId = null);
    }

    public class CourseRepository : GenericRepository<Course>, ICourseRepository
    {
        public CourseRepository(ApplicationDbContext context) : base(context) { }

        public IEnumerable<Course> GetCoursesWithDepartment()
        {
            return _dbSet
                .Include(c => c.Department)
                .Include(c => c.StudCourses)
                .Include(c => c.InstructorCourses)
                .ToList();
        }

        public Course GetCourseDetails(int id)
        {
            return _dbSet
                .Include(c => c.Department)
                .Include(c => c.StudCourses)
                    .ThenInclude(sc => sc.Student)
                .Include(c => c.InstructorCourses)
                    .ThenInclude(ic => ic.Instructor)
                .FirstOrDefault(c => c.Num == id);
        }

        public bool IsNameUniqueInDepartment(string name, int departmentId, int? excludeCourseId = null)
        {
            return !_dbSet.Any(c => 
                c.Name.ToLower() == name.ToLower() && 
                c.DepartmentId == departmentId && 
                (!excludeCourseId.HasValue || c.Num != excludeCourseId.Value));
        }
    }
}