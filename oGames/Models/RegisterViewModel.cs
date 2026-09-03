using System.ComponentModel.DataAnnotations; // Walidacja ([Required], [Compare], [DataType]).

namespace oGames.Models
{
  // Model widoku dla formularza rejestracji.
  // Służy do przekazania danych z widoku Register.cshtml do AccountController.
  public class RegisterViewModel
  {
    [Required(ErrorMessage = "Nazwa użytkownika jest wymagana.")]
    // StringLength jest lepsze niż MinLength, bo ogranicza też górny limit.
    [StringLength(30, MinimumLength = 3, ErrorMessage = "Nazwa musi mieć od 3 do 30 znaków.")]
    public string Username { get; set; } = "";

    [Required(ErrorMessage = "Hasło jest wymagane.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Hasło musi mieć co najmniej 6 znaków.")]
    [DataType(DataType.Password)] // Sprawia, że w przeglądarce pole jest typu password.
    public string Password { get; set; } = "";

    [Required(ErrorMessage = "Potwierdzenie hasła jest wymagane.")]
    // Atrybut Compare sprawdza zgodność z polem Password.
    [Compare("Password", ErrorMessage = "Hasła nie są identyczne.")]
    [DataType(DataType.Password)]
    public string ConfirmPassword { get; set; } = "";
  }
}