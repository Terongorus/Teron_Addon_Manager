# Teron Addon Manager

A Windows desktop addon manager for **Elder Scrolls Online**, inspired by [GitAddonsManager](https://gitlab.com/woblight/GitAddonsManager). Add an addon by pasting a link to it, and the app downloads, extracts, and tracks it for future updates — no manual unzipping into `Documents\Elder Scrolls Online\...\AddOns\`.

It currently focuses on [ESOUI](https://www.esoui.com/), with GitHub Releases and a generic direct-download fallback also supported as addon sources.

## Features

- **Modern Windows 11 UI** — built on WPF's Fluent theme, with a System/Light/Dark picker in the toolbar that applies instantly across the main window and every dialog.
- **Add an addon by URL** — paste a link to its ESOUI page, its GitHub repo, or a direct archive link, and the app resolves the real download, downloads it, and extracts it into the right `AddOns` folder.
- **Live and PTR support** — auto-detects whether you have a Live and/or PTR installation (`Documents\Elder Scrolls Online\<live|ptr>\AddOns\`) and lists only the ones that actually exist on the left, like switching tabs. The app never creates or deletes these folders itself — it only manages addons within installs the game has already set up.
- **Update tracking** — installed addons are checked against their source automatically on startup and on demand, using a content-hash comparison where the source provides one (ESOUI), falling back to version-string comparison otherwise (GitHub, direct links). The Status column is color-coded (with separate light/dark palettes) so anything needing attention stands out.
- **Searchable, sortable addon list** — filter the installed-addon list by name as you type, and click any column header to sort ascending, descending, or back to the original order.
- **Local addon detection** — addons you installed by hand before ever using this tool are detected by matching their folder name against ESOUI's addon catalog, with a review dialog to choose which ones to start tracking. Runs automatically on startup and on demand via "Scan for Local Addons...".
- **ESOUI marketplace browser** — browse ESOUI's full addon catalog from inside the app ("Browse Addons..."), with a live search box, a category filter, and sorting by name (A-Z or Z-A), last updated, or downloads. Each result has its own Install button that becomes Uninstall once it's tracked, so you can install or remove addons one at a time without the browser closing.
- **Addon details** — right-click any addon (installed or in the marketplace) for a details view: version, source, content hash, install folders, a clickable link back to its page, and (for ESOUI addons) the full "Addon Info" description rendered with real headings, lists, and code blocks rather than flattened text. Images can be clicked to enlarge, and the whole dialog reflows when resized or maximized.
- **Bulk actions** — multi-select with Ctrl+click, Shift+click, or Ctrl+A, then Update, Remove, or Check for Updates on the whole selection at once.
- **Remembers your setup** — theme, addon-list sort, window positions/sizes and maximized state, and the selected game version all persist across restarts.

## Requirements

- Windows, with the [.NET 10 desktop runtime](https://dotnet.microsoft.com/download/dotnet/10.0) (or build from source with the .NET 10 SDK).
- Elder Scrolls Online installed, with its default Documents-folder save location (the game doesn't allow relocating the `AddOns` folder, so neither does this tool).
- If you have **Controlled Folder Access** (Windows Security → Virus & threat protection → Ransomware protection) enabled, you'll need to allow `Teron_Addon_Manager.exe` through it — by default it blocks any app, including this one, from writing to your Documents folder.

## Getting started

1. Build or download `Teron_Addon_Manager.exe` and run it.
2. Pick **ESO Live** or **ESO PTR** from the list on the left, depending on which install you want to manage.
3. Add addons either by:
   - **Manual Add Addon...** — paste a link to an ESOUI page (e.g. `https://www.esoui.com/downloads/info4598-AnimatedActionBar.html`), a GitHub repo, or a direct `.zip` link.
   - **Browse Addons...** — search ESOUI's catalog and install directly.
4. If you already had addons installed manually, the app will offer to adopt them into tracking the first time it scans your `AddOns` folder.
5. Use **Check for Updates**, **Update Selected**, and **Remove Selected** to manage what's tracked.

Installed-addon metadata is stored in `%AppData%\Teron_Addon_Manager\addons.json`; nothing outside that file and the game's own `AddOns` folder is touched.

## Building from source

```sh
dotnet build
dotnet run
```

The project is a standard .NET WPF app (`net10.0-windows`) with no third-party NuGet dependencies — everything is built on the .NET base class library and desktop runtime (`HttpClient`, `System.IO.Compression`, `System.Text.Json`, WPF's built-in Fluent theme).

## Project status

Version 2.5.0. See [CHANGELOG.md](CHANGELOG.md) for the full history. Versions follow `major.minor.hotfix`.

## License

[GNU General Public License v3.0](LICENSE.txt).
