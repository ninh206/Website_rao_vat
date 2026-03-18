using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using website_rao_vat.Data;
using website_rao_vat.Models;

public class AdsController : Controller
{
    private readonly DataBaseWebRaoVatContext _context;
    private readonly IWebHostEnvironment _hostEnvironment;

    public AdsController(DataBaseWebRaoVatContext context, IWebHostEnvironment hostEnvironment)
    {
        _context = context;
        _hostEnvironment = hostEnvironment;
    }

    // ================== PHẦN ĐĂNG TIN (POST) ==================
    [HttpGet]
    public IActionResult Post()
    {
        if (HttpContext.Session.GetString("UserId") == null) return RedirectToAction("Login", "Account");
        ViewBag.Categories = new SelectList(_context.Categories, "CategoryId", "CategoryName");
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Post(AdPostViewModel model)
    {
        var userId = HttpContext.Session.GetString("UserId");
        if (userId == null) return RedirectToAction("Login", "Account");

        var product = new Product
        {
            Title = model.Title,
            Price = model.Price,
            Description = model.Description,
            Location = model.Location,
            CategoryId = model.CategoryId,
            UserId = int.Parse(userId),
            CreatedAt = DateTime.Now,
            Status = "Active"
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        if (model.Images != null && model.Images.Count > 0)
        {
            string uploadDir = Path.Combine(_hostEnvironment.WebRootPath, "uploads");
            if (!Directory.Exists(uploadDir)) Directory.CreateDirectory(uploadDir);

            foreach (var file in model.Images)
            {
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                string filePath = Path.Combine(uploadDir, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create)) { await file.CopyToAsync(stream); }

                _context.ProductImages.Add(new ProductImage
                {
                    ProductId = product.ProductId,
                    ImageUrl = "/uploads/" + fileName,
                    IsPrimary = (model.Images.IndexOf(file) == 0)
                });
            }
            await _context.SaveChangesAsync();
        }
        return RedirectToAction("Index", "User");
    }

    // ================== PHẦN SỬA TIN (EDIT) - FIX TẠI ĐÂY ==================

    // 1. GET: Lấy dữ liệu cũ hiện lên Form
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var userId = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login", "Account");

        var product = await _context.Products.FindAsync(id);

        // Bảo mật: Chỉ chủ bài đăng mới được vào trang sửa
        if (product == null || product.UserId.ToString() != userId) return Unauthorized();

        ViewBag.Categories = new SelectList(_context.Categories, "CategoryId", "CategoryName", product.CategoryId);
        return View(product);
    }

    // 2. POST: Lưu dữ liệu mới sau khi sửa
    // 2. POST: Lưu dữ liệu mới sau khi sửa
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Product model)
    {
        // 1. Kiểm tra ID khớp nhau
        if (id != model.ProductId) return NotFound();

        // 2. Lấy UserId từ Session để bảo mật
        var userIdStr = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userIdStr)) return RedirectToAction("Login", "Account");

        try
        {
            // 3. Tìm sản phẩm thực tế đang nằm trong Database
            var productInDb = await _context.Products.FirstOrDefaultAsync(p => p.ProductId == id);

            // 4. Kiểm tra quyền: Chỉ chủ bài đăng mới được sửa
            if (productInDb == null || productInDb.UserId.ToString() != userIdStr)
            {
                return Unauthorized();
            }

            // 5. CHỈ CẬP NHẬT các trường lấy từ Form
            productInDb.Title = model.Title;
            productInDb.Price = model.Price;
            productInDb.Location = model.Location;
            productInDb.CategoryId = model.CategoryId;
            productInDb.Description = model.Description;
            productInDb.UpdatedAt = DateTime.Now; // Ghi nhận thời gian sửa

            // 6. Lưu thay đổi
            await _context.SaveChangesAsync();

            TempData["Message"] = "Cập nhật tin đăng thành công!";
            return RedirectToAction("Index", "User");
        }
        catch (Exception ex)
        {
            // Nếu có lỗi (ví dụ lỗi kết nối DB), nạp lại danh mục và hiện lại Form
            ModelState.AddModelError("", "Không thể lưu thay đổi. Lỗi: " + ex.Message);
            ViewBag.Categories = new SelectList(_context.Categories, "CategoryId", "CategoryName", model.CategoryId);
            return View(model);
        }
    }
    // ================== TÍNH NĂNG LƯU TIN (Dùng Favorite.cs) ==================
    [HttpPost]
    public async Task<IActionResult> ToggleFavorite(int productId)
    {
        var userIdStr = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userIdStr))
            return Json(new { success = false, message = "Vui lòng đăng nhập!" });

        int userId = int.Parse(userIdStr);

        try
        {
            // 1. Dùng SingleOrDefaultAsync để đảm bảo lấy đúng và duy nhất
            var existingFavorite = await _context.Favorites
                .FirstOrDefaultAsync(f => f.UserId == userId && f.ProductId == productId);

            if (existingFavorite != null)
            {
                _context.Favorites.Remove(existingFavorite);
                await _context.SaveChangesAsync();
                return Json(new { success = true, isFavorite = false });
            }
            else
            {
                var fav = new Favorite
                {
                    UserId = userId,
                    ProductId = productId,
                    CreatedAt = DateTime.Now
                };
                _context.Favorites.Add(fav);
                await _context.SaveChangesAsync();
                return Json(new { success = true, isFavorite = true });
            }
        }
        catch (DbUpdateException)
        {
            // 2. Nếu lỡ có 2 request chạy cùng lúc gây lỗi Unique Key, 
            // ta coi như nó đã được lưu rồi và không báo lỗi đỏ cho User nữa.
            return Json(new { success = true, isFavorite = true });
        }
    }
}
