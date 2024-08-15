using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Entities.Entities
{
    public class EmployeeItem
    {
        [Key]
        public int EmployeeItemId { get; set; }
        public int EmployeeId { get; set; }
        [JsonIgnore]
        public Employee Employee { get; set; }
        public int ItemId { get; set; }
        [JsonIgnore]
        public Item Item { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal TotalPaidAmount { get; set; }
        public DateTime DateTaken { get; set; } // Eşya alım tarihi
        public DateTime? PaymentDate { get; set; } // Borç ödeme tarihi
        public virtual ICollection<PaymentHistory> PaymentHistories { get; set; }
    }
}
