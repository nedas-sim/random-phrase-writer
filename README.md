# random-phrase-writer

Picks two random words and types them wherever your cursor is. Logs every pick to a local SQLite database for stats.

## Requirements

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [PowerToys](https://aka.ms/getPowertoys)

## PowerToys shortcut setup

1. Open PowerToys and go to **Keyboard Manager** → **Remap a shortcut**
2. Click **Add shortcut remapping**
3. Set the shortcut you want (e.g. `Ctrl + Alt + P`)
4. Under **Action**, select **Run Program**
5. Set the path to `run.cmd` inside the folder where you placed this project
6. Set **Start in** to that same folder
7. Set **If running** to `Close the existing instance`
8. Save

On first trigger the app will create `phrases.db` in the project folder and seed the word list. Every subsequent trigger picks two words and types them at the cursor.

## Stats webpage

Open `stats.html` in a Chromium-based browser (Chrome, Edge).

1. Click **Load phrases.db** and navigate to the `phrases.db` file in the project folder
2. The page shows word frequency, top pairs, and recent picks
3. Stats refresh automatically every 2 seconds when the file changes

After picking the file once, the browser remembers it — next time you open the page it resumes on its own without prompting.

> The page requires an internet connection on first load to fetch the sql.js WebAssembly library from the CDN.
