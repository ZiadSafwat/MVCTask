using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using mvcLab.Models;

namespace mvcLab.Controllers
{
    [Authorize]
    public class StudentCourseController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<StudentCourseController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;

        public StudentCourseController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, ILogger<StudentCourseController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        public IActionResult Index()
        {
            var list = _context.StudCourses
                .Include(sc => sc.Student)
                .Include(sc => sc.Course)
                .ToList();
            return View(list);
        }

        [Authorize(Roles = UserRoles.Student)]
        public async Task<IActionResult> MyCourses()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser?.StudentId == null)
            {
                return NotFound("Student profile not found");
            }

            var studentCourses = await _context.StudCourses
                .Include(sc => sc.Course)
                .Include(sc => sc.Student)
                .Where(sc => sc.StudId == currentUser.StudentId)
                .ToListAsync();

            return View(studentCourses);
        }

        [Authorize(Roles = UserRoles.Student)]
        public async Task<IActionResult> EnrollCourse()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser?.StudentId == null)
            {
                return NotFound("Student profile not found");
            }

            var student = await _context.Students
                .Include(s => s.StudCourses)
                .FirstOrDefaultAsync(s => s.SSN == currentUser.StudentId);

            if (student == null)
            {
                return NotFound("Student not found");
            }

            var enrolledCourseIds = student.StudCourses.Select(sc => sc.CourseId).Where(id => id.HasValue).Select(id => id!.Value).ToList();
            var availableCourses = await _context.Courses
                .Where(c => !enrolledCourseIds.Contains(c.Num))
                .ToListAsync();

            ViewBag.AvailableCourses = availableCourses;
            return View();
        }

        [HttpPost]
        [Authorize(Roles = UserRoles.Student)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EnrollCourse(int courseId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser?.StudentId == null)
            {
                return NotFound("Student profile not found");
            }

            var course = await _context.Courses.FirstOrDefaultAsync(c => c.Num == courseId);
            if (course == null)
            {
                return NotFound("Course not found");
            }

            var existing = await _context.StudCourses.AnyAsync(sc => sc.StudId == currentUser.StudentId && sc.CourseId == courseId);
            if (existing)
            {
                TempData["Error"] = "You are already enrolled in this course.";
                return RedirectToAction(nameof(MyCourses));
            }

            var enrollment = new StudCourse
            {
                StudId = currentUser.StudentId,
                CourseId = courseId,
                Grade = 0
            };

            try
            {
                _context.StudCourses.Add(enrollment);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Successfully enrolled in the course.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enrolling in course");
                TempData["Error"] = "An error occurred while enrolling in the course.";
            }

            return RedirectToAction(nameof(MyCourses));
        }

        [HttpPost]
        [Authorize(Roles = UserRoles.Student)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DropCourse(int courseId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser?.StudentId == null)
            {
                return NotFound("Student profile not found");
            }

            var enrollment = await _context.StudCourses
                .FirstOrDefaultAsync(sc => sc.StudId == currentUser.StudentId && sc.CourseId == courseId && sc.Grade == 0);

            if (enrollment == null)
            {
                TempData["Error"] = "Enrollment not found or course cannot be dropped after grading.";
                return RedirectToAction(nameof(MyCourses));
            }

            try
            {
                _context.StudCourses.Remove(enrollment);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Successfully dropped the course.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error dropping course");
                TempData["Error"] = "An error occurred while dropping the course.";
            }

            return RedirectToAction(nameof(MyCourses));
        }

        public IActionResult Create()
        {
            PopulateStudents();
            PopulateCourses();
            return View(new StudCourse());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(StudCourse sc)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.StudCourses.Add(sc);
                    _context.SaveChanges();
                    TempData["Success"] = "Enrollment added.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error adding enrollment");
                    ModelState.AddModelError("", "Error adding enrollment");
                }
            }

            PopulateStudents(sc.StudId);
            PopulateCourses(sc.CourseId);
            return View(sc);
        }

        public IActionResult Delete(int id)
        {
            var item = _context.StudCourses
                .Include(sc => sc.Student)
                .Include(sc => sc.Course)
                .FirstOrDefault(sc => sc.Id == id);
            if (item == null) return NotFound();
            return View(item);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var item = _context.StudCourses.Find(id);
            if (item == null) return NotFound();
            try
            {
                _context.StudCourses.Remove(item);
                _context.SaveChanges();
                TempData["Success"] = "Enrollment removed.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing enrollment");
                TempData["Error"] = "Error removing enrollment.";
            }

            return RedirectToAction(nameof(Index));
        }

        private void PopulateStudents(int? selected = null)
        {
            ViewBag.Students = _context.Students.Select(s => new SelectListItem
            {
                Value = s.SSN.ToString(),
                Text = s.Name,
                Selected = selected.HasValue && s.SSN == selected.Value
            }).ToList();
        }

        private void PopulateCourses(int? selected = null)
        {
            ViewBag.Courses = _context.Courses.Select(c => new SelectListItem
            {
                Value = c.Num.ToString(),
                Text = c.Name,
                Selected = selected.HasValue && c.Num == selected.Value
            }).ToList();
        }
    }
}
