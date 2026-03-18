using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace website_rao_vat.Models;

[Index("UserId", "ProductId", Name = "UQ__Favorite__DCC800217873D14B", IsUnique = true)]
public partial class Favorite
{
    [Key]
    public int FavoriteId { get; set; }

    public int? UserId { get; set; }

    public int? ProductId { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreatedAt { get; set; }

    [ForeignKey("ProductId")]
    [InverseProperty("Favorites")]
    public virtual Product? Product { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("Favorites")]
    public virtual User? User { get; set; }
}
