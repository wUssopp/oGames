using Microsoft.EntityFrameworkCore;               // Indeksy ([Index]).
using System.ComponentModel.DataAnnotations;       // Walidacja ([Key], [Required], [Range]).
using System.ComponentModel.DataAnnotations.Schema;// Mapowanie bazy ([Table], [Column], [ForeignKey]).

namespace oGames.Models;

// Klasa reprezentująca recenzję gry.
[Table("reviews")]
// BARDZO WAŻNE: Unikalny indeks na parze (UserId + GameId).
// Dzięki temu baza danych nie pozwoli użytkownikowi dodać dwóch recenzji do tej samej gry.
[Index("UserId", "GameId", IsUnique = true)]
public partial class Review
{
  [Key]
  [Column("id")]
  public int Id { get; set; }

  // Klucz obcy do tabeli Users.
  [Column("user_id")]
  public int UserId { get; set; }

  // Klucz obcy do tabeli Games.
  [Column("game_id")]
  public int GameId { get; set; }

  // Ocena (gwiazdki). Dodaję walidację zakresu 1-5, dla bezpieczeństwa danych.
  [Column("rating")]
  [Range(1, 5, ErrorMessage = "Ocena musi być w zakresie 1-5.")]
  public int Rating { get; set; }

  // Treść opinii.
  [Column("content")]
  [Required(ErrorMessage = "Treść opinii jest wymagana.")]
  [MinLength(2, ErrorMessage = "Opinia jest za krótka.")]
  public string Content { get; set; } = null!;

  // Daty (ustawiane automatycznie przez bazę w DbContext za pomocą CURRENT_TIMESTAMP).
  // TypeName = "DATETIME" w SQLite jest umowne (SQLite trzyma to jako tekst), ale warto to zostawić dla porządku.
  [Column("created_at", TypeName = "DATETIME")]
  public DateTime? CreatedAt { get; set; }

  [Column("updated_at", TypeName = "DATETIME")]
  public DateTime? UpdatedAt { get; set; }

  // ==========================================
  // WŁAŚCIWOŚCI NAWIGACYJNE
  // ==========================================
  // Pozwalają pobrać autora recenzji (User) i ocenianą grę (Game).

  [ForeignKey("GameId")]
  [InverseProperty("Reviews")]
  public virtual Game Game { get; set; } = null!;

  [ForeignKey("UserId")]
  [InverseProperty("Reviews")]
  public virtual User User { get; set; } = null!;
}