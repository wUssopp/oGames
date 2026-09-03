using Microsoft.AspNetCore.Mvc; // Dostarcza bazowe funkcje kontrolera (View, Forbid itp.).

namespace oGames.Controllers
{
  // Główny kontroler panelu administracyjnego (strona startowa panelu).
  public class AdminController : Controller
  {
    // Sprawdza, czy w sesji przeglądarki zalogowany użytkownik ma rolę "admin".
    // Session to taki "schowek" na serwerze przypisany do konkretnego użytkownika.
    private bool IsAdmin()
    {
      var role = HttpContext.Session.GetString("Role");

      // StringComparison.OrdinalIgnoreCase sprawia, że "Admin" i "admin" to to samo.
      return string.Equals(role, "admin", StringComparison.OrdinalIgnoreCase);
    }

    // ==========================================
    // WIDOK PANELU
    // ==========================================

    // GET: /Admin/AdminPanel
    [HttpGet]
    public IActionResult AdminPanel()
    {
      // 1. Ochrona: Jeśli ktoś nie jest adminem
      if (!IsAdmin())
      {
        return RedirectToAction("Login", "Account");
      }

      // 2. Sukces: Zwracamy widok panelu.
      return View("~/Views/Admin/AdminPanel.cshtml");
    }
  }
}