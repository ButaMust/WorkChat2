using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Metadata;
using System.Text.RegularExpressions;
using WorkChat2.Data;
using WorkChat2.Models;

namespace WorkChat2.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly AppDbContext _db;
        private readonly UserManager<AppUser> _userManager;

        public ChatHub(AppDbContext db, UserManager<AppUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public async Task JoinRoom(int roomId)
        {
            var userId = _userManager.GetUserId(Context.User!)!;

            var isMember = await _db.ChatRoomParticipants.AnyAsync(p => p.ChatRoomId == roomId && p.UserId == userId);

            if (!isMember)
                throw new HubException("Not a member of this room.");

            await Groups.AddToGroupAsync(Context.ConnectionId, $"room:{roomId}");
        }

        public async Task SendMessage(int roomId, string text)
        {
            text = (text ?? "").Trim();
            if (text.Length == 0) return;
            if (text.Length > 2000) throw new HubException("Message too long.");

            var userId = _userManager.GetUserId(Context.User!)!;

            var isMember = await _db.ChatRoomParticipants.AnyAsync(p => p.ChatRoomId == roomId && p.UserId == userId);

            if (!isMember)
                throw new HubException("Not a member of this room.");

            var msg = new Message
            {
                ChatRoomId = roomId,
                SenderId = userId,
                Text = text,
                IsEdited = false,
                IsDeleted = false
            };

            _db.Messages.Add(msg);

            // bump room UpdatedAt so ordering works
            var room = await _db.ChatRooms.FirstAsync(r => r.Id == roomId);
            room.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            var sender = await _userManager.GetUserAsync(Context.User!);

            await Clients.Group($"room:{roomId}")
                .SendAsync("MessageReceived", new
                {
                    id = msg.Id,
                    roomId,
                    text = msg.Text,
                    senderName = sender?.UserName ?? "Unknown",
                    senderId = userId,
                    createdAt = msg.CreatedAt,
                });
        }

        public async Task<List<object>> GetRecentMessages(int roomId, int take = 50)
        {
            if (take < 1) take = 1;
            if (take > 200) take = 200;

            var userId = _userManager.GetUserId(Context.User!)!;

            var isMember = await _db.ChatRoomParticipants.AnyAsync(p => p.ChatRoomId == roomId && p.UserId == userId);

            if (!isMember)
                throw new HubException("Not a member of this room.");

            var msgs = await _db.Messages
                .AsNoTracking()
                .Where(m => m.ChatRoomId == roomId && !m.IsDeleted)
                .OrderByDescending(m => m.CreatedAt)
                .Take(take)
                .Select(m => new
                {
                    id = m.Id,
                    roomId = m.ChatRoomId,
                    text = m.Text,
                    senderId = m.SenderId,
                    senderName = m.Sender.UserName,
                    CreatedAt = m.CreatedAt,
                })
                .ToListAsync();

            // reverse so UI shows oldest -> newest
            msgs.Reverse();
            return msgs.Cast<object>().ToList();
        }
    }
}
