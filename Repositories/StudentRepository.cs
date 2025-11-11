using Microsoft.EntityFrameworkCore;
using mvcLab.Models;

namespace mvcLab.Repositories
{
    public interface IStudentRepository : IGenericRepository<Student>
    {
        IEnumerable<Student> GetStudentsWithDepartment();
        IEnumerable<Student> GetStudentsWithCourses();
        Student GetStudentDetails(int id);
        IEnumerable<Course> GetAvailableCourses(int studentId);
        IEnumerable<StudCourse> GetEnrolledCourses(int studentId);
    }

    public class StudentRepository : GenericRepository<Student>, IStudentRepository
    {
        public StudentRepository(ApplicationDbContext context) : base(context) { }

        public IEnumerable<Student> GetStudentsWithDepartment()
        {
            return _dbSet.Include(s => s.Department).ToList();
        }

        public IEnumerable<Student> GetStudentsWithCourses()
        {
            return _dbSet
                .Include(s => s.Department)
                .Include(s => s.StudCourses)
                    .ThenInclude(sc => sc.Course)
                .ToList();
        }

        public Student GetStudentDetails(int id)
        {
            return _dbSet
                .Include(s => s.Department)
                .Include(s => s.StudCourses)
                    .ThenInclude(sc => sc.Course)
                .FirstOrDefault(s => s.SSN == id);
        }

        public IEnumerable<Course> GetAvailableCourses(int studentId)
        {
            var student = _dbSet
                .Include(s => s.Department)
                .Include(s => s.StudCourses)
                .FirstOrDefault(s => s.SSN == studentId);

            if (student == null) return Enumerable.Empty<Course>();

            var enrolledCourseIds = student.StudCourses.Select(sc => sc.CourseId);
            
            return _context.Courses
                .Where(c => c.DepartmentId == student.DepartmentId && !enrolledCourseIds.Contains(c.Num))
                .ToList();
        }

        public IEnumerable<StudCourse> GetEnrolledCourses(int studentId)
        {
            return _context.StudCourses
                .Include(sc => sc.Course)
                .Where(sc => sc.StudId == studentId)
                .ToList();
        }
    }
}