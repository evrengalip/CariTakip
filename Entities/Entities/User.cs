
using Entities.Enum;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;


namespace Entities.Entities
{
    [Table("Users")]
    public class User
    {


        [Key]
        public int Id { get; set; }

        [StringLength(50)]
        public string? FullName { get; set; } = null;

        [Required]
        [StringLength(30)]
        public string Username { get; set; }

        [Required]
        [StringLength(100)]
        public string Password { get; set; }

        [Required]
        public string Email { get; set; }

        public bool Locked { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Required]
        public Roles Role { get; set; } = Roles.Guest;
    }

}

