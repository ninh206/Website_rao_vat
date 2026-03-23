using Microsoft.AspNetCore.SignalR;
using website_rao_vat.Models;
using website_rao_vat.Data;
using Microsoft.EntityFrameworkCore;

namespace website_rao_vat.Hubs
{
    public class ChatHub : Hub
    {
        private readonly DataBaseWebRaoVatContext _context;

        public ChatHub(DataBaseWebRaoVatContext context)
        {
            _context = context;
        }

        // Hàm được gọi từ JavaScript để gửi tin nhắn
        public async Task SendMessage(int receiverId, string message)
        {
            // Lấy ID người gửi từ Session
            var senderIdStr = Context.GetHttpContext().Session.GetString("UserId");
            if (string.IsNullOrEmpty(senderIdStr)) return; // Bảo vệ: Không đăng nhập thì không cho gửi

            int senderId = int.Parse(senderIdStr);

            // 1. Lưu tin nhắn vào Database
            var newMessage = new Message
            {
                SenderId = senderId,
                ReceiverId = receiverId,
                Content = message,
                SentAt = DateTime.Now
            };

            _context.Messages.Add(newMessage);
            await _context.SaveChangesAsync();

            // 2. Bắn tin nhắn real-time
            // Tạm thời bắn cho tất cả mọi người (Ninh sẽ học cách gửi riêng sau)
            await Clients.All.SendAsync("ReceiveMessage", senderId, message, DateTime.Now.ToString("HH:mm"));
        }
    }
}