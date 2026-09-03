// Czekamy, aż cała struktura HTML się załaduje, zanim uruchomimy skrypty
document.addEventListener("DOMContentLoaded", () => {

  // ============================================================
  // 1. OBSŁUGA FILTROWANIA (WYSZUKIWANIA) W TABELACH
  // ============================================================

  // Pobieramy wszystkie pola input, które mają atrybut 'data-table-filter'
  document.querySelectorAll("[data-table-filter]").forEach(input => {

    // Odczytujemy selektor tabeli z atrybutu (np. "#gamesTable")
    const selector = input.getAttribute("data-table-filter");
    const table = document.querySelector(selector);

    // Jeśli tabela nie istnieje, przerywamy działanie dla tego inputa
    if (!table) return;

    // Funkcja pomocnicza: zbiera wszystkie wiersze (tr) ze wszystkich sekcji body (tbody) tabeli
    const getRows = () => Array.from(table.tBodies).flatMap(tb => Array.from(tb.rows));

    // Główna funkcja filtrująca
    const runFilter = () => {
      // Pobieramy wpisany tekst, usuwamy białe znaki z boków i zamieniamy na małe litery
      const q = input.value.trim().toLowerCase();

      // Prelatujemy przez każdy wiersz tabeli
      getRows().forEach(row => {
        // Pobieramy cały tekst z wiersza
        const text = row.innerText.toLowerCase();

        // Jeśli tekst wiersza zawiera szukaną frazę -> pokaż, w przeciwnym razie -> ukryj (display: none)
        row.style.display = text.includes(q) ? "" : "none";
      });
    };

    // Nasłuchujemy wpisywania znaków w polu input
    input.addEventListener("input", runFilter);

    // Uruchamiamy filtr raz na starcie (np. gdy przeglądarka zapamiętała tekst w inpucie po odświeżeniu)
    runFilter();
  });


  // ============================================================
  // 2. OBSŁUGA SORTOWANIA TABEL (DROPDOWN / SELECT)
  // ============================================================

  // Pobieramy wszystkie elementy <select>, które mają atrybut 'data-sort-table'
  document.querySelectorAll("select[data-sort-table]").forEach(sel => {

    // Znajdujemy tabelę, którą ten select ma sortować
    const table = document.querySelector(sel.getAttribute("data-sort-table"));

    // Jeśli tabela nie istnieje lub nie ma sekcji tbody, przerywamy
    if (!table || !table.tBodies.length) return;

    // Sortujemy tylko pierwszy tbody (zazwyczaj tabele mają jeden)
    const tbody = table.tBodies[0];

    // Funkcja pomocnicza: konwertuje wartość na liczbę (obsługuje przecinki jako separatory dziesiętne)
    const num = (v) => {
      // Zamieniamy przecinek na kropkę (np. "12,99" -> "12.99") i parsujemy na float
      const n = parseFloat(String(v ?? "").replace(",", "."));
      // Jeśli wynik jest poprawną liczbą, zwracamy ją, w przeciwnym razie 0
      return Number.isFinite(n) ? n : 0;
    };

    // Główna funkcja sortująca wiersze
    const sortRows = (mode) => {
      // Tworzymy tablicę ze wszystkich wierszy w tbody
      const rows = Array.from(tbody.querySelectorAll("tr"));

      // --- Logika sortowania w zależności od wybranej opcji (value w select) ---

      // 1. Sortowanie domyślne (po ID rosnąco)
      if (mode === "default") {
        rows.sort((a, b) => num(a.dataset.id) - num(b.dataset.id));
      }

      // 2. Cena: Rosnąco
      if (mode === "price-asc") {
        rows.sort((a, b) => num(a.dataset.price) - num(b.dataset.price));
      }

      // 3. Cena: Malejąco (zamieniona kolejność b i a)
      if (mode === "price-desc") {
        rows.sort((a, b) => num(b.dataset.price) - num(a.dataset.price));
      }

      // 4. Alfabetycznie (Tytuł)
      if (mode === "title-asc") {
        rows.sort((a, b) =>
          // localeCompare z opcją "pl" zapewnia poprawne sortowanie polskich znaków (ą, ć, ę...)
          (a.dataset.title || "").localeCompare(b.dataset.title || "", "pl", { sensitivity: "base" })
        );
      }

      // 5. Ocena: Najwyższa (złożone sortowanie)
      if (mode === "rating") {
        rows.sort((a, b) => {
          const rateA = num(a.dataset.rating);
          const rateB = num(b.dataset.rating);

          // Najpierw porównujemy ocenę (wyższa wygrywa)
          if (rateA !== rateB) return rateB - rateA;

          // Jeśli oceny są takie same, wygrywa gra z większą liczbą opinii
          const countA = num(a.dataset.reviews);
          const countB = num(b.dataset.reviews);
          return countB - countA;
        });
      }

      // Fizyczne przeniesienie posortowanych wierszy z powrotem do tabeli
      // (appendChild przenosi element DOM, a nie go kopiuje)
      rows.forEach(r => tbody.appendChild(r));
    };

    // Nasłuchujemy zmiany opcji w select
    sel.addEventListener("change", () => sortRows(sel.value));

    // Uruchamiamy sortowanie na starcie (np. po odświeżeniu strony)
    sortRows(sel.value);
  });
});