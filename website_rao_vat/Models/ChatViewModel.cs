using website_rao_vat.Models;

namespace website_rao_vat.Models
{
    public class ChatViewModel
    {
        // Danh sách các cuộc hội thoại bên trái
        public List<ConversationItem> Conversations { get; set; } = new();

        // Người đang chat cùng (nếu có)
        public User? SelectedUser { get; set; }

        // Danh sách tin nhắn chi tiết bên phải
        public List<Message> Messages { get; set; } = new();
    }

    public class ConversationItem
    {
        public User OtherUser { get; set; }
        public string LastMessage { get; set; }
        public DateTime LastTime { get; set; }
    }
}