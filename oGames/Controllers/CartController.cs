using Microsoft.AspNetCore.Mvc;              // Controller, IActionResult, RedirectToAction(), NotFound().
using Microsoft.EntityFrameworkCore;         // EF Core: FirstOrDefaultAsync, AnyAsync, ToListAsync.
using oGames.Helpers;                        // Session.GetObject/SetObject (JSON w sesji).
using oGames.Models;                         // oGamesDbContext, Game, OwnedGame, CartItem.

namespace oGames.Controllers
{
  // Kontroler koszyka.
  // Koszyk nie jest trzymany w bazie danych, tylko w SESJI przeglądarki.
  // Dopiero przy Checkout (zakupie) dane trafiają do tabeli OwnedGames w bazie.
  public class CartController : Controller
  {
    private readonly oGamesDbContext _context; // Baza danych (potrzebna do pobrania info o grze i zapisu zakupu).
    private const string CartKey = "Cart";     // Klucz w sesji, gdzie trzymamy listę zakupów.

    public CartController(oGamesDbContext context)
    {
      _context = context;
    }

    // GET: /Cart/Cart
    // Wyświetla zawartość koszyka.
    public IActionResult Cart()
    {
      // 1. Pobieramy listę z sesji. Jeśli pusta/null -> tworzymy nową listę.
      var cart = HttpContext.Session.GetObject<List<CartItem>>(CartKey)
                 ?? new List<CartItem>();

      // 2. Liczymy sumę do wyświetlenia.
      ViewBag.Total = cart.Sum(x => x.Price);

      // 3. Zwracamy widok.
      return View("~/Views/Cart/Cart.cshtml", cart);
    }

    // POST: /Cart/Add
    // Dodaje grę do koszyka (przyjmuje ID gry z formularza).
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(int gameId)
    {
      // 1. Sprawdzamy, czy gra w ogóle istnieje w bazie.
      var game = await _context.Games
          .FirstOrDefaultAsync(g => g.Id == gameId);

      if (game == null) return NotFound();

      // 2. Pobieramy aktualny stan koszyka z sesji.
      var cart = HttpContext.Session.GetObject<List<CartItem>>(CartKey)
                 ?? new List<CartItem>();

      // 3. Sprawdzamy, czy gry już nie ma w koszyku (żeby nie dodać dwa razy tego samego).
      if (!cart.Any(x => x.GameId == game.Id))
      {
        // Tworzymy uproszczony obiekt CartItem.
        cart.Add(new CartItem
        {
          GameId = game.Id,
          Title = game.Title,
          Price = game.Price
        });

        // 4. Zapisujemy zaktualizowaną listę z powrotem do sesji.
        HttpContext.Session.SetObject(CartKey, cart);
      }

      // Wracamy do widoku koszyka.
      return RedirectToAction(nameof(Cart));
    }

    // POST: /Cart/Remove
    // Usuwa pozycję z koszyka.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Remove(int gameId)
    {
      var cart = HttpContext.Session.GetObject<List<CartItem>>(CartKey)
                 ?? new List<CartItem>();

      // Usuwamy element z listy w pamięci.
      cart.RemoveAll(x => x.GameId == gameId);

      // Nadpisujemy sesję nową listą.
      HttpContext.Session.SetObject(CartKey, cart);

      return RedirectToAction(nameof(Cart));
    }

    // POST: /Cart/Checkout
    // Finalizacja transakcji.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Checkout()
    {
      // 1. Sprawdzenie, czy użytkownik jest zalogowany.
      var userId = HttpContext.Session.GetInt32("UserId");
      if (userId == null)
        return RedirectToAction("Login", "Account");

      // 2. Pobranie koszyka.
      var cart = HttpContext.Session.GetObject<List<CartItem>>(CartKey)
                 ?? new List<CartItem>();

      if (!cart.Any())
        return RedirectToAction(nameof(Cart));

      // 3. Sprawdzamy, które gry z koszyka użytkownik już posiada.
      var cartGameIds = cart.Select(c => c.GameId).ToList();

      var alreadyOwnedIds = await _context.OwnedGames
          .Where(og => og.UserId == userId.Value && cartGameIds.Contains(og.GameId))
          .Select(og => og.GameId)
          .ToListAsync();

      // --- BLOKADA ZAKUPU (duplikaty) ---
      if (alreadyOwnedIds.Any())
      {
        // Pobieramy tytuły tych gier z koszyka (bo w OwnedGames mamy tylko ID, a w koszyku mamy Tytuły)
        var duplicateTitles = cart
            .Where(c => alreadyOwnedIds.Contains(c.GameId))
            .Select(c => c.Title)
            .ToList();

        // Tworzymy komunikat błędu
        var errorMsg = $"Posiadasz już w bibliotece następujące gry: {string.Join(", ", duplicateTitles)}. Usuń je z koszyka, aby kontynuować.";

        // Zapisujemy błąd do TempData (żeby wyświetlił się po przekierowaniu)
        TempData["CartError"] = errorMsg;

        // Wracamy do koszyka
        return RedirectToAction(nameof(Cart));
      }
      // ------------------------------------

      // 4. Jeśli nie ma duplikatów, dodajemy gry do bazy.
      foreach (var item in cart)
      {
        _context.OwnedGames.Add(new OwnedGame
        {
          UserId = userId.Value,
          GameId = item.GameId
          // PurchasedAt ustawi się automatycznie w bazie.
        });
      }

      // 5. Fizyczny zapis do bazy (COMMIT).
      await _context.SaveChangesAsync();

      // 6. Wyczyszczenie koszyka po udanym zakupie.
      HttpContext.Session.Remove(CartKey);

      return RedirectToAction(nameof(ThankYou));
    }


    // GET: /Cart/ThankYou
    // Ekran podziękowania.
    public IActionResult ThankYou()
    {
      return View("~/Views/Cart/ThankYou.cshtml");
    }

    // POST: /Cart/Clear
    // Ręczne wyczyszczenie koszyka przez użytkownika.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Clear()
    {
      HttpContext.Session.Remove(CartKey);
      return RedirectToAction(nameof(Cart));
    }
  }
}