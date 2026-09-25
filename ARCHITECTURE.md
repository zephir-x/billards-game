# Billiards Game - Architecture & Game Design

## Architektura i Główne Założenia (Core Principles)
Projekt to symulacja bilarda 2D oparta na środowisku .NET, kładąca nacisk na precyzyjne modelowanie fizyki oraz czystą architekturę. Gra stanowi implementację zasad SOLID, ze szczególnym uwzględnieniem całkowitej separacji matematycznego silnika fizycznego od pętli rysującej (renderingu).

### Zastosowanie zasad SOLID
- **Single Responsibility Principle (SRP):** Każdy moduł (`Physics`, `Core`, `Renderer`) ma tylko jeden powód do zmiany. Fizyka nie wie nic o sterowaniu czy rysowaniu grafik; Renderer odpowiada tylko za wyświetlanie stanu bez wglądu i możliwości mutacji zasad gry.
- **Dependency Inversion Principle (DIP):** Moduły wyższego poziomu (np. logika gry) komunikują się ze sprzętem oraz mechanizmami docelowymi wyłącznie za pomocą interfejsów (np. `IPhysicsEngine`, `IInputProvider`, `IRenderer`), a nie konkretnych implementacji.
- **Separation of Concerns (Rozdział odpowiedzialności):** Stan fizyczny i logika gry (Core/Model) są absolutnie niezależne od platformy rysującej aplikację (Renderer/View).

## Mechanika i Przepływ Gry (Context)
- **Rozgrywka Turowa (Turn-based):** Player 1 oraz Player 2 grają na zmianę. UI ekranu każdorazowo komunikuje dobitnie, czyja jest aktualnie kolej.
- **Local Hotseat:** Gra toczy się na jednym urządzeniu fizycznym (np. gracze przekazują sobie myszkę) - początkowo brak całkowity trybu multiplayer / sieciowego.
- **Menu Główne:** Zaprojektowane ascetycznie na obecny etap iteracji interfejsu; zawiera wyłącznie dwie opcje: `Play` oraz `Exit`.
- **Sterowanie Kijem (Aiming):** Kij połączony jest kinematycznie kursorem. Czubek (tip) kija zawsze znajduje się w stałej odległości od środka punktu białej bili (obracając się wokół niej), natomiast tylny koniec kija (butt) jest śledzony do aktualnej, wektorowej pozycji kursora myszy.
- **Siła i Wykonanie Uderzenia (Shot mechanics):** Gracz przytrzymuje Lewy Przycisk Myszy (LMB), aby narastała siła strzału na pasku. Zwolnienie przycisku decyduje o oddaniu bezpośredniego strzału. W przypadku niezwolnienia przysiku i narastaniu aż do punktu krytycznego (Maximum Power), strzał zostanie poddany zresetowaniu na siłę początkową (0), po czym kontroler będzie na ten sam strzał oczekiwał ponownie rozpoczynając od kliknięcia przycisku.

## Technologie
- **Język:** C# (.NET)
- **Framework (opcjonalny do wpięcia za fasadą):** MonoGame, Raylib-cs bądź SFML.Net do wyświetlania.

---

## Specyfikacja Modułów i Kontrakty Interfejsów

### 1. Silnik Fizyczny (`/Physics`)
Jądro symulacji. Odpowiada za deterministyczne, czysto matematyczne rozwiązywania położeń.

#### Stały Krok Czasowy (Fixed Time Step)
Silnik całkowicie abstrahuje od renderowanego Framerate (FPS). W zamian symulacja "podbija" o stały skok `FixedDeltaTime` co klatkę by nie generować różnic w prędkościach ze względu na wolniejszy lub szybszy sprzęt.

```csharp
public interface IPhysicsEngine
{
    // Oblicza pozycje, ruch, i kolizje dokładnie po minięciu 1 stałego kroku np 16ms
    void Step(float fixedDeltaTime); 
    void AddBody(IPhysicsBody body);
    void RemoveBody(IPhysicsBody body);
    IReadOnlyCollection<IPhysicsBody> GetBodies();
}
```

#### Kontrakty Ciał Fizycznych
Inicjacja kształtów operujących w zderzeniach (Stół to obszary ze współczynnikiem tłumienia oraz bandy boczne, a kule to koła z pełnym Restitution).

```csharp
public interface IPhysicsBody
{
    int Id { get; }
    Vector2 Position { get; set; }
    Vector2 PreviousPosition { get; set; } // Krytyczne do płynnej interpolacji renderingowej
    Vector2 Velocity { get; set; }
    float Mass { get; }
    float Restitution { get; } // Sprężystość zderzenia [0.0, 1.0]
    float Radius { get; }      // Dla punktów stycznych/band wynosi 0
    bool IsStatic { get; }     // Domyślnie bandy/ściany są IsStatic = true
    
    void ApplyImpulse(Vector2 impulse);
}
```

#### Wzory Matematyczne - Tarcie (Friction) i Translacja Czasu
W każdym kroku czasowym prędkość poruszającej się kuli zmniejsza się eksponencjalnie o tłumienie uślizgowe:

1. Zmniejszenie prędkości przez opory statyczne:
$$ v_{new} = v_{current} \cdot (1 - \mu \cdot dt) $$
*(Gdzie $\mu$ jest stałym parametrem dla stołu/sukna).*

2. Zmiana wektora pozycyjnego (Całkowanie wstecznego Eulera):
$$ p_{new} = p_{current} + v_{new} \cdot dt $$

#### Wzory Matematyczne - Kolizje Sprężyste (Elastic Collisions)
Najważniejsza i najtrudniejsza faza cyklu fizycznego. Wymaga generowania obustronnego Impulsu siły (tzw. Resolution Phase), jeżeli ciała kolidują:

1. Wyznaczenie wektora normalnego $\vec{n}$:
$$ \vec{n} = \frac{\vec{p}_A - \vec{p}_B}{||\vec{p}_A - \vec{p}_B||} $$

2. Predykcyjna prędkość względna rozpychającego się układu $\vec{v}_{rel}$:
$$ \vec{v}_{rel} = \vec{v}_A - \vec{v}_B $$

3. Wielkość wektora dociskania ku normalnej (Speed of penetration) $v_n$:
$$ v_n = \vec{v}_{rel} \cdot \vec{n} $$
*(W przypadku $v_n > 0$ oznacza to, iż układ uległ już samo-rozdzieleniu, odrzucamy impuls).*  

4. Parametr skalujący impakt - współczynnik Impulsu $j$ dla doskonałych zderzeń sprężystych opartych o parametr zachowań dynamicznych $e$:
$$ j = \frac{-(1 + e) \cdot v_n}{\frac{1}{m_A} + \frac{1}{m_B}} $$

5. Docelowy skumulowany kierunkowy wektor impulsyjny na styk bil $\vec{J}$:
$$ \vec{J} = j \cdot \vec{n} $$

6. Nanoszenie docelowych sił rozpychających z powrotem na wektory prędkości względem wagi i pędu bil:
$$ \vec{v}_A' = \vec{v}_A + \frac{\vec{J}}{m_A} $$
$$ \vec{v}_B' = \vec{v}_B - \frac{\vec{J}}{m_B} $$

### 2. Logika Gry (`/Core`)
Rozdział koordynacyjny zasady oraz Input od platformy (Adapter Wzorca). Posiada on implementację fasady by koordynować aktywnego Gracza przed odklejeniem logiki.

#### Maszyna Stanów
```csharp
public enum GameState { Menu, PlayerTurn, ChargingShot, SimulatingBalls, GameOver }

public interface IGameManager
{
    GameState CurrentState { get; }
    IPlayer ActivePlayer { get; }
    void UpdateLogic(float deltaTime); // Przekazuje Update w dół
}
```

#### Kontroler Naciągu oraz Kija
Doznania z nacisku kija ukryte są logicznie na tym poziomie:
```csharp
public interface ICueController
{
    float Power { get; } // Skala od 0.0 do 1.0 (Critical max)
    Vector2 CueDirection { get; }
    
    // Ustawienie wektoru wychylenia końca względem kursora i wyśrodkowanej bili
    void UpdateAim(Vector2 cueBallPosition, Vector2 mousePosition);
    
    // Potęgowanie naładunku (jeśli dobije > 1.0 powoduje wewnętrzny Overcharge Reset)
    void ChargeShot(float deltaTime);
    void ResetCharge();
    
    // Konstruuje siłę uderzenia i transmituje ją na interfejs IPhysicsBody białej bili
    void ExecuteShot(IPhysicsBody cueBall);
}

// Zewnętrzne podłączenie zdarzeń fizycznego urządzenia by uniknąć bibliotek graficznych
public interface IInputProvider
{
    Vector2 MouseWorldPosition { get; }
    bool IsLeftMouseDown { get; }
    bool WasLeftMouseReleased { get; }
}
```

### 3. Silnik Renderujący (`/Renderer`)
Otrzymuje z warstw wyższych read-only obiekt wizualizujący sytuację. Nigdy nie zmienia wektora pozycji by uniknąć wyścigów asynchronicznych podczas zderzeń bocznych z biblioteką.

#### Kontrakt Prezentacji (Fasada dla biblioteki GameLoopowej)
```csharp
public interface IRenderer
{
    void Initialize(ISceneParameters sceneData);
    // Alfa to reszta z dzielenia czasu dla super-płynności międzyklatkowej (zazwyczaj przedział [0, 1))
    void DrawFrame(float interpolationAlpha); 
}

public interface ISceneParameters
{
    GameState CurrentState { get; } // Do rysowania Menu lub GUI
    IPlayer CurrentTurnPlayer { get; }
    IReadOnlyCollection<IPhysicsBody> Bodies { get; }
    ICueController CueInfo { get; }
}
```

#### Wzór Interpolacji Wideo
Podczas gdy `/Physics` musi przeliczać uderzenie bili o ścianę sztywno o krok `dt = 0.016s`, gracz może używać monitora 144Hz gdzie klatka pojawia się co `0.007s` zmuszając do wyrenderowania pozycji pomimo braku nowego przeliczenia ukł. matematycznego. Unikamy tutaj lagowania obrazu tzw. *Alpha-blending interpolacją*:
$$ R_{Pos} = (Pos_{previous} \cdot (1 - \alpha)) + (Pos_{current} \cdot \alpha) $$
- **$Pos_{previous}$:** Stan bili w klatce krok wstecz.
- **$Pos_{current}$:** Obliczona nowoposadowana bila z Physics Engine.
- **$\alpha$:** Ułamek czasu jaka minęła odkąd wyrejestrowano ruch do pełnego dystansu klatki fizycznej.
