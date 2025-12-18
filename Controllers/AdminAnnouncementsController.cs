using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorkChat2.Data;           
using WorkChat2.Models;

namespace WorkChat2.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminAnnouncementsController : Controller
    {
        private readonly AppDbContext _db;
        private readonly UserManager<AppUser> _userManager;

        public AdminAnnouncementsController(AppDbContext db, UserManager<AppUser> userManager)
        { 
            _db = db;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var item = await _db.Announcements
                .OrderByDescending(a => a.IsPinned)
                .ThenByDescending(a => a.CreatedAt)
                .ToListAsync();

            return View(item);
        }

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Announcement model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.GetUserAsync(User);

            model.CreatedAt = DateTime.UtcNow;
            model.CreatedByUserId = user?.Id;

            _db.Announcements.Add(model);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Announcement Created.";
            return RedirectToAction("Announcements", "Admin");
        }

        public async Task<IActionResult> Edit(int id)
        {
            var item = await _db.Announcements.FindAsync(id);
            if (item == null) return NotFound();
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Announcement model)
        {
            if(id != model.Id) return BadRequest();
            if (!ModelState.IsValid) return View(model);

            var existing = await _db.Announcements.FindAsync(id);
            if (existing == null) return NotFound();   

            existing.Title = model.Title;
            existing.Body = model.Body;
            existing.IsPinned = model.IsPinned;
            existing.IsPublished = model.IsPublished;

            await _db.SaveChangesAsync();

            TempData["Success"] = "Announcement Updated.";
            return RedirectToAction("Announcements", "Admin");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _db.Announcements.FindAsync(id);
            if (item == null) return NotFound();

            _db.Announcements.Remove(item);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Announcement Deleted.";
            return RedirectToAction("Announcements", "Admin");
        }
    }
}
