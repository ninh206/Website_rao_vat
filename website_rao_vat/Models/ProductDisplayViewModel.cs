using System;
using System.Collections.Generic; // Phải có cái này để dùng được List<>

namespace website_rao_vat.Models
{
    // 1. Cái khuôn cho TỪNG sản phẩm
    public class ProductDisplayViewModel
    {
        public int ProductId { get; set; }
        public string Title { get; set; }
        public decimal Price { get; set; }
        public string Location { get; set; }
        public string ImageUrl { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string SellerName { get; set; }
        public string Description { get; set; }

        public bool IsFavorite { get; set; } = false; // Trạng thái thả tim
        public bool IsNew { get; set; }      // Đánh dấu bài mới
        public string TimeAgo { get; set; }  // Chuỗi thời gian đẹp
    }

    // 2. Cái khuôn cho DANH SÁCH sản phẩm và PHÂN TRANG
    // Tách riêng ra thế này để ở Controller gọi cho dễ ông nhé
    public class ProductListViewModel
    {
        public List<ProductDisplayViewModel> Products { get; set; } = new();

        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; } = 5;
    }
}