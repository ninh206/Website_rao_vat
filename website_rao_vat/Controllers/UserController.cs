using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using website_rao_vat.Data;
using website_rao_vat.Models;

namespace website_rao_vat.Controllers
{
    public class UserController : Controller
    {
        private readonly DataBaseWebRaoVatContext _context;

        public UserController(DataBaseWebRaoVatContext context)
        {
            _context = context;
        }

        // 1. Trang quản lý chính
        public async Task<IActionResult> Index()
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr)) return RedirectToAction("Login", "Account");

            int userId = int.Parse(userIdStr);

            var viewModel = new UserProfileViewModel
            {
                User = await _context.Users.FindAsync(userId),

                // Tin của tôi
                MyProducts = await _context.Products
                    .Where(p => p.UserId == userId)
                    .OrderByDescending(p => p.CreatedAt)
                    .ToListAsync(),

                // TIN ĐÃ LƯU: Lấy từ bảng Favorites, nạp kèm ảnh sản phẩm
                SavedAds = await _context.Favorites
                    .Where(f => f.UserId == userId)
                    .Include(f => f.Product)
                        .ThenInclude(p => p.ProductImages)
                    .Select(f => f.Product)
                    .ToListAsync()
            };

            return View(viewModel);
        }

        // 2. Xóa bài viết
        [HttpPost]
        public async Task<IActionResult> DeletePost(int id)
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            var product = await _context.Products.FindAsync(id);

            // Bảo mật: Chỉ cho xóa nếu đúng là chủ bài đăng
            if (product != null && product.UserId.ToString() == userIdStr)
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
                TempData["Message"] = "Đã xóa tin đăng thành công!";
            }

            return RedirectToAction("Index");
        }

        // 3. Đổi mật khẩu (Logic cơ bản)
        [HttpPost]
        public async Task<IActionResult> ChangePassword(string oldPass, string newPass)
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            var user = await _context.Users.FindAsync(int.Parse(userIdStr));

            if (user.PasswordHash == oldPass) // Trong thực tế cần verify Hash
            {
                user.PasswordHash = newPass;
                await _context.SaveChangesAsync();
                TempData["Message"] = "Đổi mật khẩu thành công!";
            }
            else
            {
                TempData["Error"] = "Mật khẩu cũ không chính xác!";
            }

            return RedirectToAction("Index");
        }
    }
}