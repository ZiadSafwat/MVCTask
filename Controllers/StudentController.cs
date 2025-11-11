using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using mvcLab.Models;

namespace mvcLab.Controllers
{
    [Authorize]
    public class StudentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<StudentController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;

        public StudentController(ApplicationDbContext context, ILogger<StudentController> logger, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _logger = logger;
            _userManager = userManager;
        }

        

         [Authorize(Roles = UserRoles.Admin)]
        public IActionResult Index()
        {
            var students = _context.Students
                .Include(s => s.Department)
                .Include(s => s.StudCourses)
                    .ThenInclude(sc => sc.Course)
                .ToList();

            return View(students);
        }

         [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.Student}")]
        public async Task<IActionResult> Details(int id)
        {
          
            var currentUser = await _userManager.GetUserAsync(User);
            if (User.IsInRole(UserRoles.Student))
            {
                if (currentUser?.StudentId == null || currentUser.StudentId != id)
                {
                    return Forbid();
                }
            }

            var student = _context.Students
                .Include(s => s.Department)
                .Include(s => s.StudCourses)
                    .ThenInclude(sc => sc.Course)
                .FirstOrDefault(s => s.SSN == id);

            if (student == null)
            {
                _logger.LogWarning("Student not found: {StudentId}", id);
                return NotFound();
            }

            return View(student);
        }

       
        [Authorize(Roles = UserRoles.Admin)]
        public IActionResult Create()
        {
            PopulateDepartmentsDropdown();
            return View();
        }

    
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = UserRoles.Admin)]
    public IActionResult Create(Student student)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    foreach (var modelState in ModelState.Values)
                    {
                        foreach (var error in modelState.Errors)
                        {
                            _logger.LogWarning("Validation error: {Error}", error.ErrorMessage);
                        }
                    }
                    PopulateDepartmentsDropdown(student?.DepartmentId);
                    return View(student);
                }

                var existingEmail = _context.Students.Any(s => s.Email.ToLower() == student.Email.ToLower());
                if (existingEmail)
                {
                    ModelState.AddModelError("Email", "This email is already registered");
                    PopulateDepartmentsDropdown(student.DepartmentId);
                    return View(student);
                }

                _context.Students.Add(student);
                _context.SaveChanges();
                
                _logger.LogInformation("Student created successfully: {StudentName} (Email: {Email})", student.Name, student.Email);
                TempData["Success"] = "Student created successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating student: {StudentName} - {Error}", student?.Name ?? "Unknown", ex.Message);
                ModelState.AddModelError("", "An error occurred while creating the student. Please try again.");
                PopulateDepartmentsDropdown(student?.DepartmentId);
                return View(student);
            }
        }

        
        [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.Student}")]
        public async Task<IActionResult> Edit(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var student = _context.Students
                .Include(s => s.Department)
                .FirstOrDefault(s => s.SSN == id);

            if (student == null)
            {
                _logger.LogWarning("Student not found for edit: {StudentId}", id);
                return NotFound();
            }

           
            if (User.IsInRole(UserRoles.Student))
            {
                if (currentUser?.StudentId == null || currentUser.StudentId != id)
                {
                    return Forbid();
                }
            }

            PopulateDepartmentsDropdown(student.DepartmentId);
            return View(student);
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.Student}")]
        public async Task<IActionResult> Edit(int id, Student student)
        {
            if (id != student.SSN)
            {
                _logger.LogWarning("Student ID mismatch: URL ID {UrlId} != Model ID {ModelId}", id, student.SSN);
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);

           
            if (User.IsInRole(UserRoles.Student))
            {
                if (currentUser?.StudentId == null || currentUser.StudentId != id)
                {
                    return Forbid();
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(student);
                    _context.SaveChanges();
                    _logger.LogInformation("Student updated successfully: {StudentName}", student.Name);
                    TempData["Success"] = "Student updated successfully!";
                    if (User.IsInRole(UserRoles.Student))
                    {
                        return RedirectToAction(nameof(MyProfile));
                    }
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    if (!StudentExists(student.SSN))
                    {
                        _logger.LogWarning(ex, "Student not found during update: {StudentId}", student.SSN);
                        return NotFound();
                    }
                    else
                    {
                        _logger.LogError(ex, "Concurrency error updating student: {StudentName}", student.Name);
                        ModelState.AddModelError("", "This student has been modified by another user. Please reload and try again.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating student: {StudentName}", student.Name);
                    ModelState.AddModelError("", "An error occurred while updating the student. Please try again.");
                }
            }

            PopulateDepartmentsDropdown(student.DepartmentId);
            return View(student);
        }

     
    [Authorize(Roles = UserRoles.Admin)]
    public IActionResult Delete(int id)
        {
            var student = _context.Students
                .Include(s => s.Department)
                .Include(s => s.StudCourses)
                    .ThenInclude(sc => sc.Course)
                .FirstOrDefault(s => s.SSN == id);

            if (student == null)
            {
                return NotFound();
            }

            return View(student);
        }

    
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = UserRoles.Admin)]
    public IActionResult DeleteConfirmed(int id)
        {
            var student = _context.Students
                .Include(s => s.StudCourses)
                .FirstOrDefault(s => s.SSN == id);

            if (student != null)
            {
                try
                {
                    if (student.StudCourses.Any())
                    {
                        _context.StudCourses.RemoveRange(student.StudCourses);
                    }

                    _context.Students.Remove(student);
                    _context.SaveChanges();
                    TempData["Success"] = "Student deleted successfully!";
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error deleting student: {StudentId}", id);
                    TempData["Error"] = "An error occurred while deleting the student. Please try again.";
                }
            }

            return RedirectToAction(nameof(Index));
        }

        
        [Authorize(Roles = UserRoles.Student)]
        public async Task<IActionResult> MyProfile()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser?.StudentId == null)
            {
                return Forbid();
            }

            return await Details(currentUser.StudentId.Value);
        }

        [AcceptVerbs("GET", "POST")]
        public IActionResult IsStudentEmailUnique(string email, int? ssn)
        {
            if (string.IsNullOrWhiteSpace(email))
                return Json(true);

            var exists = _context.Students.Any(s =>
                s.Email.ToLower() == email.ToLower() &&
                (ssn == null || s.SSN != ssn.Value));

            return Json(!exists);
        }

        private bool StudentExists(int id)
        {
            return _context.Students.Any(e => e.SSN == id);
        }

        private void PopulateDepartmentsDropdown(int? selectedDepartmentId = null)
        {
            var departments = _context.Departments
                .Select(d => new SelectListItem
                {
                    Value = d.Id.ToString(),
                    Text = d.Name,
                    Selected = selectedDepartmentId.HasValue && d.Id == selectedDepartmentId.Value
                })
                .ToList();

            ViewBag.Departments = departments;
        }
    }
}