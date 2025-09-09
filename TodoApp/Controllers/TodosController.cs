using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TodoApp.Data;
using TodoApp.Models;

namespace TodoApp.Controllers
{
    [Authorize]
    public class TodosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TodosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 👇 helper: get current user id
        private string? CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier);

        // GET: /Todos
        public async Task<IActionResult> Index(string filter = "all", string sort = "duedate_asc")
        {
            var userId = CurrentUserId;

            var query = _context.Todos
                .Where(t => t.UserId == userId)
                .Include(t => t.Category)
                .AsQueryable();

            // ✅ Apply filter
            switch (filter.ToLower())
            {
                case "completed":
                    query = query.Where(t => t.IsCompleted);
                    break;
                case "pending":
                    query = query.Where(t => !t.IsCompleted);
                    break;
                case "all":
                default:
                    break;
            }

            // ✅ Apply sorting
            switch (sort.ToLower())
            {
                case "duedate_desc":
                    query = query.OrderByDescending(t => t.DueDate);
                    break;
                case "duedate_asc":
                default:
                    query = query.OrderBy(t => t.DueDate);
                    break;
            }

            var todos = await query.ToListAsync();

            // Pass selected filter & sort to View for UI
            ViewBag.CurrentFilter = filter;
            ViewBag.CurrentSort = sort;

            return View(todos);
        }


        // GET: /Todos/Create
        public IActionResult Create()
        {
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name");
            return View();
        }

        // POST: /Todos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Todo todo)
        {
            if (ModelState.IsValid)
            {
                // assign todo to logged-in user
                todo.UserId = CurrentUserId;

                _context.Todos.Add(todo);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", todo.CategoryId);
            return View(todo);
        }

        // GET: /Todos/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var todo = await _context.Todos
                .Where(t => t.Id == id && t.UserId == CurrentUserId)
                .FirstOrDefaultAsync();

            if (todo == null)
            {
                return NotFound();
            }

            ViewBag.Categories = await _context.Categories.ToListAsync();
            return View(todo);
        }

        // POST: /Todos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Todo todo)
        {
            if (id != todo.Id) return BadRequest();

            if (ModelState.IsValid)
            {
                // make sure user owns this todo
                var existing = await _context.Todos
                    .Where(t => t.Id == id && t.UserId == CurrentUserId)
                    .FirstOrDefaultAsync();

                if (existing == null)
                {
                    return NotFound();
                }

                // update allowed fields
                existing.Title = todo.Title;
                existing.Description = todo.Description;
                existing.DueDate = todo.DueDate;
                existing.IsCompleted = todo.IsCompleted;
                existing.CategoryId = todo.CategoryId;

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Categories = await _context.Categories.ToListAsync();
            return View(todo);
        }

        // GET: /Todos/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var todo = await _context.Todos
                .Where(t => t.Id == id && t.UserId == CurrentUserId)
                .FirstOrDefaultAsync();

            if (todo == null)
            {
                return NotFound();
            }

            return View(todo);
        }

        // POST: /Todos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var todo = await _context.Todos
                .Where(t => t.Id == id && t.UserId == CurrentUserId)
                .FirstOrDefaultAsync();

            if (todo == null)
            {
                return NotFound();
            }

            _context.Todos.Remove(todo);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
