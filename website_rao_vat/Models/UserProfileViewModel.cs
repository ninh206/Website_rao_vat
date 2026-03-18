using System.Collections.Generic;

namespace website_rao_vat.Models
{
    public class UserProfileViewModel
    {
        // Thông tin người dùng và bài đăng
        public User User { get; set; }
        public List<Product> MyProducts { get; set; }
        public List<Product> SavedAds { get; set; } // Tin đã thả tim

        // Dùng cho tính năng đổi mật khẩu
        public string? OldPassword { get; set; }
        public string? NewPassword { get; set; }
        public string? ConfirmPassword { get; set; }
    }
}