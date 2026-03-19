using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using website_rao_vat.Data;
using website_rao_vat.Models;

namespace website_rao_vat.Controllers
{
    public class AdsController : Controller
    {
        private readonly DataBaseWebRaoVatContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;

        public AdsController(DataBaseWebRaoVatContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
        }

        // ==========================================
        // 1. CHỨC NĂNG ĐĂNG TIN (GET & POST)
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> Post()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login", "Account");

            var categories = await _context.Categories.ToListAsync();
            var model = new AdPostViewModel
            {
                CategoryList = new SelectList(categories, "CategoryId", "CategoryName")
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Post(AdPostViewModel model)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login", "Account");

            if (ModelState.IsValid)
            {
                var product = new Product
                {
                    Title = model.Title,
                    Price = model.Price,
                    Description = model.Description,
                    Location = model.Location,
                    CategoryId = model.CategoryId, // Đảm bảo ViewModel để int, không phải int?
                    UserId = int.Parse(userId),
                    CreatedAt = DateTime.Now,
                    Status = "Active"
                };

                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                if (model.Images != null && model.Images.Any())
                {
                    await SaveProductImages(product.ProductId, model.Images);
                }

                return RedirectToAction("Index", "User");
            }

            // Nếu lỗi, nạp lại danh mục cho Dropdown
            var categories = await _context.Categories.ToListAsync();
            model.CategoryList = new SelectList(categories, "CategoryId", "CategoryName");
            return View(model);
        }

        // ==========================================
        // 2. CHỨC NĂNG SỬA TIN (GET & POST)
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login", "Account");

            var product = await _context.Products.Include(p => p.Category).FirstOrDefaultAsync(p => p.ProductId == id);

            if (product == null || product.UserId.ToString() != userId)
                return Unauthorized();

            var categories = await _context.Categories.ToListAsync();

            var model = new AdPostViewModel
            {
                ProductId = product.ProductId,
                Title = product.Title,
                Price = product.Price,
                Description = product.Description,
                Location = product.Location,
                // Sửa dòng 101 thành:
                CategoryId = product.CategoryId ?? 0,
                CategoryList = new SelectList(categories, "CategoryId", "CategoryName", product.CategoryId)
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AdPostViewModel model)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login", "Account");

            var productInDb = await _context.Products.FirstOrDefaultAsync(p => p.ProductId == id);

            if (productInDb == null || productInDb.UserId.ToString() != userId)
                return Unauthorized();

            if (ModelState.IsValid)
            {
                productInDb.Title = model.Title;
                productInDb.Price = model.Price;
                productInDb.Description = model.Description;
                productInDb.Location = model.Location;
                // FIX LỖI CS0266 TẠI ĐÂY:
                productInDb.CategoryId = model.CategoryId;
                productInDb.UpdatedAt = DateTime.Now;

                if (model.Images != null && model.Images.Any())
                {
                    await SaveProductImages(productInDb.ProductId, model.Images);
                }

                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "User");
            }

            var categories = await _context.Categories.ToListAsync();
            model.CategoryList = new SelectList(categories, "CategoryId", "CategoryName", model.CategoryId);
            return View(model);
        }

        // ==========================================
        // 3. HÀM PHỤ TRỢ LƯU ẢNH (DÙNG CHUNG)
        // ==========================================

        private async Task SaveProductImages(int productId, List<IFormFile> images)
        {
            string path = Path.Combine(_hostEnvironment.WebRootPath, "images");
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);

            foreach (var file in images)
            {
                if (file.Length > 0)
                {
                    string fileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
                    string filePath = Path.Combine(path, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    _context.ProductImages.Add(new ProductImage
                    {
                        ProductId = productId,
                        ImageUrl = "/images/" + fileName
                    });
                }
            }
            await _context.SaveChangesAsync();
        }
    }
}