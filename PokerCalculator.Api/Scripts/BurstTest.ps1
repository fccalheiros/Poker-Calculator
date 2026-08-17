param(
    [string]$BaseUrl = "http://localhost:5113",
    [int]$RequestCount = 80,
    [int]$MaxConcurrency = 40
)

# Uses a runspace pool (threads inside this process) instead of Start-Job (separate
# powershell.exe processes per request) - much lighter and avoids the flakiness of
# spinning up dozens of full processes just to fire an HTTP call.
# Each request gets a random 3-card board (avoiding Ah/Ks, the fixed hero cards) so
# requests don't collide on the equity cache - the point is to exercise admission
# control under real concurrent load, not to measure cache hit speed.

# Windows PowerShell 5.1's Invoke-WebRequest rides on the .NET Framework HTTP stack,
# which defaults to only 2 concurrent connections per host - raise that or most of the
# parallel requests just queue for a connection slot and time out before reaching the API.
[System.Net.ServicePointManager]::DefaultConnectionLimit = [Math]::Max($MaxConcurrency * 2, 100)

$ranks = @('2','3','4','5','6','7','8','9','T','J','Q','K','A')
$suits = @('h','d','s','c')
$excluded = @('Ah','Ks')
$deck = foreach ($r in $ranks) { foreach ($s in $suits) { "$r$s" } }
$deck = $deck | Where-Object { $excluded -notcontains $_ }

$scriptBlock = {
    param($Id, $BaseUrl, $Board)
    $ProgressPreference = 'SilentlyContinue'
    $body = "{`"hero`":`"AhKs`",`"board`":`"$Board`",`"villainRanges`":[[`"TT`",`"AKo`",`"76s`"]]}"
    try {
        $resp = Invoke-WebRequest -Uri "$BaseUrl/api/equity" -Method Post -Body $body -ContentType "application/json" -TimeoutSec 90 -UseBasicParsing
        [PSCustomObject]@{ Id = $Id; Status = [int]$resp.StatusCode; Board = $Board; Error = $null }
    } catch {
        $status = $null
        if ($_.Exception.Response) { $status = [int]$_.Exception.Response.StatusCode }
        [PSCustomObject]@{ Id = $Id; Status = $status; Board = $Board; Error = $_.Exception.Message }
    }
}

$pool = [runspacefactory]::CreateRunspacePool(1, $MaxConcurrency)
$pool.Open()

$handles = 1..$RequestCount | ForEach-Object {
    $board = ($deck | Get-Random -Count 3) -join ''
    $ps = [powershell]::Create()
    $ps.RunspacePool = $pool
    [void]$ps.AddScript($scriptBlock).AddArgument($_).AddArgument($BaseUrl).AddArgument($board)
    [PSCustomObject]@{ PowerShell = $ps; Async = $ps.BeginInvoke() }
}

$results = foreach ($h in $handles) {
    $h.PowerShell.EndInvoke($h.Async)
    $h.PowerShell.Dispose()
}

$pool.Close()
$pool.Dispose()

$results | Group-Object Status | Select-Object Name, Count
Write-Host "Total: $($results.Count) | 503 (busy): $(($results | Where-Object { $_.Status -eq 503 }).Count) | Errors: $(($results | Where-Object { -not $_.Status }).Count)"

$sampleErrors = $results | Where-Object { -not $_.Status } | Select-Object -First 3
if ($sampleErrors) {
    Write-Host "`nSample errors:"
    $sampleErrors | ForEach-Object { Write-Host "  #$($_.Id): $($_.Error)" }
}
