using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using website_rao_vat.Data;
using website_rao_vat.Models;

public class AdminController : Controller
{
    private readonly DataBaseWebRaoVatContext _context;

    public AdminController(DataBaseWebRaoVatContext context)
    {
        _context = context;
    }

    // 1. Trang Dashboard: Thống kê và liệt kê danh sách
    public async Task<IActionResult> Index(string userSearch, string postSearch)
    {
        if (HttpContext.Session.GetString("UserRole") != "Admin") return RedirectToAction("Login", "Account");

        // 1. Khởi tạo query cho Users
        var usersQuery = _context.Users.AsQueryable();

        // 2. ÁP DỤNG ĐIỀU KIỆN TÌM KIẾM (Đây là chỗ quan trọng nhất)
        if (!string.IsNullOrEmpty(userSearch))
        {
            // Tìm gần đúng: chỉ cần chứa ký tự đó là hiện ra
            usersQuery = usersQuery.Where(u => u.Username.Contains(userSearch)
                                            || u.FullName.Contains(userSearch));
        }

        // --- Logic cho bài đăng giữ nguyên ---
        var postsQuery = _context.Products.Include(p => p.User).AsQueryable();
        if (!string.IsNullOrEmpty(postSearch))
        {
            postsQuery = postsQuery.Where(p => p.Title.Contains(postSearch));
        }

        ViewBag.TotalUsers = await _context.Users.CountAsync();
        ViewBag.TotalPosts = await _context.Products.CountAsync();
        ViewBag.AllPosts = await postsQuery.OrderByDescending(p => p.CreatedAt).ToListAsync();

        // Trả về danh sách đã được lọc
        return View(await usersQuery.ToListAsync());
    }

    // 2. Xóa bài viết (Bất kỳ ai)
    [HttpPost]
    public async Task<IActionResult> DeleteAnyPost(int id)
    {
        if (HttpContext.Session.GetString("UserRole") != "Admin") return Unauthorized();

        var product = await _context.Products.FindAsync(id);
        if (product != null)
        {
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            TempData["Message"] = "Đã xóa bài viết thành công!";
        }
        return RedirectToAction("Index");
    }

    // 3. GET: Trang chỉnh sửa người dùng
    [HttpGet]
    public async Task<IActionResult> EditUser(int id)
    {
        if (HttpContext.Session.GetString("UserRole") != "Admin") return Unauthorized();

        var user = await _context.Users.FindAsync(id);
        if (user == null) return NotFound();
        return View(user);
    }

    // 4. POST: Lưu thông tin người dùng sau khi sửa
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditUser(int UserId, User model, string newPassword)
    {
        if (HttpContext.Session.GetString("UserRole") != "Admin") return Unauthorized();

        // Tìm User gốc từ Database
        var userInDb = await _context.Users.FirstOrDefaultAsync(u => u.UserId == UserId);

        if (userInDb == null)
        {
            TempData["Error"] = "Không tìm thấy người dùng!";
            return RedirectToAction("Index");
        }

        // Cập nhật các thông tin cơ bản
        userInDb.Username = model.Username;
        userInDb.FullName = model.FullName;
        userInDb.Email = model.Email;

        // KIỂM TRA MẬT KHẨU MỚI: Chỉ cập nhật nếu Admin có nhập vào ô đó
        if (!string.IsNullOrEmpty(newPassword))
        {
            // Gán trực tiếp (hoặc dùng hàm Hash nếu ông đã cài đặt bảo mật)
            userInDb.PasswordHash = newPassword;
        }

        await _context.SaveChangesAsync();
        TempData["Message"] = "Đã cập nhật thông tin và mật khẩu thành công!";

        return RedirectToAction("Index");
    }
}