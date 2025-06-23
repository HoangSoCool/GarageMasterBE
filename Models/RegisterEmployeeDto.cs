using System.ComponentModel.DataAnnotations;

namespace GarageMasterBE.Models
{
  public class RegisterEmployeeDto
  {
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Phone { get; set; } = "";
    public string Address { get; set; } = "";
  }
}