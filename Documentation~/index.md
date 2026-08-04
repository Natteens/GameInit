# GameInit documentation

GameInit is a collection of independent Unity foundations. Projects can use only the modules they need instead of adopting a framework-wide lifecycle.

## Modules

### Event channels

ScriptableObject event channels provide asset-based communication between systems. Typed channels can carry common Unity and C# values, while listener components allow Inspector-driven responses.

Keep publishers and subscribers unaware of each other. Store channel assets with the feature that owns the event contract.

> Targeted entity events and the former `GameEventChannel` API were removed in version 1.5. Use the current typed broadcast channels exposed by the installed package.

### Timers

The timer module provides reusable countdown, stopwatch, frequency and interval timers. Timers are driven through the Unity player loop, so gameplay components do not need a dedicated `Update` method for each timer.

Dispose timers when their owner is destroyed or when they are no longer needed.

### Dependency injection

The lightweight injection module resolves providers and injects marked fields, properties or methods in scene objects. It is intended for small Unity projects that want explicit scene composition without adding a larger container.

Use the validation tools before entering Play Mode to catch missing providers early.

### Singleton utilities

GameInit includes basic, persistent and replacement-oriented singleton base classes. Use them only for services that truly require global ownership; ordinary scene objects should keep normal references.

### Animation events

Animation event helpers connect Animator state timing to named callbacks without placing gameplay logic inside animation clips.

### Hierarchy and Inspector tools

Editor utilities include hierarchy headers, required-field indicators and component icons. They are editor-only and do not add runtime behavior to builds.

### Extensions and helpers

The package also contains focused helpers for transforms, GameObjects, collections, strings, colors, vectors, rigidbodies, tasks, async operations and UI Toolkit.

Import extensions deliberately. Avoid broad namespaces in files where extension-name collisions would make code harder to read.

## Recommended adoption

1. Install the package.
2. Choose one module required by the project.
3. Add its namespace only where it is used.
4. Validate the feature in a small scene before adopting another module.

GameInit is most useful as a toolbox. It should not become an implicit dependency of every script in the project.

## Version notes

The public API changed in version 1.5, particularly around event channels. Older examples that reference targeted events or `GameEventChannel` do not apply to the current package.

The exact public types in the installed source are the source of truth until a generated API reference is added.