using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorkChat2.Models;
using WorkChat2.ViewModels;
using WorkChat2.Data;

namespace WorkChat2.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly AppDbContext _db;

        public AdminController(UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager, AppDbContext db)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _db = db;
        }

        public IActionResult Index() => View();

        // ----------------------------
        // CREATE USER
        // ----------------------------

        // GET: /Admin/CreateUser
        [HttpGet]
        public IActionResult CreateUser()
        {
            return View();
        }

        // POST: /Admin/CreateUser
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(CreateUserViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var existing = await _userManager.FindByEmailAsync(model.Email);
            if (existing != null)
            {
                ModelState.AddModelError("", "A user with this email already exists.");
                return View(model);
            }

            // IMPORTANT: Use Email as UserName so default Identity login works with email
            var user = new AppUser
            {
                UserName = model.Email,
                Email = model.Email,
                Name = model.Name,
                LastName = model.LastName,
                EmailConfirmed = true
            };

            var createResult = await _userManager.CreateAsync(user, model.Password);

            if (!createResult.Succeeded)
            {
                foreach (var error in createResult.Errors)
                    ModelState.AddModelError("", error.Description);

                return View(model);
            }

            if (model.IsAdmin)
            {
                if (!await _roleManager.RoleExistsAsync("Admin"))
                    await _roleManager.CreateAsync(new IdentityRole("Admin"));

                var roleResult = await _userManager.AddToRoleAsync(user, "Admin");
                if (!roleResult.Succeeded)
                {
                    TempData["Error"] = string.Join(" | ", roleResult.Errors.Select(e => e.Description));
                    return RedirectToAction(nameof(Users));
                }
            }

            TempData["Success"] = $"Created user {user.Email}.";
            return RedirectToAction(nameof(Users));
        }

        // ----------------------------
        // USERS LIST
        // ----------------------------

        // GET: /Admin/Users?q=...&page=1&pageSize=10
        [HttpGet]
        public async Task<IActionResult> Users(string? q, int page = 1, int pageSize = 10)
        {
            page = page < 1 ? 1 : page;
            pageSize = pageSize is < 5 or > 100 ? 10 : pageSize;

            var query = _userManager.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                var s = q.Trim();
                query = query.Where(u =>
                    (u.Email != null && u.Email.Contains(s)) ||
                    (u.UserName != null && u.UserName.Contains(s)) ||
                    u.Name.Contains(s) ||
                    u.LastName.Contains(s));
            }

            var totalCount = await query.CountAsync();

            var users = await query
                .OrderBy(u => u.Email)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var list = new List<AdminUserListItemVm>();
            foreach (var u in users)
            {
                var roles = await _userManager.GetRolesAsync(u);

                list.Add(new AdminUserListItemVm
                {
                    Id = u.Id,
                    Email = u.Email ?? "",
                    UserName = u.UserName ?? "",
                    Name = u.Name,
                    LastName = u.LastName,
                    CreatedAt = u.CreatedAt,
                    UpdatedAt = u.UpdatedAt,
                    Roles = roles.ToList()
                });
            }

            var vm = new AdminUsersPageVm
            {
                Users = list,
                Q = q,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount
            };

            return View(vm);
        }

        // ----------------------------
        // TOGGLE ADMIN
        // ----------------------------

        // POST: /Admin/ToggleAdmin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleAdmin(string id, string? q, int page = 1, int pageSize = 10)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var currentUserId = _userManager.GetUserId(User);
            if (user.Id == currentUserId)
            {
                TempData["Error"] = "You can't change your own admin role.";
                return RedirectToAction(nameof(Users), new { q, page, pageSize });
            }

            if (!await _roleManager.RoleExistsAsync("Admin"))
                await _roleManager.CreateAsync(new IdentityRole("Admin"));

            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            IdentityResult result = isAdmin
                ? await _userManager.RemoveFromRoleAsync(user, "Admin")
                : await _userManager.AddToRoleAsync(user, "Admin");

            if (!result.Succeeded)
                TempData["Error"] = string.Join(" | ", result.Errors.Select(e => e.Description));

            return RedirectToAction(nameof(Users), new { q, page, pageSize });
        }

        // ----------------------------
        // RESET PASSWORD
        // ----------------------------

        // GET: /Admin/ResetPassword?id=...
        [HttpGet]
        public async Task<IActionResult> ResetPassword(string id, string? q, int page = 1, int pageSize = 10)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var vm = new AdminResetPasswordVm
            {
                UserId = user.Id,
                Email = user.Email ?? ""
            };

            ViewBag.Q = q;
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;

            return View(vm);
        }

        // POST: /Admin/ResetPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(AdminResetPasswordVm vm, string? q, int page = 1, int pageSize = 10)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Q = q;
                ViewBag.Page = page;
                ViewBag.PageSize = pageSize;
                return View(vm);
            }

            var user = await _userManager.FindByIdAsync(vm.UserId);
            if (user == null) return NotFound();

            var currentUserId = _userManager.GetUserId(User);
            if (user.Id == currentUserId)
            {
                ModelState.AddModelError("", "You can't reset your own password here.");
                ViewBag.Q = q;
                ViewBag.Page = page;
                ViewBag.PageSize = pageSize;
                return View(vm);
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, vm.NewPassword);

            if (!result.Succeeded)
            {
                foreach (var e in result.Errors)
                    ModelState.AddModelError("", e.Description);

                ViewBag.Q = q;
                ViewBag.Page = page;
                ViewBag.PageSize = pageSize;
                return View(vm);
            }

            TempData["Success"] = $"Password reset for {user.Email}.";
            return RedirectToAction(nameof(Users), new { q, page, pageSize });
        }

        // ----------------------------
        // DELETE USER
        // ----------------------------

        // POST: /Admin/DeleteUser
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string id, string? q, int page = 1, int pageSize = 10)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var currentUserId = _userManager.GetUserId(User);
            if (user.Id == currentUserId)
            {
                TempData["Error"] = "You can't delete your own account.";
                return RedirectToAction(nameof(Users), new { q, page, pageSize });
            }

            if (string.Equals(user.Email, "admin@local", StringComparison.OrdinalIgnoreCase))
            {
                TempData["Error"] = "You can't delete the seeded admin account.";
                return RedirectToAction(nameof(Users), new { q, page, pageSize });
            }

            var result = await _userManager.DeleteAsync(user);

            if (!result.Succeeded)
                TempData["Error"] = string.Join(" | ", result.Errors.Select(e => e.Description));
            else
                TempData["Success"] = $"Deleted user {user.Email}.";

            return RedirectToAction(nameof(Users), new { q, page, pageSize });
        }

        // GET: /Admin/Announcements
        [HttpGet]
        public async Task<IActionResult> Announcements()
        {
            var items = await _db.Announcements
                .OrderByDescending(a => a.IsPinned)
                .ThenByDescending(a => a.CreatedAt)
                .ToListAsync();

            return View(items); // will look in Views/Admin/Announcements.cshtml
        }

    }
}
