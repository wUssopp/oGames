using Microsoft.EntityFrameworkCore; // Biblioteka ORM (Object-Relational Mapping).

namespace oGames.Models;

// Główna klasa kontekstu bazy danych.
// To jest "serce" Entity Frameworka. Reprezentuje sesję z bazą danych.
// To tutaj mapujemy klasy C# (Game, User) na tabele SQL.
public partial class oGamesDbContext : DbContext
{
  // Pusty konstruktor.
  public oGamesDbContext()
  {
  }

  // Konstruktor używany przez Dependency Injection w Program.cs.
  // Przekazuje opcje (np. ConnectionString) do klasy bazowej.
  public oGamesDbContext(DbContextOptions<oGamesDbContext> options)
      : base(options)
  {
  }

  // ==========================================
  // TABELE BAZY DANYCH (DbSet)
  // ==========================================

  // Tabela gier.
  public virtual DbSet<Game> Games { get; set; }

  // Tabela zakupionych gier (łączenie User <-> Game).
  public virtual DbSet<OwnedGame> OwnedGames { get; set; }

  // Tabela recenzji.
  public virtual DbSet<Review> Reviews { get; set; }

  // Tabela użytkowników.
  public virtual DbSet<User> Users { get; set; }

  // ==========================================
  // KONFIGURACJA POŁĄCZENIA
  // ==========================================

  protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
  {
    // To jest zabezpieczenie (fallback).
    // Normalnie konfiguracja idzie z pliku appsettings.json (przez Program.cs).
    // Ale jeśli z jakiegoś powodu opcje nie zostaną przekazane, użyjemy tego stringa na sztywno.
    if (!optionsBuilder.IsConfigured)
    {
      optionsBuilder.UseSqlite("Data Source=Data/ogames.db");
    }
  }

  // ==========================================
  // KONFIGURACJA MODELU (Fluent API)
  // ==========================================

  // Tutaj precyzyjnie konfigurujemy relacje i zachowanie tabel.
  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    // Konfiguracja tabeli OwnedGame (Zakupy)
    modelBuilder.Entity<OwnedGame>(entity =>
    {
      // Domyślna wartość daty zakupu to "TERAZ" (według czasu bazy danych).
      entity.Property(e => e.PurchasedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

      // Relacja z Grą: Jeśli usuniesz Grę -> usuń wpis o posiadaniu (Cascade).
      entity.HasOne(d => d.Game)
            .WithMany(p => p.OwnedGames)
            .OnDelete(DeleteBehavior.Cascade);

      // Relacja z Userem: Jeśli usuniesz Usera -> usuń jego zakupy (Cascade).
      entity.HasOne(d => d.User)
            .WithMany(p => p.OwnedGames)
            .OnDelete(DeleteBehavior.Cascade);
    });

    // Konfiguracja tabeli Review (Recenzje)
    modelBuilder.Entity<Review>(entity =>
    {
      // Automatyczne daty utworzenia i edycji.
      entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
      entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

      // Relacja z Grą: Usunięcie Gry kasuje jej recenzje.
      entity.HasOne(d => d.Game)
            .WithMany(p => p.Reviews)
            .OnDelete(DeleteBehavior.Cascade);

      // Relacja z Userem: Usunięcie Usera kasuje jego recenzje.
      entity.HasOne(d => d.User)
            .WithMany(p => p.Reviews)
            .OnDelete(DeleteBehavior.Cascade);
    });

    OnModelCreatingPartial(modelBuilder);
  }

  partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}