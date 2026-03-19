using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace website_rao_vat.Models
{
    [Table("Categories")] // Đảm bảo map đúng tên bảng trong DB
    public partial class Category
    {
        [Key]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Tên danh mục không được để trống")]
        [StringLength(100)]
        public string CategoryName { get; set; } = string.Empty; // Khởi tạo chuỗi rỗng để tránh lỗi null

        [StringLength(50)]
        public string? IconClass { get; set; }

        [StringLength(255)]
        public string? Description { get; set; }

        // Navigation property: Nên để virtual để EF Core có thể Lazy Loading nếu cần
        [InverseProperty("Category")]
        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    }
}