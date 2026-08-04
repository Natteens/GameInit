<div align="center">

# GameInit

A practical Unity toolbox for starting projects with less repeated foundation work.

[![Release](https://img.shields.io/github/v/release/Natteens/GameInit?style=flat-square)](https://github.com/Natteens/GameInit/releases)
[![Package](https://img.shields.io/badge/package-com.natteens.gameinit-555555?style=flat-square)](package.json)
[![License](https://img.shields.io/github/license/Natteens/GameInit?style=flat-square)](LICENSE.md)

</div>

GameInit groups small, independent systems commonly needed at the beginning of a Unity project. It is a toolbox rather than a framework: use the modules that fit the game and ignore the rest.

## Included modules

- Typed ScriptableObject event channels.
- PlayerLoop-driven timers.
- Lightweight scene dependency injection.
- Singleton, animation, hierarchy and utility helpers.

## Installation

Add the package through `Window > Package Manager > Add package from git URL`:

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

## Getting started

GameInit does not require a global bootstrap. Install the package, choose the module needed by the current feature and add only its namespace and setup.

The event-channel API changed in version 1.5. Older examples using targeted events or `GameEventChannel` do not apply to the current package.

## Documentation

Current module guidance and version notes are available in [Documentation](Documentation~/index.md).

## License

MIT. See [LICENSE.md](LICENSE.md).