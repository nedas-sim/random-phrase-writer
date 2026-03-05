Add-Type -AssemblyName System.Windows.Forms

$words = @(
    "Leksis",
    "Neksis",
    "Teksis",
    "Meksis",
    "Šmeksis",
    "Seksis",
    "Fleksis",
    "Keksis",
    "Pepsis",
    "Reksis",
    "Epilepsis"
)

$selected = $words | Get-Random -Count 2

$text = "$($selected[0]) $($selected[1])"

Start-Sleep -Milliseconds 50
[System.Windows.Forms.SendKeys]::SendWait($text)
