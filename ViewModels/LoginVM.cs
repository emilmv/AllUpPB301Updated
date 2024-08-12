using System.ComponentModel.DataAnnotations;

namespace AllUp.ViewModels;

public class LoginVM
{
    [Required]
    public string UsernameOrEmail { get; set; }
    [Required, DataType(DataType.Password)]
    public string Password { get; set; }
    [Display(Name = "Remember Me?")]
    public bool RememberMe { get; set; }
}
