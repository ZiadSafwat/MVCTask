using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using mvcLab.Models;

namespace mvcLab.Controllers
{
    [Authorize]
    public class CourseController : Controller
    {
        private readonly ApplicationDbContext context;
        private readonly ILogger<CourseController> _logger;

        public CourseController(ApplicationDbContext _context, ILogger<CourseController> logger)
        {
            context = _context;
            _logger = logger;
        }

        [Authorize(Roles = UserRoles.Admin)]
        public IActionResult Index()
        {
            var courses = context.Courses.ToList();
            return View(courses);
        }

        [Authorize(Roles = UserRoles.Admin)]
        public IActionResult Create()
        {
            return View(new Course { Name = string.Empty });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = UserRoles.Admin)]
        public IActionResult Create(Course course)
        {
            if (course.MinDegree > course.Degree)
            {
                ModelState.AddModelError("MinDegree", "Minimum degree cannot be greater than total degree");
            }

           
            var existingCourseForDeptCheck = context.Courses
                .Include(c => c.StudCourses)
                .Include(c => c.InstructorCourses)
                .FirstOrDefault(c => c.Num == course.Num);

            if (existingCourseForDeptCheck != null && existingCourseForDeptCheck.DepartmentId != course.DepartmentId)
            {
                if (existingCourseForDeptCheck.StudCourses.Any() || existingCourseForDeptCheck.InstructorCourses.Any())
                {
                    _logger.LogWarning("Attempted to change department for course with existing relationships: {CourseId}", course.Num);
                    ModelState.AddModelError("DepartmentId", "Cannot change department for a course that has students or instructors assigned");
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    context.Courses.Add(course);
                    context.SaveChanges();
                    TempData["Success"] = $"Course '{course.Name}' created successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating course: {CourseName}", course.Name);
                    ModelState.AddModelError("", "An error occurred while creating the course. Please try again.");
                }
            }

            return View(course);
        }

        [Authorize(Roles = UserRoles.Admin)]
        public IActionResult Edit(int id)
        {
            var course = context.Courses.Find(id);
            if (course == null)
            {
                return NotFound();
            }

            return View(course);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = UserRoles.Admin)]
        public IActionResult Edit(int id, Course course)
        {
            if (id != course.Num)
            {
                _logger.LogWarning("Course ID mismatch: URL ID {UrlId} != Model ID {ModelId}", id, course.Num);
                return NotFound();
            }

            if (course.MinDegree > course.Degree)
            {
                ModelState.AddModelError("MinDegree", "Minimum degree cannot be greater than total degree");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    context.Update(course);
                    context.SaveChanges();
                    _logger.LogInformation("Course updated successfully: {CourseName}", course.Name);
                    TempData["Success"] = "Course updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    if (!CourseExists(course.Num))
                    {
                        _logger.LogWarning(ex, "Course not found during update: {CourseId}", course.Num);
                        return NotFound();
                    }
                    else
                    {
                        _logger.LogError(ex, "Concurrency error updating course: {CourseName}", course.Name);
                        ModelState.AddModelError("", "This course has been modified by another user. Please reload and try again.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating course: {CourseName}", course.Name);
                    ModelState.AddModelError("", "An error occurred while updating the course. Please try again.");
                }
            }

            return View(course);
        }

        private bool CourseExists(int id)
        {
            return context.Courses.Any(c => c.Num == id);
        }

        [AcceptVerbs("GET", "POST")]
        public IActionResult IsNameUnique(string name, int departmentId, int? num)
        {
            var exists = context.Courses.Any(c => 
                c.Name.ToLower() == name.ToLower() && 
                c.DepartmentId == departmentId && 
                (num == null || c.Num != num.Value));
            
            return Json(!exists);
        }
    }
}
