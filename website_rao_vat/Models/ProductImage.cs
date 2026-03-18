using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace website_rao_vat.Models;

public partial class ProductImage
{
    [Key]
    public int ImageId { get; set; }

    public int? ProductId { get; set; }

    [StringLength(500)]
    public string ImageUrl { get; set; } = null!;

    public bool? IsPrimary { get; set; }

    [ForeignKey("ProductId")]
    [InverseProperty("ProductImages")]
    public virtual Product? Product { get; set; }
}
