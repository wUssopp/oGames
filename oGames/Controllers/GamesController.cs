using Microsoft.AspNetCore.Mvc;              // Bazowe funkcjonalności kontrolera.
using Microsoft.EntityFrameworkCore;         // Obsługa bazy danych (Include, ToListAsync).
using oGames.Models;                         // Dostęp do modeli (Game, Review, etc.).

namespace oGames.Controllers
{
  public class GamesController : Controller
  {
    private readonly oGamesDbContext _context; // Kontekst bazy danych.
    private readonly IWebHostEnvironment _env; // Pozwala pobrać ścieżkę do wwwroot (dla zdjęć).

    public GamesController(oGamesDbContext context, IWebHostEnvironment env)
    {
      _context = context;
      _env = env;
    }

    // GET: /Games/List
    // Wyświetla katalog gier.
    // Teraz pobieramy wszystkie gry, aby site.js mógł je filtrować na żywo.
    public async Task<IActionResult> List(string q)
    {
      // 1. Zaczynamy budować zapytanie (AsQueryable), ale jeszcze nie wysyłamy go do bazy
      var gamesQuery = _context.Games
          .Include(g => g.Reviews)
          .AsQueryable();

      // 2. Sprawdzamy, czy przesłano frazę wyszukiwania (q)
      if (!string.IsNullOrEmpty(q))
      {
        // Filtrujemy gry, których tytuł zawiera wpisaną frazę
        gamesQuery = gamesQuery.Where(g => g.Title.ToLower().Contains(q.ToLower()));

        // Zapisujemy szukaną frazę w ViewBag, żeby np. wyświetlić ją w inpucie na liście
        ViewBag.SearchQuery = q;
      }

      // 3. Wykonujemy finalne zapytanie do bazy (ToListAsync)
      var games = await gamesQuery
          .OrderBy(g => g.Id)
          .ToListAsync();

      ViewBag.GameCount = games.Count;

      return View("~/Views/Games/List.cshtml", games);
    }

    // GET: /Games/Details/{id}
    // Wyświetla szczegóły gry, galerię i recenzje.
    [HttpGet]
    public async Task<IActionResult> Details(int? id)
    {
      if (id == null) return NotFound();

      // Pobieramy grę wraz z opiniami i autorami tych opinii.
      var game = await _context.Games
          .Include(g => g.Reviews)
          .ThenInclude(r => r.User)
          .FirstOrDefaultAsync(g => g.Id == id.Value);

      if (game == null) return NotFound();

      // --- OBSŁUGA GALERII ZDJĘĆ ---
      // Czyścimy ścieżkę do folderu.
      var imageFolder = game.ImageDir?.Trim().TrimEnd('/').TrimStart('/');

      // Zabezpieczenie: nie pozwalamy na wychodzenie w górę drzewa katalogów.
      if (!string.IsNullOrEmpty(imageFolder) && imageFolder.Contains(".."))
        imageFolder = null;

      var images = new List<string>();

      if (!string.IsNullOrEmpty(imageFolder))
      {
        // Łączymy ścieżkę fizyczną do wwwroot z folderem gry.
        var physicalPath = Path.Combine(_env.WebRootPath, imageFolder);

        if (Directory.Exists(physicalPath))
        {
          // Skanujemy folder w poszukiwaniu plików graficznych.
          images = Directory.GetFiles(physicalPath, "*.*")
              .Where(f => f.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase)
                       || f.EndsWith(".png", StringComparison.OrdinalIgnoreCase)
                       || f.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase))
              .Select(Path.GetFileName) // Bierzemy samą nazwę pliku.
              .Take(6)                  // Ograniczamy do 6 zdjęć.
              .ToList();
        }
      }

      ViewBag.Images = images;

      return View("~/Views/Games/Details.cshtml", game);
    }

    // POST: /Games/AddReview
    // Dodaje lub edytuje opinię zalogowanego użytkownika.
    [HttpPost]
    [ValidateAntiForgeryToken] // Ochrona CSRF.
    public async Task<IActionResult> AddReview(int gameId, int rating, string content)
    {
      // 1. Sprawdzamy, czy użytkownik jest zalogowany.
      var userId = HttpContext.Session.GetInt32("UserId");
      if (userId == null)
      {
        return RedirectToAction("Login", "Account");
      }

      // 2. Walidacja danych wejściowych.
      rating = Math.Clamp(rating, 1, 5); // Ocena musi być 1-5.
      content = (content ?? "").Trim();

      // Jeśli treść za krótka, wracamy bez zapisu.
      if (content.Length < 2)
        return RedirectToAction(nameof(Details), new { id = gameId });

      // 3. Sprawdzamy, czy ten użytkownik już oceniał tę grę
      // Używamy AsNoTracking dla wydajności przy samym odczycie
      var existing = await _context.Reviews
          .AsNoTracking()
          .FirstOrDefaultAsync(r => r.GameId == gameId && r.UserId == userId.Value);

      // Jeśli opinia istnieje, NIE edytujemy. Blokujemy akcję.
      if (existing != null)
      {
        TempData["ReviewError"] = "Dodałeś już jedną opinię do tej gry.";
        return RedirectToAction(nameof(Details), new { id = gameId });
      }

      if (existing == null)
      {
        // Nowa opinia.
        _context.Reviews.Add(new Review
        {
          GameId = gameId,
          UserId = userId.Value,
          Rating = rating,
          Content = content
          // CreatedAt/UpdatedAt ustawią się w bazie automatycznie.
        });
      }
      else
      {
        // Edycja istniejącej opinii.
        existing.Rating = rating;
        existing.Content = content;
        existing.UpdatedAt = DateTime.Now; // Aktualizujemy datę edycji.
      }

      await _context.SaveChangesAsync();
      return RedirectToAction(nameof(Details), new { id = gameId });
    }

    // POST: /Games/DeleteReview
    // Usuwa opinię (dostępne tylko dla autora opinii lub admina).
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteReview(int id, int gameId)
    {
      var userId = HttpContext.Session.GetInt32("UserId");
      if (userId == null)
        return RedirectToAction("Login", "Account");

      // Szukamy opinii w bazie.
      var review = await _context.Reviews.FirstOrDefaultAsync(r => r.Id == id);

      // Jeśli nie znaleziono lub ID gry się nie zgadza (zabezpieczenie) -> powrót.
      if (review == null || review.GameId != gameId)
        return RedirectToAction(nameof(Details), new { id = gameId });

      // Sprawdzamy rolę admina.
      var role = HttpContext.Session.GetString("Role");
      var isAdmin = string.Equals(role, "admin", StringComparison.OrdinalIgnoreCase);

      // UPRAWNIENIA: Usuwać może tylko autor lub admin.
      if (review.UserId != userId.Value && !isAdmin)
        return Forbid();

      _context.Reviews.Remove(review);
      await _context.SaveChangesAsync();

      return RedirectToAction(nameof(Details), new { id = gameId });
    }
  }
}