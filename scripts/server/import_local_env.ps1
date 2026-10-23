function Import-LocalEnv {
    param([Parameter(Mandatory = $true)][string]$Path)
    if (-not (Test-Path -LiteralPath $Path)) { return }
    $lineNumber = 0
    foreach ($line in [System.IO.File]::ReadAllLines($Path)) {
        $lineNumber++
        if ($line -match '^\s*(#.*)?$') { continue }
        if ($line -notmatch '^\s*(?:export\s+)?([A-Za-z_][A-Za-z0-9_]*)\s*=\s*(.*)$') {
            throw "Invalid .env assignment at line $lineNumber."
        }
        $name = $Matches[1]
        $value = $Matches[2].Trim()
        if ($value.StartsWith('"') -or $value.StartsWith("'")) {
            $quote = [regex]::Escape($value.Substring(0, 1))
            if ($value -notmatch ('^' + $quote + '(.*?)' + $quote + '\s*(?:#.*)?$')) {
                throw "Invalid .env quoted value at line $lineNumber. Use a single-line value."
            }
            $value = $Matches[1]
        } else {
            $value = ($value -replace '\s+#.*$', '').TrimEnd()
        }
        # Treat values literally: never evaluate shell expressions or print secrets.
        # Explicit nonempty terminal variables take precedence over the file.
        if ([string]::IsNullOrEmpty([Environment]::GetEnvironmentVariable($name, 'Process'))) {
            [Environment]::SetEnvironmentVariable($name, $value, 'Process')
        }
    }
}
