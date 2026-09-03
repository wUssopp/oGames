using Microsoft.AspNetCore.Mvc;              // Bazowe funkcje kontrolera.
using Microsoft.EntityFrameworkCore;         // Obsługa bazy danych (Include, FirstOrDefaultAsync).
using oGames.Models;                         // Modele User, OwnedGame, Game.

namespace oGames.Controllers
{
  // Kontroler odpowiedzialny za wyświetlanie profilu użytkownika.
  // Obsługuje zarówno "Mój profil" jak i podgląd profilu innej osoby.
  public class ProfileController : Controller
  {
    private readonly oGamesDbContext _context;

    public ProfileController(oGamesDbContext context)
    {
      _context = context;
    }

    // GET: /Profile (mój profil) LUB /Profile/5 (profil usera o ID 5)
    // Atrybut [Route] pozwala obsłużyć oba adresy jedną metodą.
    [HttpGet]
    [Route("Profile/{id?}")]
    public async Task<IActionResult> Profile(int? id)
    {
      // 1. Sprawdzamy, kto jest zalogowany (pobieramy ID z sesji).
      var loggedUserId = HttpContext.Session.GetInt32("UserId");

      // 2. Jeśli nikt nie jest zalogowany I nie podano ID w URL -> odsyłamy do logowania.
      // (Nie pozwalamy niezalogowanym (gostkom) przeglądać profili).
      if (id == null && loggedUserId == null)
        return RedirectToAction("Login", "Account");

      // 3. Ustalamy, czyj profil wyświetlić (Target User).
      // Operator '??' oznacza: jeśli 'id' ma wartość, użyj jej. Jeśli jest null, użyj 'loggedUserId'.
      int targetUserId = id ?? loggedUserId.Value;

      // 4. Pobieramy dane użytkownika z bazy (żeby wyświetlić nazwę i rolę).
      var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == targetUserId);

      // Jeśli podano ID użytkownika, który nie istnieje -> 404.
      if (user == null) return NotFound();

      // 5. Sprawdzamy, czy to jest profil osoby, która właśnie patrzy na ekran.
      // Jeśli ID z sesji == ID wyświetlanego profilu, to "Mój profil".
      bool isMyProfile = (loggedUserId != null && loggedUserId.Value == targetUserId);

      // Przekazujemy flagi i dane do widoku.
      ViewBag.ProfileUsername = user.Username;
      ViewBag.ProfileRole = user.Role;
      ViewBag.IsMyProfile = isMyProfile;

      // 6. Pobieramy listę gier posiadanych przez tego użytkownika.
      var owned = await _context.OwnedGames
          .Where(og => og.UserId == targetUserId)
          .Include(og => og.Game)      // Dołączamy tabelę Games, żeby mieć dostęp do Tytułu gry.
          .OrderByDescending(og => og.PurchasedAt) // Sortujemy: ostatnio kupione na górze.
          .ToListAsync();

      // Zwracamy widok.
      return View("~/Views/Account/Profile.cshtml", owned);
    }
  }
}