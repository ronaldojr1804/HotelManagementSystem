using System.ComponentModel.DataAnnotations;

namespace HotelManagementSystem.Web.Models;

public class LoginModel
{
    [Required(ErrorMessage = "O email é obrigatório")]
    public string Email { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "A senha é obrigatória")]
    public string Password { get; set; } = string.Empty;
}
