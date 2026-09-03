using System.ComponentModel.DataAnnotations; // Biblioteka do walidacji (Required, StringLength itp.)

namespace oGames.Models
{
  // ViewModel formularza dodawania użytkownika (AdminPanel).
  // Nie jest to encja bazodanowa, tylko "kontrakt" dla formularza.
  public class AdminCreateUserViewModel
  {
    [Required(ErrorMessage = "Nazwa użytkownika jest wymagana")]
    [StringLength(30, MinimumLength = 3, ErrorMessage = "Nazwa musi mieć od 3 do 30 znaków")]
    public string Username { get; set; } = "";

    [Required(ErrorMessage = "Hasło jest wymagane")]
    [StringLength(100, MinimumLength = 4, ErrorMessage = "Hasło musi mieć min. 4 znaki")]
    public string Password { get; set; } = "";

    // Atrybut [Compare] sprawdza, czy to pole jest identyczne jak pole "Password".
    [Required(ErrorMessage = "Powtórz hasło")]
    [Compare("Password", ErrorMessage = "Hasła nie są identyczne")]
    public string ConfirmPassword { get; set; } = "";

    [Required(ErrorMessage = "Wybierz rolę")]
    public string Role { get; set; } = "USER"; // Domyślnie zwykły user
  }
}