using System.ComponentModel.DataAnnotations; // Walidacja ([Required], [DataType]).

namespace oGames.Models
{
  // Model widoku dla formularza logowania.
  // Służy tylko do odebrania danych wpisanych przez użytkownika.
  public class LoginViewModel
  {
    [Required(ErrorMessage = "Proszę podać nazwę użytkownika.")]
    public string Username { get; set; } = "";

    [Required(ErrorMessage = "Proszę podać hasło.")]
    [DataType(DataType.Password)] // Podpowiedź dla widoku, że to pole ma być ukryte.
    public string Password { get; set; } = "";
  }
}