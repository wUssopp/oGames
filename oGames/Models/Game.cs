using System.ComponentModel.DataAnnotations;       // Walidacja ([Required], [Range]).
using System.ComponentModel.DataAnnotations.Schema;// Mapowanie bazy ([Table], [Column]).

namespace oGames.Models;

// Klasa reprezentująca tabelę 'games' w bazie danych SQLite.
[Table("games")]
public partial class Game
{
  [Key]
  [Column("id")]
  public int Id { get; set; }

  // Tytuł jest wymagany. Dodaję komunikat błędu dla formularza w Adminie.
  [Column("title")]
  [Required(ErrorMessage = "Podaj tytuł gry.")]
  public string Title { get; set; } = null!;

  // Cena: musi być liczbą dodatnią.
  [Column("price")]
  [Range(0, 10000, ErrorMessage = "Cena musi być większa od 0.")]
  public double Price { get; set; }

  [Column("description")]
  public string? Description { get; set; }

  [Column("long_description")]
  public string? LongDescription { get; set; }

  // Ścieżka do folderu ze zdjęciami.
  [Column("image_dir")]
  public string? ImageDir { get; set; }

  // Relacja 1:N -> Jedna gra może być posiadana przez wielu użytkowników (wpisy w OwnedGame).
  [InverseProperty("Game")]
  public virtual ICollection<OwnedGame> OwnedGames { get; set; } = new List<OwnedGame>();

  // Relacja 1:N -> Jedna gra może mieć wiele recenzji.
  [InverseProperty("Game")]
  public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
}