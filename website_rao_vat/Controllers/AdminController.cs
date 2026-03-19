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
    public async Task<IActionResult> Index(string userSearch, string postSearch, int page = 1)
    {
        if (HttpContext.Session.GetString("UserRole") != "Admin") return RedirectToAction("Login", "Account");

        int pageSize = 10;

        // Query bài đăng
        var postsQuery = _context.Products.Include(p => p.User).AsQueryable();
        if (!string.IsNullOrEmpty(postSearch))
            postsQuery = postsQuery.Where(p => p.Title.Contains(postSearch));

        // Tính toán phân trang
        int totalPosts = await postsQuery.CountAsync();
        int totalPages = (int)Math.Ceiling((double)totalPosts / pageSize);
        if (page < 1) page = 1;

        // Gán dữ liệu
        ViewBag.TotalPages = totalPages;
        ViewBag.CurrentPage = page;
        ViewBag.PostSearch = postSearch;
        ViewBag.UserSearch = userSearch;
        ViewBag.TotalUsers = await _context.Users.CountAsync();
        ViewBag.TotalPosts = totalPosts;
        ViewBag.AllPosts = await postsQuery.OrderByDescending(p => p.CreatedAt)
                                           .Skip((page - 1) * pageSize)
                                           .Take(pageSize).ToListAsync();

        // Query người dùng (Hiện tại vẫn lấy hết)
        var usersQuery = _context.Users.AsQueryable();
        if (!string.IsNullOrEmpty(userSearch))
            usersQuery = usersQuery.Where(u => u.FullName.Contains(userSearch) || u.Username.Contains(userSearch));

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