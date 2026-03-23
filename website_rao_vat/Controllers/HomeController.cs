using System.Diagnostics;
using System.Security.Claims;
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

        // --- HÀM HỖ TRỢ LẤY USER ID ---
        private string GetCurrentUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier)
                   ?? HttpContext.Session.GetString("UserId");
        }

        // ==========================================
        // 1. TRANG CHỦ (INDEX) - FIX LỌC GIÁ & TIM
        // ==========================================
        public async Task<IActionResult> Index(string sortBy, string location, string query, int page = 1)
        {
            int pageSize = 5;
            string userIdStr = GetCurrentUserId();
            var favoriteProductIds = new List<int>();

            // Lấy danh sách ID đã thích để đổ màu tim
            if (!string.IsNullOrEmpty(userIdStr) && int.TryParse(userIdStr, out int userId))
            {
                favoriteProductIds = await _context.Favorites
                    .Where(f => f.UserId == userId)
                    .Select(f => f.ProductId ?? 0)
                    .ToListAsync();
            }

            // Bắt đầu Query
            var productsQuery = _context.Products
                .Include(p => p.User)
                .Include(p => p.ProductImages)
                .AsQueryable();

            // A. LOGIC LỌC (FILTER)
            if (!string.IsNullOrEmpty(query))
                productsQuery = productsQuery.Where(p => p.Title.Contains(query));

            if (!string.IsNullOrEmpty(location))
                productsQuery = productsQuery.Where(p => p.Location == location);

            // B. LOGIC SẮP XẾP (SORTING) - FIX LỖI Ở ĐÂY
            productsQuery = sortBy switch
            {
                "price_asc" => productsQuery.OrderBy(p => p.Price),
                "price_desc" => productsQuery.OrderByDescending(p => p.Price),
                "oldest" => productsQuery.OrderBy(p => p.CreatedAt),
                _ => productsQuery.OrderByDescending(p => p.CreatedAt) // Mặc định: Mới nhất
            };

            // C. TÍNH TOÁN PHÂN TRANG
            int totalItems = await productsQuery.CountAsync();
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            if (page < 1) page = 1;

            var pagedData = await productsQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new ProductDisplayViewModel
                {
                    ProductId = p.ProductId,
                    Title = p.Title,
                    Price = p.Price,
                    Location = p.Location,
                    ImageUrl = p.ProductImages.FirstOrDefault().ImageUrl ?? "/images/no-image.png",
                    Description = p.Description,
                    TimeAgo = p.CreatedAt.HasValue ? p.CreatedAt.Value.ToString("dd/MM") : "",
                    IsFavorite = favoriteProductIds.Contains(p.ProductId)
                }).ToListAsync();

            return View(new ProductListViewModel
            {
                Products = pagedData,
                CurrentPage = page,
                TotalPages = totalPages
            });
        }

        // ==========================================
        // 2. TRANG CHI TIẾT (DETAILS)
        // ==========================================
        public async Task<IActionResult> Details(int id)
        {
            var product = await _context.Products
                .Include(p => p.ProductImages).Include(p => p.User)
                .Include(p => p.Category).Include(p => p.Favorites)
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (product == null) return NotFound();

            string userIdStr = GetCurrentUserId();
            string cookieName = $"v_prod_{id}";

            // Logic tăng view
            bool isOwner = !string.IsNullOrEmpty(userIdStr) && userIdStr == product.UserId.ToString();
            if (!isOwner && Request.Cookies[cookieName] == null)
            {
                _context.ProductViews.Add(new ProductView
                {
                    ProductId = id,
                    ViewerId = userIdStr,
                    IPAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                    ViewedAt = DateTime.Now
                });
                product.ViewCount = (product.ViewCount ?? 0) + 1;
                await _context.SaveChangesAsync();
                Response.Cookies.Append(cookieName, "true", new CookieOptions { Expires = DateTimeOffset.Now.AddMinutes(30) });
            }

            ViewBag.IsFavorite = !string.IsNullOrEmpty(userIdStr) && product.Favorites.Any(f => f.UserId.ToString() == userIdStr);
            return View(product);
        }

        // ==========================================
        // 3. TÌM KIẾM (SEARCH)
        // ==========================================
        public async Task<IActionResult> Search(string query)
        {
            if (string.IsNullOrEmpty(query)) return RedirectToAction("Index");

            string userIdStr = GetCurrentUserId();
            var favoriteProductIds = new List<int>();

            if (!string.IsNullOrEmpty(userIdStr) && int.TryParse(userIdStr, out int userId))
            {
                favoriteProductIds = await _context.Favorites
                    .Where(f => f.UserId == userId)
                    .Select(f => f.ProductId ?? 0)
                    .ToListAsync();
            }

            var products = await _context.Products
                .Include(p => p.ProductImages)
                .Where(p => p.Title.Contains(query))
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            var viewModel = products.Select(p => new ProductDisplayViewModel
            {
                ProductId = p.ProductId,
                Title = p.Title,
                Price = p.Price,
                ImageUrl = p.ProductImages?.FirstOrDefault()?.ImageUrl ?? "/images/no-image.png",
                Location = p.Location ?? "Toàn quốc",
                IsFavorite = favoriteProductIds.Contains(p.ProductId),
                TimeAgo = p.CreatedAt?.ToString("dd/MM") ?? ""
            }).ToList();

            ViewBag.Keyword = query;
            return View(viewModel);
        }
    }
}