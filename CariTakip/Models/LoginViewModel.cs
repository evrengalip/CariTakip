using System.ComponentModel.DataAnnotations;

namespace CariTakip.Models
{
    public class LoginViewModel
    {


        [Required(ErrorMessage = "Username is required.")]
        [StringLength(30, ErrorMessage = "Username can be max 30 characters.")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [StringLength(16, MinimumLength = 6, ErrorMessage = "Password must be between 6 and 16 characters.")]
        public string Password { get; set; }




    }
}
