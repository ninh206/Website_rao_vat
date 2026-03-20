using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using website_rao_vat.Models;

public class ProductView
{
    [Key]
    public int ViewId { get; set; }

    public int ProductId { get; set; }

    public string? ViewerId { get; set; }

    public string? IPAddress { get; set; }

    public DateTime ViewedAt { get; set; }

    // Navigation property - Giúp lấy thông tin sản phẩm dễ dàng
    [ForeignKey("ProductId")]
    public virtual Product Product { get; set; }
}