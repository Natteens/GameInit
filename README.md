<div align="center">

# GameInit

**Reusable Unity utilities for events, timers, scene injection and common project work.**

GameInit groups a handful of systems that tend to come up in most Unity projects. Each module can be
used on its own, so adding a timer or an event channel does not force a larger project architecture.

[![Version](https://img.shields.io/github/v/tag/Natteens/GameInit?sort=semver&label=version&style=flat-square)](https://github.com/Natteens/GameInit/tags)
[![Package](https://img.shields.io/badge/package-com.natteens.gameinit-555555?style=flat-square)](./package.json)
[![License](https://img.shields.io/badge/license-MIT-blue?style=flat-square)](./LICENSE.md)

[Overview](#what-gameinit-is) · [Modules](#whats-inside) · [Installation](#installation) · [Documentation](#documentation)

</div>

---

## What GameInit Is

Most Unity projects eventually need some of the same plumbing: an event that can cross scene
boundaries, a timer that should not depend on an arbitrary coroutine owner, or a simple way to pass
a dependency to a component in the current scene.

GameInit keeps those jobs together without turning them into one large framework. There is no
required bootstrap and no architecture you have to build the rest of the game around. Use the
modules you need and leave the others alone.

## What's Inside

<table>
<tr>
<td width="50%"><strong>Typed event channels</strong><br><sub>ScriptableObject channels for communication between systems that should not depend directly on each other.</sub></td>
<td width="50%"><strong>PlayerLoop timers</strong><br><sub>Schedule time-based work without giving an unrelated component ownership of a coroutine.</sub></td>
</tr>
<tr>
<td width="50%"><strong>Scene injection</strong><br><sub>Pass lightweight dependencies inside a scene without introducing a full dependency-injection container.</sub></td>
<td width="50%"><strong>Everyday utilities</strong><br><sub>Small helpers for common Unity, hierarchy, animation and lifecycle tasks.</sub></td>
</tr>
</table>

The event API changed in **v1.5**. Examples that use targeted events or `GameEventChannel` belong to
an older version and should not be used for new code.

## Installation

In Package Manager, choose **Add package from git URL** and paste:

```text
https://github.com/Natteens/GameInit.git
```

Or add it to `Packages/manifest.json`:

```json
{
  "dependencies": {
    "com.natteens.gameinit": "https://github.com/Natteens/GameInit.git"
  }
}
```

For a project that should stay on a known version, append a release tag to the Git URL.

## Getting Started

There is no global setup step. Start with the module you need, then follow its guide for the
relevant namespace, assets and lifecycle.

As a rule, keep these modules focused on infrastructure: event channels communicate, timers
schedule work and scene injection supplies dependencies. Gameplay rules stay in gameplay code.

## Documentation

Setup, API notes and module-specific examples live in
[Documentation](./Documentation~/index.md).

Check the [changelog](./CHANGELOG.md) before upgrading an existing project, especially when moving
to v1.5 or newer because of the event-channel changes.

## License

MIT. See [LICENSE.md](./LICENSE.md).
