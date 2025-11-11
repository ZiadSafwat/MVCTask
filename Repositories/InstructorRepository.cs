using Microsoft.EntityFrameworkCore;
using mvcLab.Models;

namespace mvcLab.Repositories
{
    public interface IInstructorRepository : IGenericRepository<Instructor>
    {
        IEnumerable<Instructor> GetInstructorsWithDepartment();
        Instructor GetInstructorDetails(int id);
        IEnumerable<Course> GetAssignedCourses(int instructorId);
        bool IsEmailUnique(string email, int? excludeInstructorId = null);
    }

    public class InstructorRepository : GenericRepository<Instructor>, IInstructorRepository
    {
        public InstructorRepository(ApplicationDbContext context) : base(context) { }

        public IEnumerable<Instructor> GetInstructorsWithDepartment()
        {
            return _dbSet
                .Include(i => i.Department)
                .Include(i => i.InstructorCourses)
                    .ThenInclude(ic => ic.Course)
                .ToList();
        }

        public Instructor GetInstructorDetails(int id)
        {
            return _dbSet
                .Include(i => i.Department)
                .Include(i => i.InstructorCourses)
                    .ThenInclude(ic => ic.Course)
                .FirstOrDefault(i => i.Id == id);
        }

        public IEnumerable<Course> GetAssignedCourses(int instructorId)
        {
            return _context.InstructorCourses
                .Include(ic => ic.Course)
                .Where(ic => ic.InstructorId == instructorId)
                .Select(ic => ic.Course)
                .ToList();
        }

        public bool IsEmailUnique(string email, int? excludeInstructorId = null)
        {
            return !_dbSet.Any(i => 
                i.Email.ToLower() == email.ToLower() && 
                (!excludeInstructorId.HasValue || i.Id != excludeInstructorId.Value));
        }
    }
}