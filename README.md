<div align="center">

# GameInit

**A shelf of small Unity systems for the work every new project repeats.**

Events, timers, scene injection and practical helpers packaged as independent tools rather than one
all-or-nothing framework.

[![Release](https://img.shields.io/github/v/release/Natteens/GameInit?sort=semver&label=release&style=flat-square)](https://github.com/Natteens/GameInit/releases)
[![Package](https://img.shields.io/badge/package-com.natteens.gameinit-555555?style=flat-square)](./package.json)
[![License](https://img.shields.io/github/license/Natteens/GameInit?style=flat-square)](./LICENSE.md)

[Overview](#a-toolbox-not-a-framework) · [Modules](#whats-inside) · [Installation](#installation) · [Documentation](#documentation)

</div>

---

## A toolbox, not a framework

Unity projects often begin with the same quiet infrastructure: an event needs to cross scene
boundaries, a timer needs to survive without a coroutine, or a component needs a dependency that
should not be found through another global lookup.

GameInit collects those pieces in one package, but keeps them separate. There is no mandatory
bootstrap and no project architecture to adopt. Bring in the module that solves the current problem
and leave the rest alone.

## What's Inside

<table>
<tr>
<td width="50%"><strong>Typed event channels</strong><br><sub>ScriptableObject channels for explicit communication between otherwise unrelated systems.</sub></td>
<td width="50%"><strong>PlayerLoop timers</strong><br><sub>Schedule time-based work without attaching coroutine ownership to an arbitrary component.</sub></td>
</tr>
<tr>
<td width="50%"><strong>Scene injection</strong><br><sub>Provide lightweight dependencies inside a scene without turning the package into a container framework.</sub></td>
<td width="50%"><strong>Everyday utilities</strong><br><sub>Focused helpers for common Unity, hierarchy, animation and lifecycle tasks.</sub></td>
</tr>
</table>

The event API was simplified in **v1.5**. Older examples that use targeted events or
`GameEventChannel` describe a previous version and should not be carried into new code.

## Installation

In the Package Manager, choose **Add package from git URL** and paste:

```text
https://github.com/Natteens/GameInit.git
```

Or declare it in `Packages/manifest.json`:

```json
{
  "dependencies": {
    "com.natteens.gameinit": "https://github.com/Natteens/GameInit.git"
  }
}
```

Pin a release tag when the project needs reproducible package resolution.

## Getting Started

GameInit does not ask for a global setup step. Start from the problem you need to solve, then follow
the module guide for its namespace, assets and lifecycle.

A useful rule is to keep these systems at the infrastructure boundary: event channels communicate,
timers schedule and injection supplies dependencies. Gameplay rules should remain in gameplay code.

## Documentation

Module setup and the current API are documented in
[Documentation](./Documentation~/index.md). That reference covers the details intentionally kept out
of this presentation page, including version notes and module-specific usage.

See the [changelog](./CHANGELOG.md) before upgrading an existing project, particularly across the
v1.5 event-channel changes.

## License

MIT. See [LICENSE.md](./LICENSE.md).
