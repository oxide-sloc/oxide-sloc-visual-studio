# oxide-sloc for Visual Studio

A Visual Studio 2022 extension (VSIX) that runs [oxide-sloc](https://github.com/oxide-sloc/oxide-sloc)
code-metrics reports from inside the IDE. It shells out to the `oxide-sloc` binary, so it works with
whatever version you already have installed.

This is the companion repository for the Visual Studio extension. The VS Code extension and the
CMake integration live in the main [oxide-sloc](https://github.com/oxide-sloc/oxide-sloc) repository.

## Features

- **Analyze Solution** - scan the whole solution directory and report.
- **Analyze with oxide-sloc** - right-click a file or project in Solution Explorer to scan just that item.
- **Metrics tool window** - a dockable panel (Tools > Oxide SLOC > Metrics Window) showing code lines,
  comments, blanks, files, complexity, and unit tests from the latest run.
- **Open HTML Report** - open the full report in your browser.
- **Start Web UI** - launch the oxide-sloc web dashboard (http://127.0.0.1:4317) and open it.
- **Gate awareness** - if a run trips a `--fail-*` gate, a message box explains which one.

All commands live under **Tools > Oxide SLOC**. Reports are written to a temp directory, never into
your source tree.

## Requirements

- Visual Studio 2022 (17.x), any edition.
- The `oxide-sloc` executable available. The extension resolves it, in order, from:
  1. **Tools > Options > Oxide SLOC > Binary path**,
  2. the `SLOC_BIN` / `OXIDE_SLOC` environment variables,
  3. `oxide-sloc` on `PATH`.

Install oxide-sloc from https://github.com/oxide-sloc/oxide-sloc.

## Install

Download the `.vsix` from the [latest release](https://github.com/oxide-sloc/oxide-sloc-visualstudio/releases)
(or from the `oxide-sloc-vsix` artifact on a successful [build run](https://github.com/oxide-sloc/oxide-sloc-visualstudio/actions)),
then double-click it, or:

```
"%ProgramFiles%\Microsoft Visual Studio\2022\<edition>\Common7\IDE\VSIXInstaller.exe" OxideSloc.vsix
```

## Options

**Tools > Options > Oxide SLOC**:

| Option | Default | Description |
|--------|---------|-------------|
| Binary path | *(empty)* | Explicit path to `oxide-sloc`. Empty = env vars, then `PATH`. |
| Extra analyze flags | *(empty)* | Space-separated flags appended to `analyze` (e.g. `--per-file --activity-window 90`). |
| Web UI port | `4317` | Port for the web UI started from VS. |

## Exit codes

The analyze commands map oxide-sloc's exit codes to notifications:

| Exit | Meaning |
|------|---------|
| 0 | Success |
| 2 | Warnings gate (`--fail-on-warnings`) |
| 3 | Code lines below threshold (`--fail-below`) |
| 4 | SLOC budget exceeded (`--fail-on-budget`) |
| 5 | Growth exceeded baseline (`--fail-above-baseline`) |
| 6 | Cyclomatic complexity exceeded (`--max-complexity`) |

## Building from source

Requires Visual Studio 2022 with the **Visual Studio extension development** workload (VSSDK).

```
git clone https://github.com/oxide-sloc/oxide-sloc-visualstudio.git
cd oxide-sloc-visualstudio
msbuild OxideSloc.sln -t:Restore -p:Configuration=Release
msbuild OxideSloc.sln -t:Build   -p:Configuration=Release
```

The `.vsix` is produced under `src/OxideSloc/bin/Release/`. Or open `OxideSloc.sln` in Visual Studio
and press **F5** to launch an experimental instance with the extension loaded.

CI builds the VSIX on every push (see [`.github/workflows/build.yml`](.github/workflows/build.yml))
and uploads it as a build artifact.

## Project layout

```
OxideSloc.sln
src/OxideSloc/
  OxideSloc.csproj              VSSDK project (net472, VS 2022)
  source.extension.vsixmanifest VSIX metadata
  OxideSlocPackage.cs           AsyncPackage entry point
  OxideSlocPackage.vsct         Command table (menus, groups, buttons)
  PackageGuids.cs               Shared GUIDs / command IDs
  GeneralOptions.cs             Tools > Options page
  SlocState.cs                  Shared last-run state + change event
  Commands/                     One class per menu command
  Services/                     Binary resolution, --plain parsing, output pane
  ToolWindows/                  Metrics tool window (code-built WPF)
```

## License

AGPL-3.0-or-later. See [LICENSE](LICENSE). Part of the oxide-sloc project by Nima Shafie.
