using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorkChat2.Data;
using WorkChat2.Models;
using WorkChat2.ViewModels;

namespace WorkChat2.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ChatAdminController : Controller
    {
        private readonly AppDbContext _db;
        private readonly UserManager<AppUser> _userManager;

        public ChatAdminController(AppDbContext db, UserManager<AppUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        // GET: /ChatAdmin/Create
        public async Task<IActionResult> Create()
        {
            var users = await _db.Users
                .OrderBy(u => u.UserName)
                .Select(u => new UserSelectVm
                {
                    Id = u.Id,
                    UserName = u.UserName!
                })
                .ToListAsync();

            return View(new CreateChatRoomVm
            {
                Users = users
            });
        }

        // POST: /ChatAdmin/Create
        [HttpPost]
        public async Task<IActionResult> Create(CreateChatRoomVm vm)
        {
            if (!vm.IsGroup)
            {
                ModelState.AddModelError("", "Admin can only create group chats here.");
            }

            if (!ModelState.IsValid)
            {
                vm.Users = await _db.Users
                    .Select(u => new UserSelectVm
                    {
                        Id = u.Id,
                        UserName = u.UserName!
                    })
                    .ToListAsync();

                return View(vm);
            }

            var adminId = _userManager.GetUserId(User)!;

            var room = new ChatRoom
            {
                Name = vm.Name,
                IsGroup = true,
                CreatedByUserId = adminId
            };

            _db.ChatRooms.Add(room);
            await _db.SaveChangesAsync();

            // add participants
            // remove admin from selected users + remove duplicates
            var participantIds = vm.SelectedUserIds
                .Where(id => id != adminId)
                .Distinct()
                .ToList();

            // add non-admin users
            foreach (var userId in participantIds)
            {
                _db.ChatRoomParticipants.Add(new ChatRoomParticipant
                {
                    ChatRoomId = room.Id,
                    UserId = userId,
                    IsAdmin = false
                });
            }

            // add admin exactly once
            _db.ChatRoomParticipants.Add(new ChatRoomParticipant
            {
                ChatRoomId = room.Id,
                UserId = adminId,
                IsAdmin = true
            });


            await _db.SaveChangesAsync();

            return RedirectToAction("Room", "Chat", new { id = room.Id });
        }
    }
}
