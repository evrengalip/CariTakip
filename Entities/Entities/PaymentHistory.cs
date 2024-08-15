using System.ComponentModel.DataAnnotations;

namespace Entities.Entities
{
    public class PaymentHistory
    {
        [Key]
        public int PaymentHistoryId { get; set; }
        public int EmployeeItemId { get; set; }
        public EmployeeItem EmployeeItem { get; set; }
        public decimal PaidAmount { get; set; }
        
        public DateTime PaymentDate { get; set; }

        public decimal RemainingDebt { get; set; }
    }
}
