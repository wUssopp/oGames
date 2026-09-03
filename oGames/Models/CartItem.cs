namespace oGames.Models
{
  // Klasa pomocnicza reprezentująca pojedynczą pozycję w koszyku.
  // Obiekty tej klasy są serializowane do JSON i trzymane w Sesji przeglądarki.
  public class CartItem
  {
    public int GameId { get; set; }         // ID gry (kluczowe do sfinalizowania zakupu).

    public string Title { get; set; } = ""; // Kopia tytułu (żeby wyświetlić koszyk bez pytania o to bazy).

    public double Price { get; set; }       // Kopia ceny (jw.).
  }
}