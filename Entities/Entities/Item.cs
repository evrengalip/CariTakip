using System.ComponentModel.DataAnnotations;

namespace Entities.Entities
{
    public class Item
    {
        [Key]
        public int ItemId { get; set; }

        
        public string? Name { get; set; }

        [DataType(DataType.Currency)]
        
        public decimal Price { get; set; }
        public string? ImagePath { get; set; }
        public virtual ICollection<EmployeeItem> EmployeeItems { get; set; }=new List<EmployeeItem>();


    }
}
