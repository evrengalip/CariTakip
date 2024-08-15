namespace Entities.Entities
{
    public class CombinedChangeLog
    {
        public string Action { get; set; }
        public string OldValues { get; set; }
        public string NewValues { get; set; }
        public DateTime ChangeDate { get; set; }
        public string User { get; set; }
        public string EmployeeName { get; set; }
        public string ItemName { get; set; }
        public string LogType { get; set; } // "Employee" veya "Item"

    }

}
