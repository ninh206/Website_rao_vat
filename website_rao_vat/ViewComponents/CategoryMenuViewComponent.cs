using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using website_rao_vat.Data;

namespace website_rao_vat.ViewComponents
{
    // 1. "Cái khuôn" nhỏ nằm ngay trong file này luôn nè Ninh
    public class CategoryWithCount
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public int Count { get; set; }
    }

    public class CategoryMenuViewComponent : ViewComponent
    {
        private readonly DataBaseWebRaoVatContext _context;

        public CategoryMenuViewComponent(DataBaseWebRaoVatContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            // 2. Thay vì ToList đơn thuần, mình dùng Select để nhồi dữ liệu vào khuôn
            var model = await _context.Categories
                .Select(c => new CategoryWithCount
                {
                    CategoryId = c.CategoryId,
                    CategoryName = c.CategoryName,
                    // Đếm trực tiếp số tin đăng của từng danh mục
                    Count = _context.Products.Count(p => p.CategoryId == c.CategoryId)
                })
                .ToListAsync();

            // Trả về danh sách đã có số lượng
            return View(model);
        }
    }
}