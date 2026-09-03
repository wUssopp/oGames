using Microsoft.AspNetCore.Mvc;              // Controller, IActionResult, atrybuty HttpGet/HttpPost, Forbid(), NotFound().
using Microsoft.EntityFrameworkCore;         // EF Core: ToListAsync(), FindAsync(), FirstOrDefaultAsync().
using oGames.Models;                         // Model Game i DbContext.

namespace oGames.Controllers
{
  // Kontroler administracyjny do zarządzania grami (CRUD).
  public class AdminGamesController : Controller
  {
    private readonly oGamesDbContext _context; // Dostęp do bazy danych.

    // Konstruktor z DI; zapisujemy DbContext w polu.
    public AdminGamesController(oGamesDbContext context) => _context = context;

    // Sprawdzenie uprawnień admina na podstawie roli w sesji.
    private bool IsAdmin()
        => string.Equals(
            HttpContext.Session.GetString("Role"),
            "admin",
            StringComparison.OrdinalIgnoreCase
        );

    // LISTA: GET /AdminGames/AdminGames
    // Wyświetla tabelę wszystkich gier w panelu admina.
    [HttpGet]
    public async Task<IActionResult> AdminGames()
    {
      // Jeśli nie admin -> login.
      if (!IsAdmin()) return RedirectToAction("Login", "Account");

      // Pobieramy wszystkie gry z bazy posortowane po Id.
      var games = await _context.Games
          .OrderBy(g => g.Id)
          .ToListAsync();

      // Zwracamy widok z pełną ścieżką.
      return View("~/Views/Admin/AdminGames.cshtml", games);
    }

    // CREATE (GET): GET /AdminGames/AdminGamesCreate
    // Wyświetla pusty formularz dodawania nowej gry.
    [HttpGet]
    public IActionResult AdminGamesCreate()
    {
      if (!IsAdmin()) return Forbid();

      // Zwracamy widok formularza.
      return View("~/Views/Admin/AdminGamesCreate.cshtml");
    }

    // CREATE (POST): Zapis nowej gry wysłanej z formularza.
    [HttpPost]
    [ValidateAntiForgeryToken] // Chroni przed fałszerstwem żądania (CSRF).
    public async Task<IActionResult> AdminGamesCreate(Game game)
    {
      if (!IsAdmin()) return Forbid();

      // Walidacja modelu (czy wpisano wymagane pola, czy cena to liczba itp.).
      if (!ModelState.IsValid)
      {
        // Jak błąd -> wracamy do formularza z tym, co user wpisał (żeby nie kasować).
        return View("~/Views/Admin/AdminGamesCreate.cshtml", game);
      }

      // EF Core śledzi nowy obiekt.
      _context.Games.Add(game);

      // Fizyczny zapis SQL INSERT do bazy.
      await _context.SaveChangesAsync();

      // Powrót do listy gier.
      return RedirectToAction(nameof(AdminGames));
    }

    // EDIT (GET): Wyświetla formularz edycji istniejącej gry.
    [HttpGet]
    public async Task<IActionResult> AdminGamesEdit(int id)
    {
      if (!IsAdmin()) return Forbid();

      // Pobieramy grę po ID (Primary Key).
      var game = await _context.Games.FindAsync(id);

      // Jeśli gry nie ma (np. błędne ID w URL) -> błąd 404.
      if (game == null) return NotFound();

      return View("~/Views/Admin/AdminGamesEdit.cshtml", game);
    }

    // EDIT (POST): Zapis zmian w edytowanej grze.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AdminGamesEdit(Game game)
    {
      if (!IsAdmin()) return Forbid();

      if (!ModelState.IsValid)
        return View("~/Views/Admin/AdminGamesEdit.cshtml", game);

      // BEZPIECZNIEJSZY wariant niż _context.Games.Update(game):
      // Pobieramy encję z bazy i aktualizujemy tylko dozwolone pola,
      // żeby nie nadpisać przypadkiem innych danych (overposting).
      var dbGame = await _context.Games.FindAsync(game.Id);
      if (dbGame == null) return NotFound();

      // Mapowanie pól z formularza -> do encji śledzonej przez EF.
      dbGame.Title = game.Title;
      dbGame.Price = game.Price;
      dbGame.Description = game.Description;
      dbGame.LongDescription = game.LongDescription;
      dbGame.ImageDir = game.ImageDir;

      // Zapis do bazy (SQL UPDATE).
      await _context.SaveChangesAsync();

      return RedirectToAction(nameof(AdminGames));
    }

    // DELETE (POST): usuwa grę.
    [HttpPost]
    [ValidateAntiForgeryToken] // Ochrona CSRF.
    public async Task<IActionResult> AdminGamesDelete(int id)
    {
      if (!IsAdmin()) return Forbid();

      // Szukamy gry po Id.
      var game = await _context.Games.FindAsync(id);

      // Jeśli nie ma -> 404.
      if (game == null) return NotFound();

      // Usuwamy i zapisujemy.
      _context.Games.Remove(game);
      await _context.SaveChangesAsync();

      return RedirectToAction(nameof(AdminGames));
    }
  }
}