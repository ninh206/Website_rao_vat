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
            var userIdStr = HttpContext.Session.GetString("UserId");

            var product = await _context.Products
                .Include(p => p.ProductImages)
                .Include(p => p.User)
                .Include(p => p.Category)
                .Include(p => p.Favorites)
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (product == null) return NotFound();

            // Nếu ông muốn sạch tuyệt đối, hãy tạo thêm ProductDetailViewModel. 
            // Ở đây tôi nạp thêm thông tin thả tim vào ViewBag để View chỉ việc hiện
            ViewBag.IsFavorite = !string.IsNullOrEmpty(userIdStr) &&
                                 product.Favorites.Any(f => f.UserId.ToString() == userIdStr);

            return View(product);
        }

        public IActionResult Privacy() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}