# GameInit

[![Unity](https://img.shields.io/badge/Unity-2022.3+-black.svg)](https://unity.com)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE.md)
[![OpenUPM](https://img.shields.io/npm/v/com.natteens.gameinit?label=OpenUPM&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.natteens.gameinit/)
[![GitHub Release](https://img.shields.io/github/v/release/Natteens/com.natteens.gameinit)](https://github.com/Natteens/com.natteens.gameinit/releases)

A Unity package providing a production-ready foundation for new projects — event channels, timers, dependency injection, singleton utilities, animation events, hierarchy tools, and a broad set of extension methods.

---

## Table of Contents

- [Installation](#installation)
- [Event System](#event-system)
- [Timer System](#timer-system)
- [Dependency Injection](#dependency-injection)
- [Singleton Utilities](#singleton-utilities)
- [Animation Events](#animation-events)
- [Hierarchy Tools](#hierarchy-tools)
- [Utilities & Extensions](#utilities--extensions)
- [API Reference](#api-reference)

---

## Installation

### Via OpenUPM (Recommended)

```bash
openupm add com.natteens.gameinit
```

Or add the scoped registry manually in `Packages/manifest.json`:

```json
{
  "scopedRegistries": [
    {
      "name": "package.openupm.com",
      "url": "https://package.openupm.com",
      "scopes": ["com.natteens"]
    }
  ],
  "dependencies": {
    "com.natteens.gameinit": "1.4.2"
  }
}
```

### Via Git URL

1. Open the Package Manager (`Window` → `Package Manager`)
2. Click `+` → `Add package from git URL...`
3. Enter: `https://github.com/Natteens/com.natteens.gameinit.git`

### Via Local Disk

1. Clone the repository
2. In the Package Manager, click `+` → `Add package from disk...`
3. Select the `package.json` file

---

## Event System

A decoupled, ScriptableObject-based event system following the **Observer / Publisher-Subscriber** pattern. All channels are assets — zero MonoBehaviour coupling between producers and consumers.

### Available Channels

| Type | Description |
|------|-------------|
| `VoidEventChannel` | Parameterless events |
| `BoolEventChannel` | Boolean payloads |
| `IntEventChannel` | Integer payloads |
| `FloatEventChannel` | Float payloads |
| `StringEventChannel` | String payloads |
| `Vector2EventChannel` | Vector2 payloads |
| `Vector3EventChannel` | Vector3 payloads |
| `GameEventChannel` | Parameterless with name/description metadata |

Create any channel via `Assets → Create → Scriptable Objects → GameInit → Events → ...`

### Broadcasting Events

```csharp
[SerializeField] private VoidEventChannel onPlayerDied;
[SerializeField] private IntEventChannel onScoreChanged;

void PlayerDeath()
{
    onPlayerDied.RaiseEvent(new VoidEvent());
}

void AddScore(int points)
{
    onScoreChanged.RaiseEvent(points);
}

// GameEventChannel has a convenience overload
[SerializeField] private GameEventChannel onGameStarted;
onGameStarted.RaiseEvent(); // no struct required
```

### Targeted Events

Channels also support directing events to a specific entity by ID, avoiding broadcast overhead:

```csharp
// Send to one entity
onDamageReceived.RaiseEvent(enemy.gameObject, 50);
onDamageReceived.RaiseEvent(enemy.gameObject.GetInstanceID(), 50);

// Subscribe as a specific entity
onDamageReceived.Subscribe(gameObject.GetInstanceID(), HandleDamage);
```

### Subscribing and Unsubscribing

```csharp
void OnEnable()
{
    onPlayerDied.Subscribe(HandlePlayerDeath);
    onScoreChanged.Subscribe(HandleScoreChange);
}

void HandlePlayerDeath(VoidEvent _) { }
void HandleScoreChange(int newScore) { }

void OnDisable()
{
    onPlayerDied.Unsubscribe(HandlePlayerDeath);
    onScoreChanged.Unsubscribe(HandleScoreChange);
}
```

### Replay Last Value

When `replayLastValue` is enabled on a channel asset, new subscribers immediately receive the last raised value:

```csharp
// Subscriber added after the event fires still gets the value
channel.Subscribe(OnValueChanged); // fires immediately if channel.HasValue
```

### Inspector-Driven Listeners

Add a listener component to any GameObject to wire events without code:

```
VoidEventListener, BoolEventListener, IntEventListener,
FloatEventListener, StringEventListener,
Vector2EventListener, Vector3EventListener, GameEventListener
```

Each exposes a `UnityEvent<T>` field in the Inspector.

---

## Timer System

Timers are registered into the Unity Player Loop automatically via `TimerBootstrapper` — no `Update()` required on your MonoBehaviours.

### Timer Types

#### CountdownTimer

Counts from an initial value down to zero.

```csharp
var countdown = new CountdownTimer(10f);
countdown.OnTimerStop += () => Debug.Log("Time's up!");
countdown.Start();

// Reset with same or new duration
countdown.Reset();
countdown.Reset(15f);

bool done = countdown.IsFinished; // true when CurrentTime <= 0
```

#### StopwatchTimer

Counts upward indefinitely. Useful for measuring elapsed time.

```csharp
var stopwatch = new StopwatchTimer();
stopwatch.Start();

float elapsed = stopwatch.CurrentTime;
```

#### FrequencyTimer

Fires `OnTick` N times per second.

```csharp
var timer = new FrequencyTimer(ticksPerSecond: 10);
timer.OnTick += () => Debug.Log("Tick");
timer.Start();

// Change frequency without recreating
timer.Reset(newTicksPerSecond: 5);
```

#### IntervalTimer

A countdown timer that fires `OnInterval` at regular sub-intervals before completing.

```csharp
// Fires every 2 seconds over 10 seconds (5 total intervals)
var timer = new IntervalTimer(totalTime: 10f, intervalSeconds: 2f);
timer.OnInterval += () => Debug.Log("Interval fired");
timer.OnTimerStop += () => Debug.Log("Complete");
timer.Start();
```

### Common API (All Timers)

```csharp
timer.Start();
timer.Stop();
timer.Pause();
timer.Resume();
timer.Reset();

bool running  = timer.IsRunning;
bool finished = timer.IsFinished;
float time    = timer.CurrentTime;
float progress = timer.Progress; // 0–1, clamped (based on initial duration)
```

Timers implement `IDisposable`. Call `timer.Dispose()` or use `using` when done.

---

## Dependency Injection

A lightweight reflection-based DI system for Unity. An `Injector` component scans the scene on `Awake` (execution order `-1000`), collects all `IDependencyProvider` MonoBehaviours, and injects into all `[Inject]`-marked fields, properties, and methods.

### Setup

```csharp
// 1. Provider — exposes dependencies
public class ServiceProvider : MonoBehaviour, IDependencyProvider
{
    [Provide]
    public AudioService ProvideAudio() => new AudioService();
}

// 2. Consumer — declares dependencies
public class Player : MonoBehaviour
{
    [Inject] private AudioService audio;

    [Inject]
    public void Init(AudioService audio)
    {
        this.audio = audio;
    }
}
```

Add an `Injector` GameObject to your scene. All providers and injectables in the scene are resolved automatically.

### Editor Tools

The `Injector` Inspector exposes:
- **Validate Dependencies** — logs any unresolved `[Inject]` fields
- **Clear All Injectable Fields** — nulls all injected values (useful for testing)

A `GameObject → Dependency Injection → Create Injector` menu item creates a pre-configured Injector GameObject.

---

## Singleton Utilities

Three variants covering different persistence and deduplication strategies.

### `Singleton<T>`

Basic singleton. Destroys duplicate instances. Survives scene loads if not parented.

```csharp
public class GameManager : Singleton<GameManager>
{
    protected override void Awake()
    {
        base.Awake();
        // initialization
    }
}

GameManager.Instance.DoSomething();
```

### `PersistentSingleton<T>`

Calls `DontDestroyOnLoad` on the first instance. Destroys any later instance. Optionally unparents itself on Awake (`AutoUnparentOnAwake = true` by default).

```csharp
public class AudioManager : PersistentSingleton<AudioManager> { }
```

### `RegulatorSingleton<T>`

Persistent singleton that, when a duplicate exists, destroys the **older** instance (by `InitializationTime`) instead of the newer one. Useful for scene-reload scenarios where you want the freshest instance to win.

```csharp
public class NetworkManager : RegulatorSingleton<NetworkManager> { }
```

---

## Animation Events

A StateMachineBehaviour-based system that decouples animation event triggers from game logic.

### Components

| Component | Role |
|-----------|------|
| `AnimationEventStateBehaviour` | State Machine Behaviour — fires an event at a normalized time |
| `AnimationEventReceiver` | MonoBehaviour — receives named events and invokes callbacks |
| `AnimationEvent` | Serializable pair of `eventName` + `UnityEvent` |

### Setup

```csharp
// Register handlers at runtime
var receiver = GetComponent<AnimationEventReceiver>();
receiver.AddEvent("Attack", OnAttackHit);
receiver.AddEvent("Land", OnLand);

void OnAttackHit() { /* detect hits, apply damage */ }
void OnLand()      { /* play dust VFX, camera shake */ }
```

Add `AnimationEventStateBehaviour` to the desired Animator state. Set `eventName` to match and `triggerTime` (0–1 normalized) to control when it fires.

Events can also be configured entirely in the Inspector via the `AnimationEventReceiver` list, with no code required.

### Editor Preview

The `AnimationEventStateBehaviour` Inspector includes a **Preview** button. With a GameObject selected, it scrubs the animation to the configured `triggerTime`, supporting both `AnimationClip` and `BlendTree` states.

---

## Hierarchy Tools

### HierarchyHeader

Visual section dividers inside the Unity Hierarchy window.

Add the `HierarchyHeader` component to an empty GameObject. Configure in the Inspector:

- Text color and font style
- Solid background color or gradient
- Text alignment

Create one via `GameObject → Hierarchy → Create Custom Header`.

### Required Field Indicator

Mark serialized fields with `[RequiredField]`:

```csharp
[RequiredField] public Rigidbody rb;
[RequiredField] public AudioClip deathSound;
```

An error icon appears next to the field in the Inspector and next to the GameObject in the Hierarchy when the field is unassigned. The icon updates immediately on change.

### Component Icons

`HierarchyIconDisplay` automatically draws the primary component icon next to each GameObject name in the Hierarchy, making it easier to identify object roles at a glance. Prefab instances are excluded.

---

## Utilities & Extensions

### `WaitFor` — Cached Coroutine Yields

Eliminates per-frame `WaitForSeconds` allocations:

```csharp
yield return WaitFor.Seconds(0.5f);
yield return WaitFor.FixedUpdate;
yield return WaitFor.EndOfFrame;
```

### `VectorMath`

Static helpers for common 3D math operations:

```csharp
float angle   = VectorMath.GetAngle(v1, v2, planeNormal);
float dot     = VectorMath.GetDotProduct(vector, direction);
Vector3 flat  = VectorMath.RemoveDotVector(velocity, Vector3.up);
Vector3 proj  = VectorMath.ProjectPointOntoLine(origin, dir, point);
```

### Extension Methods

**`TransformExtensions`** — `Reset()`, `Children()`, `DestroyChildren()`, `EnableChildren()`, `DisableChildren()`, `ForEveryChild()`, `InRangeOf()`

**`GameObjectExtensions`** — `GetOrAdd<T>()`, `OrNull<T>()`, `DestroyChildren()`, `SetLayersRecursively()`, `SetActive<T>()`, `SetInactive<T>()`, `Path()`, `PathFull()`

**`Vector3Extensions`** — `With()`, `Add()`, `InRangeOf()`, `ComponentDivide()`, `ToVector3()`, `RandomOffset()`, `RandomPointInAnnulus()`, `Quantize()`

**`Vector2Extensions`** — `With()`, `Add()`, `InRangeOf()`, `RandomPointInAnnulus()`

**`ListExtensions`** — `IsNullOrEmpty()`, `Clone()`, `Swap()`, `Shuffle()`, `Filter()`

**`EnumerableExtensions`** — `ForEach()`, `Random()`

**`StringExtensions`** — `IsNullOrWhiteSpace()`, `IsNullOrEmpty()`, `IsBlank()`, `OrEmpty()`, `Shorten()`, `Slice()`, `ConvertToAlphanumeric()`, Rich text helpers (`RichColor()`, `RichBold()`, etc.)

**`ColorExtensions`** — `SetAlpha()`, `Add()`, `Subtract()`, `ToHex()`, `FromHex()`, `Blend()`, `Invert()`

**`RigidbodyExtensions`** — `ChangeDirection()`, `Stop()` (Unity 6 compatible)

**`NumberExtensions`** — `AtLeast()`, `AtMost()`, `IsOdd()`, `IsEven()`, `Approx()`, `PercentageOf()`

**`AsyncOperationExtensions`** / **`TaskExtensions`** — `AsTask()`, `AsCoroutine()`, `AsCompletedTask<T>()`, `Forget()`

**`VisualElementExtensions`** / **`UQueryBuilderExtensions`** — UI Toolkit helpers for creating, styling, and querying elements

**`ReflectionExtensions`** — Type casting, display names, delegate checks, method rebasing

---

## API Reference

### EventChannel\<T\>

```csharp
// Broadcast
void RaiseEvent(T value)
void Subscribe(Action<T> callback)
void Unsubscribe(Action<T> callback)

// Targeted (by entity ID)
void RaiseEvent(int targetId, T value)
void RaiseEvent(GameObject target, T value)
void RaiseEvent(Component target, T value)
void Subscribe(int entityId, Action<T> callback)
void Unsubscribe(int entityId, Action<T> callback)

// State
T LastValue { get; }
bool HasValue { get; }

// Cleanup
void ResetValue()
bool TryGetValue(int entityId, out T value)
```

### Timer (base)

```csharp
void Start()
void Stop()
void Pause()
void Resume()
void Reset()
void Reset(float newTime)  // not available on FrequencyTimer

float CurrentTime { get; }
float Progress    { get; }  // 0–1
bool  IsRunning   { get; }
bool  IsFinished  { get; }

Action OnTimerStart
Action OnTimerStop
```

### FrequencyTimer (additional)

```csharp
int TicksPerSecond { get; }
Action OnTick
void Reset(int newTicksPerSecond)
```

### IntervalTimer (additional)

```csharp
Action OnInterval
```

---

## Namespaces

```csharp
using GameInit.AnimationEvents;
using GameInit.DependencyInjection;
using GameInit.GameEvents.Channels;
using GameInit.GameEvents.EventListeners;
using GameInit.GameEvents;
using GameInit.Hierarchy;
using GameInit.Timers;
using GameInit.Utils;
using GameInit.Utils.Extensions;
```

---

## Contributing

Bug reports and pull requests are welcome on [GitHub](https://github.com/Natteens/com.natteens.gameinit).

- Open an issue with a clear description and reproduction steps
- Include your Unity version and target platform

---

## License

MIT — see [LICENSE.md](LICENSE.md).
