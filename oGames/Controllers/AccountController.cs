using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using oGames.Models;

namespace oGames.Controllers
{
  public class AccountController : Controller
  {
    private readonly oGamesDbContext _context;

    public AccountController(oGamesDbContext context)
    {
      _context = context;
    }

    // ==========================================
    // METODY POMOCNICZE
    // ==========================================

    // Ustawia kluczowe dane użytkownika w sesji (zalogowanie).
    private void SetUserSession(User user)
    {
      HttpContext.Session.SetInt32("UserId", user.Id);
      HttpContext.Session.SetString("Username", user.Username);
      HttpContext.Session.SetString("Role", user.Role);
    }

    // ==========================================
    // LOGOWANIE
    // ==========================================

    // GET: Wyświetla połączony widok logowania i rejestracji.
    [HttpGet]
    public IActionResult Login() => View();

    // POST: Próba zalogowania.
    [HttpPost]
    [ValidateAntiForgeryToken] // Zabezpieczenie przed atakami CSRF.
    public async Task<IActionResult> Login(LoginViewModel model)
    {
      // 1. Sprawdzamy, czy pola nie są puste (wymagane przez adnotacje w Modelu).
      if (!ModelState.IsValid)
        return View(model);

      // 2. Szukamy użytkownika w bazie po nazwie.
      var user = await _context.Users
          .FirstOrDefaultAsync(u => u.Username == model.Username);

      // 3. Weryfikacja:
      // Czy user w ogóle istnieje?
      // Czy hasło wpisane w formularzu pasuje do hasha w bazie (BCrypt)?
      if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.Password))
      {
        ViewBag.Error = "Błędny login lub hasło.";
        return View(model); // Wracamy do formularza z wpisanymi danymi, żeby user nie musiał pisać od nowa.
      }

      // 4. Sukces - ustawiamy sesję (logujemy) i przekierowujemy na Home.
      SetUserSession(user);
      return RedirectToAction("Index", "Home");
    }

    // ==========================================
    // REJESTRACJA
    // ==========================================

    // POST: Tworzenie nowego konta.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
      // 1. Walidacja formularza (długość hasła, powtórzenie hasła itp.).
      if (!ModelState.IsValid)
      {
        // Musimy zwrócić widok "Login", ale nie możemy przekazać 'model' (RegisterViewModel),
        // bo widok Login oczekuje LoginViewModel. Wyświetlamy sam widok z błędami w ModelState.
        return View("Login");
      }

      // 2. Sprawdzenie unikalności loginu.
      var exists = await _context.Users.AnyAsync(u => u.Username == model.Username);
      if (exists)
      {
        ViewBag.RegisterError = "Taki użytkownik już istnieje.";
        return View("Login");
      }

      // 3. Hashowanie hasła.
      var hashed = BCrypt.Net.BCrypt.HashPassword(model.Password);

      // 4. Tworzenie obiektu encji.
      var user = new User
      {
        Username = model.Username,
        Password = hashed,
        Role = "USER" // Domyślna rola to zwykły użytkownik.
      };

      // 5. Zapis do bazy danych.
      _context.Users.Add(user);
      await _context.SaveChangesAsync();

      // 6. Automatyczne logowanie po rejestracji (żeby nie kazać mu się logować ręcznie).
      SetUserSession(user);

      return RedirectToAction("Index", "Home");
    }

    // ==========================================
    // WYLOGOWANIE
    // ==========================================

    public IActionResult Logout()
    {
      // Czyścimy całą sesję (User zapomina tożsamość).
      HttpContext.Session.Clear();
      return RedirectToAction("Index", "Home");
    }
  }
}