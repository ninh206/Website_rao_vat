using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using website_rao_vat.Data;
using website_rao_vat.Models;

public class ProductController : Controller
{
    private readonly DataBaseWebRaoVatContext _context;

    public ProductController(DataBaseWebRaoVatContext context)
    {
        _context = context;
    }

    // --- 1. ACTION THỐNG KÊ (GIỮ NGUYÊN TÊN THEO Ý NINH) ---
    [HttpGet]
    public IActionResult ThongKe(int? productId)
    {
        // SỬA TẠI ĐÂY: Dùng Session thay vì Authorize
        var userIdStr = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userIdStr)) return RedirectToAction("Login", "Account");

        int uId = int.Parse(userIdStr);

        // Lấy thông tin Profile để hiện Sidebar
        var model = GetUserProfileModel(uId);
        ViewBag.Products = model.MyProducts;

        int selectedId = productId ?? (model.MyProducts.FirstOrDefault()?.ProductId ?? 0);
        ViewBag.SelectedProductId = selectedId;

        if (selectedId > 0)
        {
            ViewBag.TotalViews = _context.ProductViews.Count(v => v.ProductId == selectedId);
            ViewBag.TotalLikes = _context.Favorites.Count(f => f.ProductId == selectedId);

            // Logic lấy 7 ngày gần nhất (như đã bàn ở bước trước)
            var last7Days = Enumerable.Range(0, 7).Select(i => DateTime.Today.AddDays(-i)).OrderBy(d => d).ToList();
            var dailyViews = _context.ProductViews
                .Where(v => v.ProductId == selectedId && v.ViewedAt >= DateTime.Today.AddDays(-6))
                .GroupBy(v => v.ViewedAt.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() }).ToList();

            ViewBag.ChartData = last7Days.Select(d => dailyViews.FirstOrDefault(v => v.Date == d.Date)?.Count ?? 0).ToList();
            ViewBag.ChartLabels = last7Days.Select(d => d.ToString("dd/MM")).ToList();
        }

        return View(model);
    }

    // --- 2. HÀM PHỤ (SỬA LẠI ĐỂ DÙNG SESSION ID) ---
    private UserProfileViewModel GetUserProfileModel(int uId)
    {
        return new UserProfileViewModel
        {
            User = _context.Users.FirstOrDefault(u => u.UserId == uId),
            MyProducts = _context.Products.Include(p => p.ProductImages).Where(p => p.UserId == uId).ToList() ?? new List<Product>(),
            SavedAds = _context.Favorites.Include(f => f.Product).ThenInclude(p => p.ProductImages).Where(f => f.UserId == uId).Select(f => f.Product).ToList() ?? new List<Product>()
        };
    }
}