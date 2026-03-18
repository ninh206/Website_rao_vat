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

        public async Task<IActionResult> Index()
        {
            // 1. Chặn Admin (Cái này ông làm đúng rồi)
            if (HttpContext.Session.GetString("UserRole") == "Admin")
            {
                return RedirectToAction("Index", "Admin");
            }

            // 2. LẤY DỮ LIỆU: Phải có đoạn này để lấy tin đăng ra
            var products = await _context.Products
                .Include(p => p.ProductImages) // Lấy kèm ảnh
                .Include(p => p.User)           // Lấy kèm thông tin người đăng
                .OrderByDescending(p => p.CreatedAt) // Tin mới nhất lên đầu
                .Take(8) // Lấy 8 tin thôi cho đẹp
                .ToListAsync();

            // 3. TRUYỀN DỮ LIỆU: Phải bỏ biến 'products' vào đây
            return View(products);
        }
        public async Task<IActionResult> Details(int id)
        {
            var product = await _context.Products
                .Include(p => p.ProductImages)
                .Include(p => p.User)
                .Include(p => p.Category)
                .Include(p => p.Favorites) // Thêm để trang chi tiết cũng hiện nút tim chuẩn
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (product == null)
            {
                return NotFound();
            }

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