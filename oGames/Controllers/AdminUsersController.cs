using Microsoft.AspNetCore.Mvc;              // Controller, IActionResult, Forbid(), RedirectToAction(), TempData.
using Microsoft.EntityFrameworkCore;         // EF Core: ToListAsync(), AnyAsync(), FirstOrDefaultAsync(), DbUpdateException.
using oGames.Models;                         // User + ViewModel: AdminCreateUserViewModel.

namespace oGames.Controllers
{
  // Kontroler do administracyjnego zarządzania użytkownikami (lista, dodawanie, usuwanie).
  public class AdminUsersController : Controller
  {
    private readonly oGamesDbContext _context; // DbContext do pracy z bazą danych.

    // Konstruktor z DI (Dependency Injection) - przekazujemy kontekst bazy.
    public AdminUsersController(oGamesDbContext context) => _context = context;

    // Sprawdza, czy zalogowany użytkownik ma rolę admin (rola w sesji).
    private bool IsAdmin()
        => string.Equals(
            HttpContext.Session.GetString("Role"), // Rola zapisana w sesji przy logowaniu.
            "admin",                               // Porównujemy do "admin".
            StringComparison.OrdinalIgnoreCase     // Case-insensitive (wielkość liter nie ma znaczenia).
        );

    // GET: /AdminUsers/AdminUsers
    // Wyświetla listę użytkowników z opcją wyszukiwania.
    [HttpGet]
    public async Task<IActionResult> AdminUsers(string? q)
    {
      // Ochrona akcji: jeśli nie admin -> login.
      if (!IsAdmin()) return RedirectToAction("Login", "Account");

      // Pobranie wszystkich użytkowników jako zapytania (jeszcze niewykonane).
      var query = _context.Users.AsQueryable();

      // Jeśli wpisano coś w wyszukiwarkę (parametr q):
      if (!string.IsNullOrWhiteSpace(q))
      {
        q = q.Trim();
        // SQL: WHERE username LIKE '%q%'
        query = query.Where(u => u.Username.Contains(q));
      }

      // Wykonanie zapytania i sortowanie po ID.
      var users = await query.OrderBy(u => u.Id).ToListAsync();

      // Przekazujemy wpisaną frazę z powrotem do widoku, żeby została w polu input.
      ViewBag.SearchQuery = q;

      // Jawna ścieżka do widoku.
      return View("~/Views/Admin/AdminUsers.cshtml", users);
    }

    // GET: /AdminUsers/AdminUsersCreate
    // Wyświetla pusty formularz tworzenia użytkownika.
    [HttpGet]
    public IActionResult AdminUsersCreate()
    {
      if (!IsAdmin()) return Forbid();

      // Jawna ścieżka do widoku.
      return View("~/Views/Admin/AdminUsersCreate.cshtml");
    }

    // POST: /AdminUsers/AdminUsersCreate
    // Odbiera dane z formularza, waliduje i tworzy usera.
    [HttpPost]
    [ValidateAntiForgeryToken] // Zabezpieczenie przed atakami CSRF.
    public async Task<IActionResult> AdminUsersCreate(AdminCreateUserViewModel model)
    {
      if (!IsAdmin()) return Forbid();

      // 1. Walidacja adnotacji (Required, MinLength itd. z ViewModelu).
      if (!ModelState.IsValid)
        return View("~/Views/Admin/AdminUsersCreate.cshtml", model);

      // 2. Sprawdzenie unikalności loginu w bazie.
      if (await _context.Users.AnyAsync(u => u.Username == model.Username))
      {
        ModelState.AddModelError("Username", "Taki użytkownik już istnieje.");
        return View("~/Views/Admin/AdminUsersCreate.cshtml", model);
      }

      // 3. Haszowanie hasła.
      var hashed = BCrypt.Net.BCrypt.HashPassword(model.Password);

      // 4. Przepisanie danych do bazy.
      var user = new User
      {
        Username = model.Username,
        Password = hashed,
        Role = model.Role // Rola wybrana w select (USER lub admin).
      };

      // 5. Zapis.
      _context.Users.Add(user);
      await _context.SaveChangesAsync();

      return RedirectToAction(nameof(AdminUsers));
    }

    // POST: /AdminUsers/AdminUsersDelete
    // Usuwa użytkownika po ID.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AdminUsersDelete(int id)
    {
      if (!IsAdmin()) return Forbid();

      // ZABEZPIECZENIE: Admin nie może usunąć konta, na którym jest obecnie zalogowany.
      var currentUserId = HttpContext.Session.GetInt32("UserId");
      if (currentUserId != null && currentUserId.Value == id)
      {
        TempData["AdminUsersError"] = "Nie możesz usunąć aktualnie zalogowanego konta.";
        return RedirectToAction(nameof(AdminUsers));
      }

      // Pobieramy usera do usunięcia.
      var user = await _context.Users
          .FirstOrDefaultAsync(u => u.Id == id);

      if (user == null) return NotFound();

      // Oznaczamy do usunięcia.
      _context.Users.Remove(user);

      try
      {
        // Próba zapisu w bazie.
        // Jeśli user ma powiązane dane (np. gry, opinie) i baza nie ma włączonego CASCADE DELETE,
        // tutaj pokaże się wyjątek.
        await _context.SaveChangesAsync();
      }
      catch (DbUpdateException)
      {
        // Łapiemy błąd bazy danych i wyświetlamy ładny komunikat zamiast błędu serwera.
        TempData["AdminUsersError"] = "Nie udało się usunąć użytkownika.";
      }

      return RedirectToAction(nameof(AdminUsers));
    }
  }
}