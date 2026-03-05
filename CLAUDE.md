# random-phrase-writer

## What this project is

A Windows utility that types a random two-word phrase wherever the cursor is. Triggered by a PowerToys keyboard shortcut → `run.cmd` → `random-phrase-writer.cs`. Fire-and-forget: the process runs, types, and exits.

## File overview

| File | Purpose |
|---|---|
| `random-phrase-writer.cs` | Main app — picks words, logs to SQLite, types the phrase |
| `run.cmd` | Entry point called by PowerToys: `cd`s to project dir, then `dotnet run random-phrase-writer.cs` |
| `stats.html` | Browser-based stats viewer, reads `phrases.db` directly via sql.js (WebAssembly) |
| `phrases.db` | SQLite database, created on first run, gitignored |

## Key technical decisions

### .NET 10 file-based app
`random-phrase-writer.cs` uses no `.csproj`. It runs via `dotnet run random-phrase-writer.cs` — a .NET 10 feature. NuGet packages are declared with `#:package` directives at the top of the file. Compilation is cached so subsequent runs are fast.

### Keystroke injection
Uses `SendInput` (user32.dll P/Invoke) with `KEYEVENTF_UNICODE` flag. This correctly handles Unicode characters (e.g. `Š`). Do not switch to `SendKeys` — it doesn't handle Unicode reliably.

### 50ms delay before typing
`await Task.Delay(50)` is intentional. It gives the hotkey time to release so the trigger key combination isn't included in the typed output.

### Working directory = project folder
`run.cmd` does `cd /d <project dir>` before invoking dotnet. This ensures `Directory.GetCurrentDirectory()` in the script always resolves to the project folder, so `phrases.db` lands next to the source files.

### Type declarations at the bottom
C# top-level programs require all type declarations (`struct`, `class`) to come after top-level statements. The structs and static classes are at the bottom of the file for this reason.

## SQLite schema

```sql
CREATE TABLE words (
    id   INTEGER PRIMARY KEY,
    text TEXT NOT NULL
);

CREATE TABLE picks (
    id        INTEGER PRIMARY KEY,
    word1_id  INTEGER NOT NULL,
    word2_id  INTEGER NOT NULL,
    picked_at TEXT NOT NULL  -- ISO 8601 UTC
);
```

Words are seeded once on first run if the `words` table is empty. To add/remove words, edit the `words` table directly in the DB.

## Stats page

`stats.html` is a plain HTML file with no build step. It uses:
- **File System Access API** (`showOpenFilePicker`) — user picks `phrases.db` once, handle is persisted in IndexedDB and reused on subsequent loads
- **sql.js** loaded from CDN — runs SQLite queries in the browser via WebAssembly
- Polls every 2 seconds, re-renders only when `file.lastModified` changes

Requires a Chromium-based browser (Chrome, Edge). Does not work in Firefox (File System Access API not supported).
