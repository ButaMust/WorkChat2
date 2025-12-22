using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorkChat2.Data;
using WorkChat2.Models;
using WorkChat2.ViewModels;

namespace WorkChat2.Controllers
{
    public class ChatController : Controller
    {
        private readonly AppDbContext _db;
        private readonly UserManager<AppUser> _userManager;

        public ChatController(AppDbContext db, UserManager<AppUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        // Shows list of rooms current user is in
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User)!;

            var rooms = await _db.ChatRoomParticipants
                .Where(p => p.UserId == userId)
                .Select(p => p.ChatRoom)
                .OrderByDescending(r => r.UpdatedAt)
                .Select(r => new ChatRoomListItemVm
                {
                    Id = r.Id,
                    Name = r.IsGroup ? (r.Name ?? "UnNamed Group") : "Direct Chat",
                    IsGroup = r.IsGroup,
                    LastUpdatedUtc = r.UpdatedAt,
                })
                .ToListAsync();

            return View(rooms);
        }

        // Open a room
        public async Task<IActionResult> Room(int id)
        {
            var userId = _userManager?.GetUserId(User)!;

            var isMember = await _db.ChatRoomParticipants.AnyAsync(p => p.ChatRoomId == id && p.UserId == userId);
            if (isMember) return Forbid();

            var room = await _db.ChatRooms
                .AsNoTracking()
                .FirstAsync(r => r.Id == id);

            var vm = new ChatRoomVm
            {
                RoomId = room.Id,
                RoomTitle = room.IsGroup ? (room.Name ?? "UnNamed Group") : "Direct Chat"
            };

            return View(vm);
        }
    }
}
