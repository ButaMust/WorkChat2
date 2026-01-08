using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorkChat2.Data;
using WorkChat2.Models;
using WorkChat2.ViewModels;

namespace WorkChat2.Controllers
{
    [Authorize]
    public class ChatController : Controller
    {
        private readonly AppDbContext _db;
        private readonly UserManager<AppUser> _userManager;

        public ChatController(AppDbContext db, UserManager<AppUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        // GET: /Chat
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User)!;

            var rooms = await _db.ChatRoomParticipants
                .AsNoTracking()
                .Where(p => p.UserId == userId)
                .Select(p => p.ChatRoom)
                .OrderByDescending(r => r.UpdatedAt)
                .Select(r => new ChatRoomListItemVm
                {
                    Id = r.Id,
                    Name = r.IsGroup ? (r.Name ?? "Unnamed Group") : "Direct Chat",
                    IsGroup = r.IsGroup,
                    LastUpdatedUtc = r.UpdatedAt,
                })
                .ToListAsync();

            return View(rooms);
        }

        // GET: /Chat/Room/5
        [HttpGet]
        public async Task<IActionResult> Room(int id)
        {
            var userId = _userManager.GetUserId(User)!;

            var isMember = await _db.ChatRoomParticipants
                .AsNoTracking()
                .AnyAsync(p => p.ChatRoomId == id && p.UserId == userId);

            if (!isMember) return Forbid();

            var room = await _db.ChatRooms
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == id);

            if (room == null) return NotFound();

            var vm = new ChatRoomVm
            {
                RoomId = room.Id,
                RoomTitle = room.IsGroup ? (room.Name ?? "Unnamed Group") : "Direct Chat"
            };

            return View(vm);
        }

        // POST: /Chat/CreateDirect
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateDirect(string otherUserId)
        {
            var myId = _userManager.GetUserId(User)!;

            if (string.IsNullOrWhiteSpace(otherUserId) || otherUserId == myId)
                return BadRequest();

            // optional safety: ensure the other user exists
            var otherExists = await _db.Users.AsNoTracking().AnyAsync(u => u.Id == otherUserId);
            if (!otherExists) return NotFound();

            // find existing direct room between these two (exactly 2 participants)
            var existingRoomId = await _db.ChatRooms
                .AsNoTracking()
                .Where(r => !r.IsGroup)
                .Where(r => _db.ChatRoomParticipants.Count(p => p.ChatRoomId == r.Id) == 2)
                .Where(r =>
                    _db.ChatRoomParticipants.Any(p => p.ChatRoomId == r.Id && p.UserId == myId) &&
                    _db.ChatRoomParticipants.Any(p => p.ChatRoomId == r.Id && p.UserId == otherUserId)
                )
                .Select(r => r.Id)
                .FirstOrDefaultAsync();

            if (existingRoomId != 0)
                return RedirectToAction("Room", new { id = existingRoomId });

            var room = new ChatRoom
            {
                IsGroup = false,
                Name = null,
                CreatedByUserId = myId
            };

            _db.ChatRooms.Add(room);
            await _db.SaveChangesAsync();

            _db.ChatRoomParticipants.AddRange(
                new ChatRoomParticipant { ChatRoomId = room.Id, UserId = myId, IsAdmin = false },
                new ChatRoomParticipant { ChatRoomId = room.Id, UserId = otherUserId, IsAdmin = false }
            );

            await _db.SaveChangesAsync();

            return RedirectToAction("Room", new { id = room.Id });
        }
    }
}
