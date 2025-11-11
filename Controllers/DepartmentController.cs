using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using mvcLab.Models;

namespace mvcLab.Controllers
{
    [Authorize]
    public class DepartmentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DepartmentController> _logger;

        public DepartmentController(ApplicationDbContext context, ILogger<DepartmentController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [AllowAnonymous]
        public IActionResult Index()
        {
            var depts = _context.Departments.ToList();
            return View(depts);
        }

        [AllowAnonymous]
        public IActionResult Details(int id)
        {
            var dept = _context.Departments
                .Include(d => d.Instructors)
                .Include(d => d.Students)
                .FirstOrDefault(d => d.Id == id);

            if (dept == null) return NotFound();
            return View(dept);
        }

        public IActionResult Create()
        {
             
            return View(new Department { Name = string.Empty, Manager = string.Empty, Location = string.Empty });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Department department)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Departments.Add(department);
                    _context.SaveChanges();
                    TempData["Success"] = "Department created.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating department");
                    ModelState.AddModelError("", "Error creating department");
                }
            }

            return View(department);
        }

        public IActionResult Edit(int id)
        {
            var dept = _context.Departments.Find(id);
            if (dept == null) return NotFound();
            return View(dept);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Department department)
        {
            if (id != department.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(department);
                    _context.SaveChanges();
                    TempData["Success"] = "Department updated.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    if (!_context.Departments.Any(d => d.Id == id)) return NotFound();
                    _logger.LogError(ex, "Concurrency error updating department");
                    ModelState.AddModelError("", "Concurrency error updating department");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating department");
                    ModelState.AddModelError("", "Error updating department");
                }
            }

            return View(department);
        }

        public IActionResult Delete(int id)
        {
            var dept = _context.Departments
                .Include(d => d.Instructors)
                .Include(d => d.Students)
                .FirstOrDefault(d => d.Id == id);

            if (dept == null) return NotFound();
            return View(dept);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var dept = _context.Departments.Find(id);
            if (dept == null) return NotFound();

            try
            {
                _context.Departments.Remove(dept);
                _context.SaveChanges();
                TempData["Success"] = "Department deleted.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting department");
                TempData["Error"] = "Error deleting department.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
