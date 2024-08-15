using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.Entities
{
    public class ChangeLogEmployee 
    {
        public int Id { get; set; }

        public string Action { get; set; }

        public string? OldValues { get; set; }

        public string NewValues { get; set; }

        public DateTime ChangeDate { get; set; }

        // User relationship
        public int? UserId { get; set; }
        [ForeignKey("UserId")]
        public User User { get; set; }

        // Employee relationship
        public int? EmployeeId { get; set; }
        [ForeignKey("EmployeeId")]
        public Employee Employee { get; set; }

      
    }
}
