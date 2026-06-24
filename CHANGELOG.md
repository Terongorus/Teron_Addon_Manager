# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).
Versions follow major.minor.hotfix (e.g. 1.2.3).

## [2.4.1] - 2026-06-23

### Changed

- The Marketplace browser no longer closes after installing an addon. Each result row now has its own Install button that installs just that addon and turns into an Uninstall button once it's tracked, replacing the old "select rows, then click one shared Install Selected button that closed the window" flow. A status message confirms each install or removal instead of a popup.

### Fixed

- Window position was being remembered across restarts, but width and height weren't — both the main window and the Marketplace browser always reopened at their default size unless maximized. They're now saved and restored too, including a related fix so that un-maximizing a window that was closed while maximized correctly returns it to its last manually-sized dimensions instead of the application default.

## [2.4.0] - 2026-06-23

### Added

- A live search box in the main window's toolbar filters the installed-addon list by name as you type. It shows "Search" as placeholder text when empty and unfocused (like a browser's address bar), and pressing Escape while it's focused clears it.
- The Marketplace browser's existing search box now behaves the same way: the separate "Search:" label is gone in favor of placeholder text, and Escape clears it while focused.
- Both windows' toolbar controls now widen automatically when the window is maximized instead of leaving a large empty gap: the main window's search box, and the Marketplace's search box, category filter, and sort dropdown all grow proportionally, then return to their normal size when the window is restored.

## [2.3.0] - 2026-06-23

### Added

- The app now remembers your theme choice, the main addon list's column sort, both windows' positions (and whether they were maximized), and the selected game version, restoring all of it the next time you open the app, in a new `%AppData%\Teron_Addon_Manager\uisettings.json` file.
- Marketplace's search/category/sort filters are intentionally left out of what's remembered, so the browser always opens unfiltered; the "Scan for Local Addons" and "Addon Details" dialogs also keep their original default positioning rather than being persisted.

## [2.2.1] - 2026-06-23

### Fixed

- Clicking empty space below the addon list, pressing Escape, or clicking an already-selected row again now all deselect, matching how Windows' own list and Explorer views behave. Previously, once a row was selected there was no way to get back to "nothing selected." The same fix was applied to the Marketplace's results list.

## [2.2.0] - 2026-06-23

### Added

- The main addon list's columns (Name, Installed, Latest, Status, Source) can now be sorted by clicking their headers, cycling through ascending, descending, and the original order, with an arrow indicator showing the current direction.
- The Marketplace's sort dropdown gained a descending "Name (Z-A)" option alongside the existing ascending one.
- "Update Selected" and "Remove Selected" are now disabled whenever nothing is selected, instead of being clickable with nothing to act on.
- When either window is maximized, its list's columns now stretch to fill the extra width instead of leaving it empty; they return to their normal widths when the window isn't maximized.

## [2.1.1] - 2026-06-23

### Fixed

- The addon list's Status badges now use separate color palettes for light and dark mode, so they stay readable in both instead of washing out in dark mode.
- The theme picker's unselected segments (e.g. "Light" and "System" while "Dark" is active) could become unreadable after switching themes a second time — their text color is now recalculated against the theme that's actually active instead of going stale.

## [2.1.0] - 2026-06-23

### Changed

- The main window and the Marketplace browser both got a modernized Windows 11-style visual pass to match the new Fluent look introduced in 2.0.0: card-style panels around the toolbar and lists, softer divider and hover colors, and the toolbar's actions reworked into a single row of icon buttons with tooltips instead of text buttons.
- Added a System/Light/Dark theme picker to the main window's toolbar; choosing one applies immediately and is shared with every dialog opened afterward.

## [2.0.0] - 2026-06-23

### Changed

- **Migrated the entire UI from WinForms to WPF**, adopting .NET's built-in Windows 11 Fluent theme. This is a major rewrite of every window and dialog (main window, Marketplace browser, Add Addon, Addon Details, Scan Results) from WinForms Designer/resx files to XAML with code-behind, plus a new Fluent-styled message box replacing the WinForms one. No third-party packages were added — the Fluent theme and everything else used here ships with the .NET 10 desktop runtime, keeping the project's "base class library only" approach intact.
- The Addon Details dialog's content (descriptions, images, code blocks) now reflows using WPF's native layout instead of the hand-built resize-callback system the WinForms version needed — text and lists use the available width, while images and code blocks keep their natural size, with less code and more reliable behavior than before.

## [1.1.0] - 2026-06-22

### Added

- The "Game Version" list now only shows ESO Live/PTR entries that actually have an AddOns folder on disk, instead of always listing both — and the app no longer creates or deletes any of those folders itself; it only manages addons within installs that already exist.
- The addon list's Status column is now color-coded: green for "Up to date", yellow for "Update available", red for "Check failed", so addons needing attention stand out immediately.
- Addon descriptions render with real structure instead of flattened plain text: bold section headings (from ESOUI's `[SIZE]`/`[B]` formatting), properly indented bullet and numbered lists, and bordered monospace code blocks (`[CODE]`, `[QUOTE]`, `[HIGHLIGHT]`) — much closer to how the addon's page looks on the website itself.
- Description images can be clicked to open an enlarged preview, and the details dialog now reflows when resized or maximized: text, lists, and headings use the extra width, while images and code blocks stay at a natural, readable size instead of stretching to fill the window.

### Fixed

- Long descriptions could occasionally get cut off partway through, or leave extra blank space after the content, depending on the addon — the scrollable area is now always recalculated from the content's actual current size instead of a value that could go stale.
- The description text no longer shows a blinking edit cursor or otherwise looks like an editable field, and the mouse wheel now scrolls correctly while hovering directly over it.
- A handful of addons had bracketed placeholder text in their description (like `[argument]`) silently deleted, because the cleanup step was treating any bracketed word as a BBCode tag instead of checking it against the actual set of tags ESOUI uses.

## [1.0.0] - 2026-06-20

### Changed

- First stable release. Everything from 0.1.0 through 0.3.0 below makes up this release's feature set.

## [0.3.0] - 2026-06-20

### Added

- Addon details now include the addon's full "Addon Info" description from ESOUI, converted from its BBCode formatting into plain text. Inline screenshots render as actual images instead of raw links, left-aligned and borderless to match the surrounding text — click one to open it in an enlarged preview window.

### Fixed

- Long descriptions could get cut off partway through with no way to scroll down to see the rest, because the scrollable area's size wasn't always recalculated once every screenshot and wrapped paragraph had finished sizing itself. The dialog now stays in sync as that content settles, so the full description is always reachable.

## [0.2.2] - 2026-06-20

### Changed

- Renamed the "Add Addon..." toolbar action to "Manual Add Addon..." to distinguish it from installing through the marketplace, and reordered the toolbar so Check for Updates, Update Selected, and Remove Selected come before it.

## [0.2.1] - 2026-06-20

### Added

- Select all addons in either list with Ctrl+A.
- Right-click context menu on each addon: in the main window, View Details, Check for Updates, Update, and Remove; in the marketplace, View Details and Install Selected. "View Details" opens a new dialog with a bold title/subtitle header, a clean two-column field grid, and clickable links for URL fields, instead of a flat block of text.
- Slightly larger default UI font for readability.

### Fixed

- Two tracked addons could end up claiming the same AddOns folder (e.g. if a local-scan match shifted to a different ESOUI listing on a later run, or a marketplace install overwrote a folder another tracked addon already owned). Conflicting adoptions are now skipped and reported instead of silently duplicated; a conflicting marketplace install now replaces the existing tracked entry instead of duplicating it.

## [0.2.0] - 2026-06-20

### Added

- Detect addons already installed manually in the AddOns folder and match them against the ESOUI catalog by folder name, with a review dialog to choose which ones to adopt for update tracking. Runs automatically on startup alongside the update check, and on demand via "Scan for Local Addons...".
- Browse the full ESOUI addon catalog from within the app ("Browse Addons..."), with search by name/author, a category filter, and sort by name, last updated, or downloads. Installing from search results goes through the same install pipeline as "Add Addon".

## [0.1.1] - 2026-06-20

### Fixed

- Removing or updating an addon no longer crashes the app when its files are marked read-only (preserved from the source archive, or set by other addon-management tools) — the read-only attribute is now cleared before deleting, and cleared on copy so addons installed going forward don't pick it back up.
- "Remove Selected" now catches per-addon failures and reports them in a dialog instead of letting an unhandled exception crash the app.

## [0.1.0] - 2026-06-20

### Added

- Add an addon by pasting a web link to its page or archive. Supported sources: ESOUI (via the `api.mmoui.com` file-details API), GitHub Releases, and a generic fallback that treats any other URL as a direct archive download.
- Download and extract an addon's archive into the correct ESO `AddOns` folder, auto-detecting whether **Live** and/or **PTR** installations exist (`Documents\Elder Scrolls Online\<live|ptr>\AddOns\`), with a left-side list to switch between them like tabs.
- Track installed addons (version, content hash, installed folders) in a local library file (`%AppData%\Teron_Addon_Manager\addons.json`) so updates can be detected later.
- Check installed addons against their source for updates, automatically on startup and on demand, using content-hash comparison where available and falling back to version-string comparison.
- Add, update, or remove installed addons from the main window via a toolbar of actions (Add Addon, Check for Updates, Update Selected, Remove Selected, Open AddOns Folder).

### Security

- Sanitize the addon name before using it as a folder name when an archive places files at its root instead of inside a named folder. The name comes from the source's API response, so without sanitizing it, a malicious listing could have supplied a name like `../../../Startup` to write outside the intended folder.
