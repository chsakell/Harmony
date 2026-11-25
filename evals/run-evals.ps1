# Runs every case in evals/cases/*.json non-interactively in a throwaway git worktree and checks the result.
# Local run: ./evals/run-evals.ps1 -Agent copilot -Filter spec-*

param(
    [ValidateSet('copilot', 'claude')]
    [string]$Agent = $(if ($env:EVAL_AGENT) { $env:EVAL_AGENT } else { 'copilot' }),
    [double]$MinPassRate = 0.8,
    [string]$Filter = '*'
)

# Native commands write progress to stderr; with 'Stop' Windows PowerShell 5.1 would treat that as a failure.
$ErrorActionPreference = 'Continue'
$repo = (git rev-parse --show-toplevel)
$cases = @(Get-ChildItem -LiteralPath (Join-Path $repo 'evals/cases') -Filter "$Filter.json")
if ($cases.Count -eq 0) { Write-Output "No eval cases match '$Filter'."; exit 0 }

function Invoke-Agent([string]$prompt, [string]$logFile) {
    if ($Agent -eq 'copilot') {
        copilot -p $prompt --allow-all-tools --deny-tool 'shell(git push)' --no-ask-user -s *> $logFile
    }
    else {
        claude -p $prompt --permission-mode acceptEdits --allowedTools 'Read,Write,Edit,Glob,Grep,Bash(dotnet:*),Bash(git status:*),Bash(git diff:*),Bash(git log:*)' *> $logFile
    }
}

function Test-Check($check, [string]$work) {
    $path = if ($check.path) { Join-Path $work $check.path } else { $null }
    switch ($check.type) {
        'fileExists' {
            if (-not (Test-Path -LiteralPath $path)) { return "missing $($check.path)" }
        }
        'fileMatches' {
            if (-not (Test-Path -LiteralPath $path)) { return "missing $($check.path)" }
            if ((Get-Content -LiteralPath $path -Raw) -notmatch $check.pattern) { return "$($check.path) does not match /$($check.pattern)/" }
        }
        'fileNotMatches' {
            if ((Test-Path -LiteralPath $path) -and ((Get-Content -LiteralPath $path -Raw) -match $check.pattern)) { return "$($check.path) matches /$($check.pattern)/" }
        }
        'noChanges' {
            $changed = @(git -C $work status --porcelain -- @($check.paths))
            if ($changed.Count -gt 0) { return "unexpected changes: $($changed -join ', ')" }
        }
        'command' {
            Push-Location $work
            try {
                pwsh -NoProfile -Command $check.run *> $null
                if ($LASTEXITCODE -ne 0) { return "command failed: $($check.run)" }
            }
            finally { Pop-Location }
        }
        default { return "unknown check type '$($check.type)'" }
    }
    $null
}

$results = foreach ($caseFile in $cases) {
    $case = Get-Content -LiteralPath $caseFile.FullName -Raw | ConvertFrom-Json
    $work = Join-Path ([System.IO.Path]::GetTempPath()) ("eval-{0}-{1}" -f $case.name, [guid]::NewGuid().ToString('N').Substring(0, 8))
    git -C $repo worktree add --detach $work HEAD *> $null
    if ($LASTEXITCODE -ne 0) { throw "Could not create a worktree for case '$($case.name)'." }
    try {
        foreach ($s in @($case.setup)) {
            if ($s) { Copy-Item -Recurse -Force (Join-Path $repo $s.from) (Join-Path $work $s.to) }
        }
        if ($case.branch) { git -C $work switch -q -c $case.branch }
        git -C $work add -A
        git -C $work -c user.name=eval -c user.email=eval@example.invalid commit -q --allow-empty -m "eval setup"

        Push-Location $work
        try { Invoke-Agent $case.prompt (Join-Path ([System.IO.Path]::GetTempPath()) "$($case.name).log") }
        finally { Pop-Location }

        $failures = @(@(foreach ($check in $case.checks) { Test-Check $check $work }) | Where-Object { $_ })
        [pscustomobject]@{ Case = $case.name; Passed = ($failures.Count -eq 0); Details = ($failures -join '; ') }
    }
    finally {
        git -C $repo worktree remove --force $work *> $null
        if ($case.branch) { git -C $repo branch -D $case.branch *> $null }
    }
}

$results | Format-Table -AutoSize | Out-String | Write-Output
$passed = @($results | Where-Object Passed).Count
$rate = $passed / $results.Count
Write-Output ("Pass rate: {0}/{1} ({2:P0}), minimum {3:P0}" -f $passed, $results.Count, $rate, $MinPassRate)

if ($env:GITHUB_STEP_SUMMARY) {
    $lines = @('## Agent evals', '', "Agent: $Agent", '', '| Case | Result | Details |', '|---|---|---|')
    $lines += $results | ForEach-Object { "| $($_.Case) | $(if ($_.Passed) { 'pass' } else { 'fail' }) | $($_.Details) |" }
    $lines += '', ("Pass rate: {0:P0} (minimum {1:P0})" -f $rate, $MinPassRate)
    $lines | Out-File -FilePath $env:GITHUB_STEP_SUMMARY -Append -Encoding utf8
}

if ($rate -lt $MinPassRate) { exit 1 }
exit 0
