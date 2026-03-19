using Microsoft.AspNetCore.Mvc.Rendering;

namespace website_rao_vat.Models
{
    public class AdPostViewModel
    {
        // Các trường dữ liệu để Sửa/Thêm
        public int ProductId { get; set; } // Dùng khi Sửa
        public string Title { get; set; }
        public decimal Price { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public int CategoryId { get; set; }

        // Thay thế hoàn toàn cho ViewBag.Categories
        public SelectList? CategoryList { get; set; }

        // Hứng file ảnh
        public List<IFormFile>? Images { get; set; }
    }
}