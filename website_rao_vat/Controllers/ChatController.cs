using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using website_rao_vat.Data;
using website_rao_vat.Models;

namespace website_rao_vat.Controllers
{
    public class ChatController : Controller
    {
        private readonly DataBaseWebRaoVatContext _context;
        public ChatController(DataBaseWebRaoVatContext context) => _context = context;

        // Trang Inbox chính
        public async Task<IActionResult> Index(int? withUserId)
        {
            var myIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(myIdStr)) return RedirectToAction("Login", "Account");
            int myId = int.Parse(myIdStr);

            // 1. Lấy tất cả tin nhắn của mình
            var allMessages = await _context.Messages
                .Where(m => m.SenderId == myId || m.ReceiverId == myId)
                .OrderByDescending(m => m.SentAt)
                .ToListAsync();

            // 2. Lấy danh sách ID những người mình đã chat
            var otherUserIds = allMessages
                .Select(m => m.SenderId == myId ? m.ReceiverId : m.SenderId)
                .Distinct().ToList();

            var otherUsers = await _context.Users
                .Where(u => otherUserIds.Contains(u.UserId))
                .ToDictionaryAsync(u => u.UserId);

            // 3. Đóng gói vào ViewModel (Sửa lỗi N+1 Query)
            var convos = allMessages
                .GroupBy(m => m.SenderId == myId ? m.ReceiverId : m.SenderId)
                .Select(g => new ConversationItem
                {
                    OtherUser = otherUsers.GetValueOrDefault(g.Key),
                    LastMessage = g.First().Content,
                    LastTime = g.First().SentAt
                }).Where(x => x.OtherUser != null).ToList();

            var model = new ChatViewModel { Conversations = convos };

            // 4. Nếu đang mở 1 cuộc chat cụ thể
            if (withUserId.HasValue)
            {
                model.SelectedUser = await _context.Users.FindAsync(withUserId);
                model.Messages = allMessages
                    .Where(m => (m.SenderId == myId && m.ReceiverId == withUserId) ||
                                (m.SenderId == withUserId && m.ReceiverId == myId))
                    .OrderBy(m => m.SentAt).ToList();
            }

            return View(model); // CHÚ Ý: Gửi model kiểu ChatViewModel
        }

        [HttpGet]
        public async Task<IActionResult> GetChatHistory(int receiverId)
        {
            var myIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(myIdStr)) return Json(new { error = "Unauthorized" });
            int myId = int.Parse(myIdStr);

            var messages = await _context.Messages
                .Where(m => (m.SenderId == myId && m.ReceiverId == receiverId) ||
                            (m.SenderId == receiverId && m.ReceiverId == myId))
                .OrderBy(m => m.SentAt)
                .Select(m => new { senderId = m.SenderId, content = m.Content, time = m.SentAt.ToString("HH:mm") })
                .ToListAsync();
            return Json(messages);
        }
    }
}