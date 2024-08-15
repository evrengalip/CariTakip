
using Entities.Entities;
using System.ComponentModel.DataAnnotations;

namespace CariTakip.Models
{
    public class ItemViewModel
    {
        public int ItemId { get; set; }
        [Required]
        [StringLength(50)]
        [Display(Name = "Item Adı")]
        public string Name { get; set; }
        public decimal? Price { get; set; }
        
        public ICollection<EmployeeItem> EmployeeItems { get; set; } = new List<EmployeeItem>();
    }
}
