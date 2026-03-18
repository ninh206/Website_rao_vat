using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using website_rao_vat.Data;
using website_rao_vat.Models;

public class AccountController : Controller
{
    private readonly DataBaseWebRaoVatContext _context;

    public AccountController(DataBaseWebRaoVatContext context)
    {
        _context = context;
    }

    [HttpGet]
    public IActionResult Login()
    {
        if (HttpContext.Session.GetString("UserName") != null)
        {
            return HttpContext.Session.GetString("UserRole") == "Admin"
                ? RedirectToAction("Index", "Admin")
                : RedirectToAction("Index", "Home");
        }
        return View();
    }

    [HttpGet]
    public IActionResult Register() => View();

    // 1. XỬ LÝ ĐĂNG NHẬP (Cập nhật trạng thái ONLINE)
    [HttpPost]
    public async Task<IActionResult> Login(string username, string password)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => (u.Username == username || u.Email == username) && u.PasswordHash == password);

        if (user != null)
        {
            // --- CẬP NHẬT TRẠNG THÁI ONLINE ---
            user.IsActive = true;
            await _context.SaveChangesAsync();

            // Lưu Session
            HttpContext.Session.SetString("UserId", user.UserId.ToString());
            HttpContext.Session.SetString("UserName", user.FullName ?? user.Username);
            HttpContext.Session.SetString("UserRole", user.Username == "admin" ? "Admin" : "User");

            if (user.Username == "admin")
                return RedirectToAction("Index", "Admin");

            return RedirectToAction("Index", "Home");
        }

        ViewBag.Error = "Tên đăng nhập hoặc mật khẩu không đúng!";
        return View();
    }

    // 2. XỬ LÝ ĐĂNG XUẤT (Cập nhật trạng thái OFFLINE)
    public async Task<IActionResult> Logout()
    {
        var userIdStr = HttpContext.Session.GetString("UserId");

        if (!string.IsNullOrEmpty(userIdStr))
        {
            int userId = int.Parse(userIdStr);
            var user = await _context.Users.FindAsync(userId);

            if (user != null)
            {
                // --- CẬP NHẬT TRẠNG THÁI OFFLINE ---
                user.IsActive = false;
                await _context.SaveChangesAsync();
            }
        }

        // Xóa toàn bộ Session
        HttpContext.Session.Clear();
        return RedirectToAction("Login");
    }

    // 3. Xử lý Đăng ký (Mặc định khi đăng ký xong vẫn là false cho tới khi login)
    [HttpPost]
    public async Task<IActionResult> Register(string fullName, string email, string password)
    {
        var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (existingUser != null)
        {
            ViewBag.Error = "Email này đã tồn tại!";
            return View();
        }

        var user = new User
        {
            FullName = fullName,
            Email = email,
            Username = email,
            PasswordHash = password,
            CreatedAt = DateTime.Now,
            IsActive = false // Mới đăng ký thì chưa online
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return RedirectToAction("Login");
    }
}