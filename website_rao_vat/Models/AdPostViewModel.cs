using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace website_rao_vat.Models
{
    public class AdPostViewModel
    {
        public string Title { get; set; }
        public decimal Price { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public int CategoryId { get; set; }

        // Tên này phải là "Images" để khớp với asp-for="Images"
        public List<IFormFile> Images { get; set; }
    }
}