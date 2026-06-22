# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).
Versions follow major.minor.hotfix (e.g. 1.2.3).

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
