using Microsoft.AspNetCore.Mvc;              // Bazowe klasy kontrolera.
using Microsoft.EntityFrameworkCore;         // Obs³uga bazy danych (CountAsync).
using oGames.Models;                         // Dostêp do DbContext i ErrorViewModel.
using System.Diagnostics;                    // Potrzebne do Activity.Current (œledzenie b³êdów).

namespace oGames.Controllers
{
  // Kontroler strony g³ównej.
  public class HomeController : Controller
  {
    private readonly oGamesDbContext _context; // Potrzebujemy bazy, ¿eby policzyæ gry.

    // Konstruktor: Wstrzykujemy tylko DbContext.
    public HomeController(oGamesDbContext context)
    {
      _context = context;
    }

    // GET: / (Strona g³ówna)
    public async Task<IActionResult> Index()
    {
      // Pobieramy liczbê gier w bazie, ¿eby wyœwietliæ statystykê na powitanie.
      var gameCount = await _context.Games.CountAsync();

      // Przekazujemy liczbê do widoku przez ViewBag.
      ViewBag.GameCount = gameCount;

      // Œcie¿ka do widoku strony g³ównej.
      return View("~/Views/Home/Index.cshtml");
    }

    // Obs³uga b³êdów.
    // ResponseCache wy³¹cza cache'owanie tej strony przez przegl¹darkê.
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
      // Tworzymy model b³êdu z ID ¿¹dania (pomaga w debugowaniu logów).
      var model = new ErrorViewModel
      {
        RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
      };

      return View("~/Views/Shared/Error.cshtml", model);
    }
  }
}