using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using mvcLab.Models;

namespace mvcLab.Controllers
{
    public class InstructorCourseController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<InstructorCourseController> _logger;

        public InstructorCourseController(ApplicationDbContext context, ILogger<InstructorCourseController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public IActionResult Index()
        {
            var list = _context.InstructorCourses
                .Include(ic => ic.Instructor)
                .Include(ic => ic.Course)
                .ToList();
            return View(list);
        }

        public IActionResult Create()
        {
            PopulateInstructors();
            PopulateCourses();
            return View(new InstructorCourse());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(InstructorCourse ic)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.InstructorCourses.Add(ic);
                    _context.SaveChanges();
                    TempData["Success"] = "Assignment added.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error adding assignment");
                    ModelState.AddModelError("", "Error adding assignment");
                }
            }

            PopulateInstructors(ic.InstructorId);
            PopulateCourses(ic.CourseId);
            return View(ic);
        }

        public IActionResult Delete(int id)
        {
            var item = _context.InstructorCourses
                .Include(ic => ic.Instructor)
                .Include(ic => ic.Course)
                .FirstOrDefault(ic => ic.Id == id);

            if (item == null) return NotFound();
            return View(item);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var item = _context.InstructorCourses.Find(id);
            if (item == null) return NotFound();
            try
            {
                _context.InstructorCourses.Remove(item);
                _context.SaveChanges();
                TempData["Success"] = "Assignment removed.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing assignment");
                TempData["Error"] = "Error removing assignment.";
            }

            return RedirectToAction(nameof(Index));
        }

        private void PopulateInstructors(int? selected = null)
        {
            ViewBag.Instructors = _context.Instructors.Select(i => new SelectListItem
            {
                Value = i.Id.ToString(),
                Text = i.Name,
                Selected = selected.HasValue && i.Id == selected.Value
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
