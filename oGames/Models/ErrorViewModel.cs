namespace oGames.Models
{
  // Model widoku b³êdu (u¿ywany w Views/Shared/Error.cshtml).
  // Nie jest zapisywany w bazie danych. S³u¿y tylko do wyœwietlenia informacji o b³êdzie.
  public class ErrorViewModel
  {
    // Unikalny identyfikator ¿¹dania HTTP (Trace Identifier).
    // Przydatny przy debugowaniu - pozwala znaleŸæ konkretny b³¹d w logach serwera.
    public string? RequestId { get; set; }

    // W³aœciwoœæ wyliczana (Expression-bodied property).
    // Zwraca true, jeœli RequestId ma jak¹œ wartoœæ.
    // Widok u¿ywa tego pola, aby zdecydowaæ, czy wyœwietliæ sekcjê "Request ID".
    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
  }
}