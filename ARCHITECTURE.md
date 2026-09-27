# Billiards 2D - Architecture & Game Design

## Core Principles
The project is a 2D billiards simulation built in .NET, focusing on precise physics modeling and clean architecture. The game strictly adheres to SOLID principles, ensuring complete separation between the mathematical physics engine and the rendering loop.

### SOLID Principles Application
- **Single Responsibility Principle (SRP):** Each module (`Physics`, `Core`, `Renderer`) has only one reason to change. The physics engine is completely unaware of rendering or input methods; the renderer only displays the state without mutating game rules.
- **Dependency Inversion Principle (DIP):** High-level modules communicate with underlying mechanisms exclusively through interfaces (e.g., `IPhysicsEngine`, `IInputProvider`, `IRenderer`), never through concrete implementations.
- **Separation of Concerns:** The physical state and game logic (Model) are entirely independent of the platform drawing the application (View).

## Game Mechanics & Context
- **Turn-based Gameplay:** Player 1 and Player 2 alternate turns. The UI explicitly communicates the active player's turn.
- **Local Hotseat:** The game is played on a single physical device (players share the mouse). There is no multiplayer/networking logic in this iteration.
- **Main Menu:** Ascetic design containing only two options: `Play` and `Exit`.
- **Aiming (Cue Control):** The cue is kinematically linked to the cursor. The cue tip maintains a fixed distance from the cue ball's center, while the cue butt tracks the mouse cursor's directional vector.
- **Shot Mechanics:** The player holds the Left Mouse Button (LMB) to charge shot power. Releasing the button executes the shot. If the button is held until the power reaches the critical maximum, the shot is aborted (power resets to 0), and the controller waits for a new mouse click to begin aiming again.

## Tech Stack
- **Language:** C# (.NET 9)
- **Framework (hidden behind facades):** MonoGame (preferred), Raylib-cs, or SFML.Net for rendering and input handling.
- **Testing:** xUnit for deterministic physics and game state verification.

---

## Development Roadmap (Execution Phases)

- **Phase 1: Physics Foundations & Scaffolding [Completed]**
  - [x] Solution and project scaffolding (`BilliardsGame.sln`, `BilliardsGame.Physics.csproj`, `BilliardsGame.Physics.Tests.csproj`).
  - [x] Core interfaces: `IPhysicsEngine`, `IPhysicsBody` (Extracted to `BilliardsGame.Interfaces.csproj`).
  - [x] Basic entities: `Ball` (dynamic circle body), `Cushion` (static boundary segment).
  - [x] Unit test suite for initial entity instantiation and impulse response.

- **Phase 2: Physics Implementation & Verification [Completed]**
  - [x] Numerical integration (Backward Euler) and table friction/drag decay.
  - [x] Collision detection & resolution: Ball-to-Ball (circle vs circle) and Ball-to-Cushion (circle vs segment).
  - [x] Bilateral elastic impulse resolution ($j \cdot \vec{n}$).
  - [x] Motion sleep threshold (`AreAllBodiesAtRest`).
  - [x] Comprehensive unit tests for physics determinism and impulse conservation.

- **Phase 3: Game Core & Input [Completed]**
  - [x] Module `/Core` setup (`BilliardsGame.Core.csproj`).
  - [x] Entities & Contracts: `IPlayer`, `Player`.
  - [x] State machine: `IGameManager` (`Menu`, `PlayerTurn`, `ChargingShot`, `SimulatingBalls`, `GameOver`).
  - [x] Turn coordinator (switching turns after all balls stop moving, foul handling stub).
  - [x] Aiming & power charging controller: `ICueController` & `CueController`.
  - [x] Hardware input abstraction: `IInputProvider`.
  - [x] Core logic unit tests testing the state machine, turns, and cue interactions without a rendering context.

- **Phase 4: Rendering & Presentation [Next]**
  - Module `/Renderer` setup (`BilliardsGame.Renderer.csproj`).
  - Rendering facade (`IRenderer`) and Alpha-blending interpolation for arbitrary refresh rates.
  - Minimal UI overlay: Active player turn label, shot power charging bar, Main Menu (`Play`, `Exit`).
  - Integration adapter for the chosen graphics backend.

---

## Module Specifications & Interface Contracts

### 1. Interfaces & Physics Engine (`/Interfaces`, `/Physics`)
The core of the simulation. Handles deterministic mathematical resolution of positions and velocities.

#### Fixed Time Step
The engine abstracts completely from the rendering framerate (FPS). The simulation steps forward by a constant `FixedDeltaTime` to prevent physics inconsistencies across different hardware speeds.

```csharp
public interface IPhysicsEngine
{
    // Calculates positions, movement, and collisions after a fixed time step (e.g., 16ms)
    void Step(float fixedDeltaTime); 
    void AddBody(IPhysicsBody body);
    void RemoveBody(IPhysicsBody body);
    IReadOnlyCollection<IPhysicsBody> GetBodies();
    bool AreAllBodiesAtRest(float sleepVelocityThreshold = 0.001f);
}
```

#### Physics Body Contracts
Defines shapes participating in collisions. Cushions/walls are static bodies; balls are dynamic circles with restitution.

```csharp
public interface IPhysicsBody
{
    int Id { get; }
    Vector2 Position { get; set; }
    Vector2 PreviousPosition { get; set; } // Critical for smooth rendering interpolation
    Vector2 Velocity { get; set; }
    float Mass { get; }
    float Restitution { get; } // Collision elasticity [0.0, 1.0]
    float Radius { get; }      // 0 for static lines/cushions
    bool IsStatic { get; }     // True for static geometry
    
    void ApplyImpulse(Vector2 impulse);
}
```

#### Mathematics - Friction & Integration
In every time step, moving bodies decelerate due to drag:

Static drag application:
$$v_{new} = v_{current} \cdot (1 - \mu \cdot dt)$$
*(Where $\mu$ is the constant friction coefficient of the table cloth).*

Position update (Backward Euler Integration):
$$p_{new} = p_{current} + v_{new} \cdot dt$$

#### Mathematics - Elastic Collisions
The resolution phase generates a bilateral impulse force if bodies overlap:

1. Normal vector $\vec{n}$:
$$\vec{n} = \frac{\vec{p}_A - \vec{p}_B}{\|\vec{p}_A - \vec{p}_B\|}$$

2. Relative velocity $\vec{v}_{rel}$:
$$\vec{v}_{rel} = \vec{v}_A - \vec{v}_B$$

3. Speed of penetration $v_n$:
$$v_n = \vec{v}_{rel} \cdot \vec{n}$$
*(If $v_n > 0$, the bodies are already separating; impulse is aborted).*

4. Impulse scalar $j$ based on restitution $e$:
$$j = \frac{-(1 + e) \cdot v_n}{\frac{1}{m_A} + \frac{1}{m_B}}$$

5. Final impulse vector $\vec{J}$:
$$\vec{J} = j \cdot \vec{n}$$

6. Applying forces to velocities based on mass:
$$\vec{v}_A' = \vec{v}_A + \frac{\vec{J}}{m_A}$$
$$\vec{v}_B' = \vec{v}_B - \frac{\vec{J}}{m_B}$$

---

### 2. Game Logic (`/Core`)
Coordinates game rules and handles platform input via adapter patterns.

#### Player Contract
```csharp
public interface IPlayer
{
    int Id { get; }
    string Name { get; }
}
```

#### State Machine
```csharp
public enum GameState { Menu, PlayerTurn, ChargingShot, SimulatingBalls, GameOver }

public interface IGameManager
{
    GameState CurrentState { get; }
    IPlayer ActivePlayer { get; }
    void UpdateLogic(float deltaTime); // Propagates logic updates
    void StartGame();
    void EndTurn();
}
```

#### Cue Controller
Manages aiming and shot power mechanics:

```csharp
public interface ICueController
{
    float Power { get; } // Scale from 0.0 to 1.0 (Critical max)
    Vector2 CueDirection { get; }
    
    // Sets cue alignment based on mouse and cue ball position
    void UpdateAim(Vector2 cueBallPosition, Vector2 mousePosition);
    
    // Increases power (resets if exceeding 1.0)
    void ChargeShot(float deltaTime);
    void ResetCharge();
    
    // Applies final shot force to the cue ball
    void ExecuteShot(IPhysicsBody cueBall);
}

// Abstracts physical input devices from graphics libraries
public interface IInputProvider
{
    Vector2 MouseWorldPosition { get; }
    bool IsLeftMouseDown { get; }
    bool WasLeftMouseReleased { get; }
}
```

---

### 3. Rendering Engine (`/Renderer`)
Receives a read-only state for visualization. Never mutates position vectors to avoid race conditions and coupling.

#### Presentation Contract
```csharp
public interface IRenderer
{
    void Initialize(ISceneParameters sceneData);
    // Alpha is the time remainder for inter-frame smoothness [0, 1)
    void DrawFrame(float interpolationAlpha); 
}

public interface ISceneParameters
{
    GameState CurrentState { get; } // For rendering UI/Menu context
    IPlayer CurrentTurnPlayer { get; }
    IReadOnlyCollection<IPhysicsBody> Bodies { get; }
    ICueController CueInfo { get; }
}
```

#### Video Interpolation
While `/Physics` calculates at a rigid $dt = 0.016s$, a 144Hz monitor draws frames every $0.007s$. To prevent visual stuttering, we apply Alpha-blending interpolation:

$$R_{Pos} = (Pos_{previous} \cdot (1 - \alpha)) + (Pos_{current} \cdot \alpha)$$

- **$Pos_{previous}$:** Physics state from the previous step.
- **$Pos_{current}$:** The newly calculated physics state.
- **$\alpha$:** The time fraction elapsed since the last physics step.