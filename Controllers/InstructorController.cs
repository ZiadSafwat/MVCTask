using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using mvcLab.Models;

namespace mvcLab.Controllers
{
    [Authorize]
    public class InstructorController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<InstructorController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;

        public InstructorController(ApplicationDbContext context, ILogger<InstructorController> logger, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _logger = logger;
            _userManager = userManager;
        }

        [Authorize(Roles = UserRoles.Admin)]
        public IActionResult Index()
        {
            var instructors = _context.Instructors.Include(i => i.Department).ToList();
            return View(instructors);
        }
        [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.Instructor}")]
        public async Task<IActionResult> Details(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (User.IsInRole(UserRoles.Instructor))
            {
                if (currentUser?.InstructorId == null || currentUser.InstructorId != id)
                {
                    return Forbid();
                }
            }

            var instructor = _context.Instructors
                .Include(i => i.Department)
                .Include(i => i.InstructorCourses)
                    .ThenInclude(ic => ic.Course)
                .FirstOrDefault(i => i.Id == id);

            if (instructor == null) return NotFound();
            return View(instructor);
        }

        [Authorize(Roles = UserRoles.Admin)]
        public IActionResult Create()
        {
            PopulateDepartments();
            
            return View(new Instructor { Name = string.Empty, Email = string.Empty, Address = string.Empty });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = UserRoles.Admin)]
        public IActionResult Create(Instructor instructor)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Instructors.Add(instructor);
                    _context.SaveChanges();
                    TempData["Success"] = "Instructor created.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating instructor");
                    ModelState.AddModelError("", "Error creating instructor");
                }
            }

            PopulateDepartments(instructor.DepartmentId);
            return View(instructor);
        }

        [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.Instructor}")]
        public async Task<IActionResult> Edit(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var instructor = _context.Instructors.Find(id);
            if (instructor == null) return NotFound();

            if (User.IsInRole(UserRoles.Instructor))
            {
                if (currentUser?.InstructorId == null || currentUser.InstructorId != id)
                {
                    return Forbid();
                }
            }

            PopulateDepartments(instructor.DepartmentId);
            return View(instructor);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.Instructor}")]
        public IActionResult Edit(int id, Instructor instructor)
        {
            if (id != instructor.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(instructor);
                    _context.SaveChanges();
                    TempData["Success"] = "Instructor updated.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    if (!_context.Instructors.Any(i => i.Id == id)) return NotFound();
                    _logger.LogError(ex, "Concurrency error updating instructor");
                    ModelState.AddModelError("", "Concurrency error updating instructor");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating instructor");
                    ModelState.AddModelError("", "Error updating instructor");
                }
            }

            PopulateDepartments(instructor.DepartmentId);
            return View(instructor);
        }

    [Authorize(Roles = UserRoles.Admin)]
    public IActionResult Delete(int id)
        {
            var instructor = _context.Instructors
                .Include(i => i.Department)
                .FirstOrDefault(i => i.Id == id);

            if (instructor == null) return NotFound();
            return View(instructor);
        }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = UserRoles.Admin)]
    public IActionResult DeleteConfirmed(int id)
        {
            var instructor = _context.Instructors.Find(id);
            if (instructor == null) return NotFound();

            try
            {
                _context.Instructors.Remove(instructor);
                _context.SaveChanges();
                TempData["Success"] = "Instructor deleted.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting instructor");
                TempData["Error"] = "Error deleting instructor.";
            }

            return RedirectToAction(nameof(Index));
        }

        [AcceptVerbs("GET", "POST")]
        public IActionResult IsEmailUnique(string email, int? id)
        {
            if (string.IsNullOrWhiteSpace(email)) return Json(true);
            var exists = _context.Instructors.Any(i => i.Email.ToLower() == email.ToLower() && (id == null || i.Id != id.Value));
            return Json(!exists);
        }

        private void PopulateDepartments(int? selectedId = null)
        {
            ViewBag.Departments = _context.Departments.Select(d => new SelectListItem
            {
                Value = d.Id.ToString(),
                Text = d.Name,
                Selected = selectedId.HasValue && d.Id == selectedId.Value
            }).ToList();
        }
    }
}
