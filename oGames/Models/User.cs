using Microsoft.EntityFrameworkCore;               // Indeksy ([Index]).
using System.ComponentModel.DataAnnotations;       // Walidacja ([Required], [StringLength]).
using System.ComponentModel.DataAnnotations.Schema;// Mapowanie ([Table], [Column]).

namespace oGames.Models;

// Klasa reprezentująca użytkownika systemu.
[Table("users")]
// Indeks unikalny na nazwie użytkownika.
// Dzięki temu baza danych nie pozwoli zarejestrować dwóch osób o nicku "Admin".
[Index("Username", IsUnique = true)]
public partial class User
{
  [Key]
  [Column("id")]
  public int Id { get; set; }

  [Column("username")]
  [Required]
  [StringLength(30)] // Zgodne z RegisterViewModel.
  public string Username { get; set; } = null!;

  // Hash generuje biblioteka BCrypt. Jest on nieodwracalny.
  [Column("password")]
  [Required]
  public string Password { get; set; } = null!;

  // Rola użytkownika w systemie.
  // Wartości: "USER" (zwykły klient) lub "admin" (zarządca).
  [Column("role")]
  [Required]
  public string Role { get; set; } = "USER"; // Domyślnie zwykły user.

  // ==========================================
  // RELACJE
  // ==========================================

  // Lista gier kupionych przez tego użytkownika.
  [InverseProperty("User")]
  public virtual ICollection<OwnedGame> OwnedGames { get; set; } = new List<OwnedGame>();

  // Lista recenzji napisanych przez tego użytkownika.
  [InverseProperty("User")]
  public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
}