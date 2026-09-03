using Microsoft.EntityFrameworkCore;                // Atrybut [PrimaryKey].
using System.ComponentModel.DataAnnotations.Schema; // Atrybuty mapowania ([Table], [Column]).

namespace oGames.Models;

// Tabela łącząca Użytkowników z Grami (kto co posiada).
// Definiujemy KLUCZ ZŁOŻONY: Para (UserId + GameId) musi być unikalna.
// To zapobiega sytuacji, w której user kupuje tę samą grę dwa razy.
[PrimaryKey("UserId", "GameId")]
[Table("owned_games")]
public partial class OwnedGame
{
  // Część 1 klucza głównego i zarazem klucz obcy do tabeli Users.
  [Column("user_id")]
  public int UserId { get; set; }

  // Część 2 klucza głównego i zarazem klucz obcy do tabeli Games.
  [Column("game_id")]
  public int GameId { get; set; }

  // Data zakupu. Może być null, ale w DbContext ustawiliśmy domyślnie CURRENT_TIMESTAMP.
  [Column("purchased_at")]
  public DateTime? PurchasedAt { get; set; }

  // ==========================================
  // WŁAŚCIWOŚCI NAWIGACYJNE (Navigation Properties)
  // ==========================================
  // Dzięki nim, mając obiekt OwnedGame, możemy od razu dostać się do pełnych danych Gry i Usera.

  [ForeignKey("GameId")]
  [InverseProperty("OwnedGames")]
  public virtual Game Game { get; set; } = null!;

  [ForeignKey("UserId")]
  [InverseProperty("OwnedGames")]
  public virtual User User { get; set; } = null!;
}