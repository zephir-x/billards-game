# Billiards 2D - Architecture & Game Design

## Core Principles
The project is a 2D billiards simulation built in .NET, focusing on precise physics modeling and clean architecture. The game strictly adheres to SOLID principles, ensuring complete separation between the mathematical physics engine and the rendering loop.

### SOLID Principles Application
- **Single Responsibility Principle (SRP):** Each module (`Physics`, `Core`, `Renderer`, `App`) has only one reason to change. The physics engine is completely unaware of rendering or input methods; the renderer only displays the state without mutating game rules.
- **Dependency Inversion Principle (DIP):** High-level modules communicate with underlying mechanisms exclusively through interfaces (e.g., `IPhysicsEngine`, `IInputProvider`, `IRenderer`), never through concrete implementations.
- **Separation of Concerns:** The physical state and game logic (Model) are entirely independent of the platform drawing the application (View).

## Game Mechanics & Context
- **Turn-based Gameplay:** Player 1 and Player 2 alternate turns. The UI explicitly communicates the active player's turn.
- **Local Hotseat:** The game is played on a single physical device (players share the mouse). There is no multiplayer/networking logic in this iteration.
- **Main Menu:** Ascetic design containing only two options: `Play` and `Exit`.
- **Aiming (Cue Control):** The cue is kinematically linked to the cursor. The cue tip maintains a fixed distance from the cue ball's center, while the cue butt tracks the mouse cursor's directional vector.
- **Shot Mechanics (Overheat System):** The player holds the Left Mouse Button (LMB) to charge shot power, which is visualized by a dedicated Power Bar located outside the billiard table. Releasing the button before reaching the maximum executes the shot. However, if the power reaches the critical maximum (1.0), the cue becomes **Overheated/Blocked**, and a shot can no longer be executed. The power bar will then slowly drain back to 0.0. Only when it completely resets to zero does the cue become available for aiming and shooting again.

## Tech Stack
- **Language:** C# (.NET 9)
- **Framework (hidden behind facades):** Raylib-cs for hardware bindings, rendering, and input tracking.
- **Testing:** xUnit and Moq for deterministic physics and game state verification without graphical context.

---

## Development Roadmap (Execution Phases)

- **Phase 1: Physics Foundations & Scaffolding [Completed]**
  - [x] Solution and project scaffolding (`BilliardsGame.sln`, `BilliardsGame.Physics.csproj`, `BilliardsGame.Physics.Tests.csproj`).
  - [x] Core interfaces: `IPhysicsEngine`, `IPhysicsBody` (Extracted to `BilliardsGame.Interfaces.csproj`).
  - [x] Basic entities: `Ball` (dynamic circle body), `Cushion` (static boundary segment).
  - [x] Unit test suite for initial entity instantiation and impulse response.

- **Phase 2: Physics Implementation & Verification [Completed]**
  - [x] Numerical integration (Backward Euler) and table friction/drag decay.
  - [x] Collision detection & resolution: Ball-to-Ball (circle vs circle) and Ball-to-Cushion (circle vs segment) via polimorphic traits `ICircleBody`, `ISegmentBody`.
  - [x] Bilateral elastic impulse resolution ($j \cdot \vec{n}$).
  - [x] Motion sleep threshold (`AreAllBodiesAtRest`).
  - [x] Comprehensive unit tests for physics determinism and impulse conservation (Tested on generic `IEnumerable`).

- **Phase 3: Game Core & Input [Completed]**
  - [x] Module `/Core` setup (`BilliardsGame.Core.csproj`).
  - [x] Entities & Contracts: `IPlayer`, `Player`.
  - [x] State machine: `IGameManager` (`Menu`, `PlayerTurn`, `ChargingShot`, `SimulatingBalls`, `GameOver`).
  - [x] Turn coordinator (switching turns after all balls stop moving, foul handling stub).
  - [x] Aiming & power charging controller: `ICueController` & `CueController`.
  - [x] Hardware input abstraction: `IInputProvider`.
  - [x] Core logic unit tests testing the state machine, turns, and cue interactions without a rendering context (using Moq).

- **Phase 4: Rendering & Presentation [Completed]**
  - [x] Module `/App` setup (`BilliardsGame.App.csproj`).
  - [x] Rendering DIP contracts (`IRenderer`, `ISceneParameters`).
  - [x] Frame drawing facade and Alpha-blending interpolation for mapping physics updates across varying hardware refresh rates.
  - [x] Object drawing: Minimal UI overlay, dynamically targeted aiming mechanics.
  - [x] Integration adapter implemented via Raylib-cs mappings and fixed x64 emulation configurations.

- **Phase 5: Physics Scale & Game Feel [Completed]**
  - [x] Fine-tune visual and physical scales: Adjust ball radii, cue rendering thickness, and window projections to maintain readable proportions on high-res displays.
  - [x] Recalibrate shot power: Drastically increase the fundamental `ExecuteShot` impulse multiplier or tweak entity mass so balls correctly traverse the table with momentum.
  - [x] Table proportions: Overhaul `Program.cs` instantiation to form a clean, perfectly aligned inner rectangular playfield without messy segment overlaps.

- **Phase 6: Core Mechanics (UI Pockets & Overheat) [Completed]**
  - [x] Engine integration: Introduce `IPocket` / `Hole` geometric entities explicitly to `PhysicsEngine` via dependency injection list.
  - [x] Spatial mapping: Place 6 standard pockets systematically around the cushion vertices.
  - [x] Sinking logic: If a ball's center converges with a pocket radius, immediately remove it from rendering and physics resolution (`_bodies`).
  - [x] Overheat mechanic: Modifying `CueController` so that hitting `Power > 1.0` blocks shooting and triggers a slow cooldown drain back to `0.0`.
  - [x] UI Power Bar: Render a dynamic Power Bar (filling up and changing color) completely outside the physical table boundaries.

- **Phase 7: Win States, Logic Bugs & Menus [Completed]**
  - [x] **Urgent Bug Fixes - White Ball Sinking (Scratch/Foul):** Currently, sinking the white ball permanently despawns it, soft-locking the `GameManager` in `SimulatingBalls` or crashing aiming calculations. The engine must track the white ball state and respawn it upon turn end if sank (Foul penalty logic).
  - [x] **Urgent Bug Fixes - Black Ball Sinking (Win Condition):** Sinking the object ball (black ball) currently does nothing but despawn it. The Engine must broadcast a win event, ending the session and jumping the state to `GameOver`.
  - [x] Main Menu Flow: Add a starting splash screen capturing user input for `Play` or `Exit` buttons explicitly blocking game loop start.
  - [x] In-Game Menu overlay: Override `ESC` key handling (preventing the default Raylib abrupt application exit) to raise an in-game pause overlay screen with clickable buttons: `Continue`, `Restart`, and `Exit`.
  - [x] HUD Implementation: Build a Top-Center GUI mapping the current scores, balls left, and formatted `Turn: Player X` tag.

- **Phase 8: Table UI Polish, Visual Feedback & Collision Hitboxes [Completed]**
  - [x] **Hitbox/Visual Synchronization:** Recalibrated rendering alignment explicitly using custom quad geometry mapping over 16 independent jaw buffers preventing physics tunneling while mimicking accurate pool boundaries.
  - [x] **Table Aesthetics:** Thickened rails, inner bevels, custom cutouts per pocket.
  - [x] **Ghost Guide (Aim Line):** Ethereal dashed rendering for cue prediction modeling vectors against physics overlaps.
  - [x] **UI/HUD Overhaul:** Segmented dedicated scoring zones (Player 1 left, Player 2 right) explicitly awaiting Phase 9 Ball entities.

- **Phase 9: 8-Ball Game Mode & Tiers (Full Rack Setup) [Completed]**
  - [x] **Instancing Full Rack:** Foot spot algorithmic deployment covering true-to-life 8-Ball structure generation overlapping arrays preventing zero-lapses.
  - [x] **Entity Differentiation (Solid vs Striped):** `BallType` enums introduced mapped to complex rendering properties distinguishing base colors & stripped patterns. 
  - [x] **Complex Turn Logic (Basic):** Sinking an assigned type continues player turn. Unassigned table assignments dictating suites based on primary sink events.

- **Phase 10: Advanced Adjudication Engine & Testing Framework [Completed]**
  - [x] **A. Physics Snapshot Metadata Hooking:** The physics engine (`PhysicsEngine`) must track and output metadata exactly per stroke (`StrokeData` context block during `SimulatingBalls`):
    - `FirstBallHit`: Which specific ball Id the white ball collided with first.
    - `RailsHit`: How many rails were touched *after* the initial ball-to-ball contact.
    - `SunkBalls`: A chronological queue of pocketed ball Ids resolving simultaneous sinks.
  - [x] **B. Foul Validation System (IRulesEngine):** Decouple dirty rule checking out of `GameManager` creating a dedicated isolated `RuleValidator`. It evaluates stroke data against the table pool state to determine legal strokes:
    - *Open Table Fault:* Hitting the 8-Ball primary when the table assignment is open.
    - *Wrong Tier Fault:* Hitting an opposing stripe/solid explicitly first.
    - *No Rail Fault:* Soft defensive taps preventing game flow (if nothing sunk, ball must eventually hit a rail after strike).
    - *Scratch & Sink Fault:* Sinking the white ball concurrently whilst sinking a correct object ball (should invalidate the continuation logic immediately).
  - [x] **C. Ball-in-Hand Interactive Placement:** Overwrite hardcoded white ball spawn mechanisms with an interactive state `GameState.BallInHand`. After a foul, the player can dynamically ghost-place the white ball with mouse cursor clicking anywhere on the board validated safe (`Vector2` overlaps via safe collision check).
  - [x] **D. Automated Theory Testing Framework (The "How-To-Test" Solution):** To prevent manual QA-attrition reproducing a billion billiard outcomes, `BilliardsGame.Core.Tests` MUST introduce data-driven xUnit `[Theory]` definitions parsing a robust set of edge case matrix states mimicking instantaneous shot outcomes. Mock the `PhysicsEngine.StrokeData` property to return rigged histories and enforce `RuleValidator` behaviors to assert Turn and Foul conditions rapidly.

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

### 3. Rendering Engine (`/App`)
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
