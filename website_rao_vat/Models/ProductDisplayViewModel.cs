namespace website_rao_vat.Models
{
    public class ProductDisplayViewModel
    {
        public int ProductId { get; set; }
        public string Title { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string ImageUrl { get; set; } = "/images/no-image.png";
        public string Location { get; set; } = "Toàn quốc";
        public string TimeAgo { get; set; } = string.Empty;

        // Logic để View chỉ việc hiển thị, không phải tính toán
        public bool IsFavorite { get; set; }
        public bool IsNew { get; set; }
    }
}