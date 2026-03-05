#:package Microsoft.Data.Sqlite@9.0.3

using Microsoft.Data.Sqlite;
using System.Runtime.InteropServices;

// P/Invoke constants
const uint INPUT_KEYBOARD = 1;
const uint KEYEVENTF_UNICODE = 0x0004;
const uint KEYEVENTF_KEYUP = 0x0002;

// DB setup
var dbPath = Path.Combine(Directory.GetCurrentDirectory(), "phrases.db");
using var conn = new SqliteConnection($"Data Source={dbPath}");
conn.Open();

conn.ExecuteNonQuery("""
    CREATE TABLE IF NOT EXISTS words (
        id   INTEGER PRIMARY KEY,
        text TEXT NOT NULL
    );
    CREATE TABLE IF NOT EXISTS picks (
        id        INTEGER PRIMARY KEY,
        word1_id  INTEGER NOT NULL,
        word2_id  INTEGER NOT NULL,
        picked_at TEXT NOT NULL
    );
""");

// Seed words if empty
var count = (long)conn.ExecuteScalar("SELECT COUNT(*) FROM words")!;
if (count == 0)
{
    string[] words = ["Leksis", "Neksis", "Teksis", "Meksis", "Šmeksis", "Seksis", "Fleksis", "Keksis", "Pepsis", "Reksis", "Epilepsis"];
    foreach (var word in words)
    {
        var cmd = conn.CreateCommand();
        cmd.CommandText = "INSERT INTO words (text) VALUES ($w)";
        cmd.Parameters.AddWithValue("$w", word);
        cmd.ExecuteNonQuery();
    }
}

// Pick 2 distinct random words
var allWords = new List<(long id, string text)>();
using (var cmd = conn.CreateCommand())
{
    cmd.CommandText = "SELECT id, text FROM words ORDER BY RANDOM() LIMIT 2";
    using var reader = cmd.ExecuteReader();
    while (reader.Read())
        allWords.Add((reader.GetInt64(0), reader.GetString(1)));
}

var (id1, w1) = allWords[0];
var (id2, w2) = allWords[1];
var phrase = $"{w1} {w2}";

// Log pick
using (var cmd = conn.CreateCommand())
{
    cmd.CommandText = "INSERT INTO picks (word1_id, word2_id, picked_at) VALUES ($a, $b, $t)";
    cmd.Parameters.AddWithValue("$a", id1);
    cmd.Parameters.AddWithValue("$b", id2);
    cmd.Parameters.AddWithValue("$t", DateTime.UtcNow.ToString("o"));
    cmd.ExecuteNonQuery();
}

// Wait for hotkey to release before typing
await Task.Delay(50);

// Type the phrase via SendInput (Unicode, works with all chars including Š)
var inputs = new List<INPUT>();
foreach (char c in phrase)
{
    inputs.Add(new INPUT { type = INPUT_KEYBOARD, ki = new KEYBDINPUT { wScan = c, dwFlags = KEYEVENTF_UNICODE } });
    inputs.Add(new INPUT { type = INPUT_KEYBOARD, ki = new KEYBDINPUT { wScan = c, dwFlags = KEYEVENTF_UNICODE | KEYEVENTF_KEYUP } });
}
NativeMethods.SendInput((uint)inputs.Count, inputs.ToArray(), Marshal.SizeOf<INPUT>());

// Type declarations must follow top-level statements

[StructLayout(LayoutKind.Sequential)]
struct INPUT { public uint type; public KEYBDINPUT ki; long padding; }

[StructLayout(LayoutKind.Sequential)]
struct KEYBDINPUT { public ushort wVk; public ushort wScan; public uint dwFlags; public uint time; public IntPtr dwExtraInfo; }

static class NativeMethods
{
    [DllImport("user32.dll")]
    public static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);
}

static class SqliteConnectionExtensions
{
    public static void ExecuteNonQuery(this SqliteConnection conn, string sql)
    {
        using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        cmd.ExecuteNonQuery();
    }

    public static object? ExecuteScalar(this SqliteConnection conn, string sql)
    {
        using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        return cmd.ExecuteScalar();
    }
}
