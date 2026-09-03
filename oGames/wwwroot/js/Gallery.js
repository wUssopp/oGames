// Używamy IIFE (Immediately Invoked Function Expression), aby zamknąć zmienne w lokalnym zakresie
// i nie zaśmiecać globalnej przestrzeni nazw window.
(() => {

  // ============================================================
  // 1. INICJALIZACJA I POBRANIE ELEMENTÓW DOM
  // ============================================================

  // Pobieramy listę zdjęć przekazaną z widoku Razor (window.GalleryImages)
  const images = window.GalleryImages || [];

  // Jeśli nie ma zdjęć, przerywamy działanie skryptu
  if (!images.length) return;

  // Pobieramy referencje do elementów HTML
  const mainImg = document.getElementById("gMain");        // Główne zdjęcie na stronie szczegółów
  const thumbsWrap = document.getElementById("gThumbs");   // Kontener na miniaturki
  const modalEl = document.getElementById("galleryModal"); // Okno modalne (Bootstrap)
  const lightboxImg = document.getElementById("gLightboxImg"); // Zdjęcie wewnątrz modala
  const counter = document.getElementById("gCounter");     // Licznik zdjęć 

  let currentIndex = 0; // Przechowuje indeks aktualnie wyświetlanego zdjęcia

  // ============================================================
  // 2. FUNKCJE LOGICZNE
  // ============================================================

  // Funkcja podświetlająca aktywną miniaturkę
  function updateActiveThumb(idx) {
    if (!thumbsWrap) return;

    // Usuwamy klasę 'active' ze wszystkich przycisków
    thumbsWrap.querySelectorAll(".g-thumbBtn").forEach(b => b.classList.remove("active"));

    // Znajdujemy przycisk odpowiadający aktualnemu indeksowi i dodajemy 'active'
    const btn = thumbsWrap.querySelector(`.g-thumbBtn[data-index="${idx}"]`);
    if (btn) btn.classList.add("active");
  }

  // Funkcja aktualizująca tekst licznika
  function updateCounterText() {
    if (counter) counter.textContent = `${currentIndex + 1}/${images.length}`;
  }

  // Główna funkcja wyświetlająca zdjęcie
  function showImage(newIndex) {
    // Obliczamy nowy indeks w pętli (tzw. karuzela).
    // Wzór (i + n) % n pozwala obsłużyć ujemne indeksy (cofanie z 0 na ostatni)
    // oraz przekroczenie zakresu (z ostatniego na 0).
    currentIndex = (newIndex + images.length) % images.length;

    const src = images[currentIndex];

    // Podmieniamy źródło obrazka na stronie głównej
    if (mainImg) mainImg.src = src;

    // Podmieniamy źródło obrazka w modalu (jeśli istnieje)
    if (lightboxImg) lightboxImg.src = src;

    // Aktualizujemy stan interfejsu
    updateActiveThumb(currentIndex);
    updateCounterText();
  }

  // Funkcje pomocnicze do nawigacji
  function next() { showImage(currentIndex + 1); }
  function prev() { showImage(currentIndex - 1); }


  // ============================================================
  // 3. OBSŁUGA ZDARZEŃ (EVENT LISTENERS)
  // ============================================================

  // Kliknięcie w miniaturkę 
  if (thumbsWrap) {
    thumbsWrap.addEventListener("click", (e) => {
      // Sprawdzamy, czy kliknięto w element z klasą .g-thumbBtn
      const btn = e.target.closest(".g-thumbBtn");
      if (!btn) return;

      // Pobieramy indeks z atrybutu data-index i wyświetlamy to zdjęcie
      const i = Number(btn.dataset.index || "0");
      showImage(i);
    });
  }

  // Kliknięcie w przyciski Dalej/Wstecz
  document.addEventListener("click", (e) => {
    // Szukamy elementu z atrybutem data-action="next" lub "prev"
    const btn = e.target.closest("[data-action]");
    if (!btn) return;

    const action = btn.getAttribute("data-action");
    if (action === "next") next();
    if (action === "prev") prev();
  });

  // Obsługa klawiatury (Strzałki Lewo/Prawo) - tylko gdy modal jest otwarty
  document.addEventListener("keydown", (e) => {
    // Bootstrap dodaje klasę 'modal-open' do body, gdy modal jest aktywny
    const isModalOpen = document.body.classList.contains("modal-open");
    if (!isModalOpen) return;

    if (e.key === "ArrowRight") next();
    if (e.key === "ArrowLeft") prev();
  });

  // Gdy modal zostanie otwarty (zdarzenie Bootstrapa)
  // Upewniamy się, że zdjęcie w modalu jest zsynchronizowane z tym, co było na stronie
  if (modalEl) {
    modalEl.addEventListener("shown.bs.modal", () => {
      if (lightboxImg) lightboxImg.src = images[currentIndex];
      updateCounterText();
    });
  }

  // 4. START
  // Wyświetlamy pierwsze zdjęcie na starcie
  showImage(0);

})();