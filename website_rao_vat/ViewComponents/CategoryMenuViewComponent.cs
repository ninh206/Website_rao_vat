using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using website_rao_vat.Data;

namespace website_rao_vat.ViewComponents
{
    // Tên class BẮT BUỘC phải có chữ ViewComponent ở cuối
    public class CategoryMenuViewComponent : ViewComponent
    {
        private readonly DataBaseWebRaoVatContext _context;

        public CategoryMenuViewComponent(DataBaseWebRaoVatContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            // Lấy danh sách danh mục từ DB
            var categories = await _context.Categories.ToListAsync();

            // Nó sẽ tự động tìm đến file Default.cshtml trong folder CategoryMenu của ông
            return View(categories);
        }
    }
}