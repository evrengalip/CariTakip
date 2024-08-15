using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Entities.Entities
{
    public class Employee
    {
        [Key]
        public int EmployeeId { get; set; }

        
        public string Name { get; set; }
        public string Department { get; set; }
        public string Position { get; set; }
        public string Email { get; set; }
        [JsonIgnore]
        public ICollection<EmployeeItem> EmployeeItems { get; set; } = new List<EmployeeItem>();
       
    }
}
