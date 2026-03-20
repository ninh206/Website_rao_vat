using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using website_rao_vat.Data;
using website_rao_vat.Models;

namespace website_rao_vat.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly DataBaseWebRaoVatContext _context;

        public HomeController(ILogger<HomeController> logger, DataBaseWebRaoVatContext context)
        {
            _logger = logger;
            _context = context;
        }

        // ==========================================
        // 1. TRANG CHỦ (INDEX)
        // ==========================================
        public async Task<IActionResult> Index()
        {
            // Bảo mật: Nếu là Admin thì đẩy vào trang quản trị luôn
            if (HttpContext.Session.GetString("UserRole") == "Admin")
            {
                return RedirectToAction("Index", "Admin");
            }

            var userIdStr = HttpContext.Session.GetString("UserId");

            // Lấy 12 sản phẩm mới nhất
            var products = await _context.Products
                .Include(p => p.ProductImages)
                .Include(p => p.Favorites)
                .OrderByDescending(p => p.CreatedAt)
                .Take(12)
                .ToListAsync();

            // CHUYỂN ĐỔI SANG VIEWMODEL (Xử lý hết logic tại đây)
            var viewModel = products.Select(p => new ProductDisplayViewModel
            {
                ProductId = p.ProductId,
                Title = p.Title,
                Price = p.Price,
                ImageUrl = p.ProductImages?.FirstOrDefault()?.ImageUrl ?? "/images/no-image.png",
                Location = p.Location ?? "Toàn quốc",

                // Logic: Đã thả tim chưa? (Check theo UserId trong Session)
                IsFavorite = !string.IsNullOrEmpty(userIdStr) &&
                             p.Favorites.Any(f => f.UserId.ToString() == userIdStr),

                // Logic: Tin mới (Đăng trong vòng 3 ngày gần đây)
                IsNew = p.CreatedAt > DateTime.Now.AddDays(-3),

                // Định dạng thời gian hiển thị
                TimeAgo = p.CreatedAt?.ToString("dd/MM") ?? ""
            }).ToList();

            return View(viewModel);
        }

        // ==========================================
        // 2. TRANG CHI TIẾT (DETAILS)
        // ==========================================
        public async Task<IActionResult> Details(int id)
        {
            // 1. Lấy thông tin sản phẩm (Nạp đủ các bảng liên quan)
            var product = await _context.Products
                .Include(p => p.ProductImages).Include(p => p.User)
                .Include(p => p.Category).Include(p => p.Favorites)
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (product == null) return NotFound();

            // 2. Lấy UserId từ Session (Do AccountController của ông dùng Session)
            var currentUserId = HttpContext.Session.GetString("UserId");
            string cookieName = $"v_prod_{id}";

            // --- LOGIC GHI NHẬN LƯỢT XEM (PHẢI NẰM Ở ĐÂY) ---
            // Điều kiện: Nếu (Khách vãng lai HOẶC người xem khác chủ tin) VÀ chưa có Cookie trong 30p
            bool isOwner = !string.IsNullOrEmpty(currentUserId) && currentUserId == product.UserId.ToString();

            if (!isOwner && Request.Cookies[cookieName] == null)
            {
                // Ghi vào bảng ProductViews
                _context.ProductViews.Add(new ProductView
                {
                    ProductId = id,
                    ViewerId = currentUserId, // Có thể null nếu chưa đăng nhập
                    IPAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                    ViewedAt = DateTime.Now
                });

                // Tăng ViewCount trong bảng Products
                product.ViewCount = (product.ViewCount ?? 0) + 1;

                await _context.SaveChangesAsync();

                // Cắm Cookie để không bị đếm trùng khi F5
                Response.Cookies.Append(cookieName, "true", new CookieOptions { Expires = DateTimeOffset.Now.AddMinutes(30) });
            }

            ViewBag.IsFavorite = !string.IsNullOrEmpty(currentUserId) && product.Favorites.Any(f => f.UserId.ToString() == currentUserId);
            return View(product);
        }

        public IActionResult Privacy() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        // ==========================================
        // 3. HÀM TÌM KIẾM (SEARCH)
        // ==========================================
        public async Task<IActionResult> Search(string query)
        {
            // Nếu không nhập gì thì quay về trang chủ
            if (string.IsNullOrEmpty(query))
            {
                return RedirectToAction("Index");
            }

            var userIdStr = HttpContext.Session.GetString("UserId");

            // Lọc sản phẩm theo tiêu đề (Tương đương LIKE %query% trong SQL)
            var products = await _context.Products
                .Include(p => p.ProductImages)
                .Include(p => p.Favorites)
                .Where(p => p.Title.Contains(query)) // Logic tìm kiếm chính ở đây nè Ninh
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            // Chuyển sang ViewModel để hiển thị ra View
            var viewModel = products.Select(p => new ProductDisplayViewModel
            {
                ProductId = p.ProductId,
                Title = p.Title,
                Price = p.Price,
                ImageUrl = p.ProductImages?.FirstOrDefault()?.ImageUrl ?? "/images/no-image.png",
                Location = p.Location ?? "Toàn quốc",
                IsFavorite = !string.IsNullOrEmpty(userIdStr) && p.Favorites.Any(f => f.UserId.ToString() == userIdStr),
                IsNew = p.CreatedAt > DateTime.Now.AddDays(-3),
                TimeAgo = p.CreatedAt?.ToString("dd/MM") ?? ""
            }).ToList();

            ViewBag.Keyword = query; // Để hiện lại câu "Kết quả tìm kiếm cho: ..."
            return View(viewModel);
        }
    }
}