# PreToolUse guard shared by Claude Code, VS Code and GitHub Copilot CLI (all read .claude/settings.json).
# Exit 2 denies the tool call; any internal error exits 0 because Copilot treats hook crashes as a deny.
# Rules and paths live in sdlc-guard.json next to this script.

$ErrorActionPreference = 'Stop'

function Deny([string]$reason) {
    $json = @{ permissionDecision = 'deny'; permissionDecisionReason = $reason } | ConvertTo-Json -Compress
    [Console]::Out.Write($json)
    [Console]::Error.WriteLine($reason)
    exit 2
}

function ConvertTo-GlobRegex([string]$glob) {
    $g = $glob -replace '\\', '/'
    if ($g -notmatch '/') { $g = "**/$g" }
    $sb = New-Object System.Text.StringBuilder
    $i = 0
    while ($i -lt $g.Length) {
        $c = $g[$i]
        if ($c -eq '*') {
            if ($i + 1 -lt $g.Length -and $g[$i + 1] -eq '*') {
                if ($i + 2 -lt $g.Length -and $g[$i + 2] -eq '/') { [void]$sb.Append('(?:.*/)?'); $i += 3; continue }
                [void]$sb.Append('.*'); $i += 2; continue
            }
            [void]$sb.Append('[^/]*')
        }
        elseif ($c -eq '?') { [void]$sb.Append('[^/]') }
        else { [void]$sb.Append([regex]::Escape([string]$c)) }
        $i++
    }
    '^' + $sb.ToString() + '$'
}

function Test-AnyGlob([string]$path, $globs) {
    foreach ($glob in $globs) {
        if ([regex]::IsMatch($path, (ConvertTo-GlobRegex $glob), 'IgnoreCase')) { return $glob }
    }
    $null
}

function Get-ToolPaths($node) {
    $found = New-Object System.Collections.Generic.List[string]
    $stack = New-Object System.Collections.Stack
    $stack.Push($node)
    while ($stack.Count -gt 0) {
        $n = $stack.Pop()
        if ($null -eq $n) { continue }
        if ($n -is [string]) {
            # apply_patch style payloads name files inside the patch text
            foreach ($m in [regex]::Matches($n, '(?m)^\*\*\* (?:Add|Update|Delete) File: (.+)$')) { $found.Add($m.Groups[1].Value.Trim()) }
            continue
        }
        if ($n -is [System.Collections.IEnumerable]) { foreach ($item in $n) { $stack.Push($item) }; continue }
        foreach ($p in $n.PSObject.Properties) {
            $isPathKey = $p.Name -match '^(file_?path|filePath|notebook_path|target_file|path|paths|files?)$'
            if ($isPathKey -and $p.Value -is [string]) { $found.Add($p.Value) }
            elseif ($isPathKey -and $p.Value -is [System.Collections.IEnumerable]) {
                foreach ($v in $p.Value) { if ($v -is [string]) { $found.Add($v) } else { $stack.Push($v) } }
            }
            else { $stack.Push($p.Value) }
        }
    }
    $found
}

function ConvertTo-RepoPath([string]$path, [string]$root, [string]$cwd) {
    $full = $path
    if (-not [System.IO.Path]::IsPathRooted($path)) { $full = Join-Path $cwd $path }
    $full = ([System.IO.Path]::GetFullPath($full)) -replace '\\', '/'
    $r = ($root -replace '\\', '/').TrimEnd('/')
    if ($full.StartsWith("$r/", [System.StringComparison]::OrdinalIgnoreCase)) { return $full.Substring($r.Length + 1) }
    $full
}

function Get-FrontMatterStatus([string]$file) {
    $text = Get-Content -LiteralPath $file -Raw
    $m = [regex]::Match($text, '(?s)^\s*---\s*\r?\n(.*?)\r?\n---')
    if (-not $m.Success) { return '' }
    $s = [regex]::Match($m.Groups[1].Value, '(?m)^status:\s*([A-Za-z-]+)')
    if ($s.Success) { return $s.Groups[1].Value.ToLowerInvariant() }
    ''
}

try {
    $raw = [Console]::In.ReadToEnd()
    if ([string]::IsNullOrWhiteSpace($raw)) { exit 0 }
    $payload = $raw | ConvertFrom-Json

    $toolName = [string]$payload.tool_name
    if (-not $toolName) { $toolName = [string]$payload.toolName }
    $toolInput = $payload.tool_input
    if ($null -eq $toolInput) { $toolInput = $payload.toolArgs }
    if ($toolInput -is [string]) { try { $toolInput = $toolInput | ConvertFrom-Json } catch { $toolInput = @{ command = $toolInput } } }

    $cwd = [string]$payload.cwd
    if (-not $cwd) { $cwd = (Get-Location).Path }
    $root = $env:CLAUDE_PROJECT_DIR
    if (-not $root) { $root = (git -C $cwd rev-parse --show-toplevel 2>$null) }
    if (-not $root) { $root = $cwd }

    $config = Get-Content -LiteralPath (Join-Path $PSScriptRoot 'sdlc-guard.json') -Raw | ConvertFrom-Json

    $isShell = $toolName -match '(?i)^(bash|powershell)$|terminal|shell'
    $isEdit = (-not $isShell) -and ($toolName -match '(?i)edit|write|create|replace|insert|patch|delete|rename|move')
    $isRead = (-not $isShell) -and ($toolName -match '(?i)^(read|view|grep|glob)$|read_?file|search')

    if ($isShell) {
        $command = [string]$toolInput.command
        foreach ($pattern in $config.blockedCommandPatterns) {
            if ([regex]::IsMatch($command, $pattern, 'IgnoreCase')) {
                Deny "Blocked by the SDLC guard: '$command' is not allowed for agents (force-push or direct push to main/develop). Push your branch and open a pull request instead."
            }
        }
        foreach ($pattern in $config.secretCommandPatterns) {
            if ([regex]::IsMatch($command, $pattern, 'IgnoreCase')) {
                Deny "Blocked by the SDLC guard: the command touches a secrets file or the user-secrets store. Agents must not read secrets; ask a human to provide a non-secret value."
            }
        }
        exit 0
    }

    if (-not ($isEdit -or $isRead)) { exit 0 }

    $paths = @(Get-ToolPaths $toolInput | ForEach-Object { ConvertTo-RepoPath $_ $root $cwd })

    foreach ($path in $paths) {
        $hit = Test-AnyGlob $path $config.secretPaths
        if ($hit) { Deny "Blocked by the SDLC guard: '$path' matches secret pattern '$hit'. Agents must not read or write secrets." }
    }

    if (-not $isEdit) { exit 0 }

    foreach ($path in $paths) {
        $hit = Test-AnyGlob $path $config.protectedPaths
        if ($hit) { Deny "Blocked by the SDLC guard: '$path' is protected (pattern '$hit' in .claude/hooks/sdlc-guard.json). It is generated, managed by a tool, or owned by humans; ask a human to change it." }
    }

    $codePaths = @($paths | Where-Object { Test-AnyGlob $_ $config.planRequiredPaths })
    if ($codePaths.Count -eq 0) { exit 0 }

    $branch = (git -C $root branch --show-current 2>$null)
    if ($branch -notmatch '^feature/(\d+)-') { exit 0 }
    $id = $Matches[1]

    $featuresDir = Join-Path $root 'docs/features'
    if (-not (Test-Path -LiteralPath $featuresDir)) { exit 0 }
    $folder = Get-ChildItem -LiteralPath $featuresDir -Directory | Where-Object { $_.Name -like "$id-*" } | Select-Object -First 1
    # No feature folder means a track-S change; the CI gate checks the sdlc:track-s label.
    if (-not $folder) { exit 0 }

    $plan = Join-Path $folder.FullName 'plan.md'
    $relativePlan = "docs/features/$($folder.Name)/plan.md"
    if (-not (Test-Path -LiteralPath $plan)) {
        Deny "Blocked by the SDLC guard: #$id has no $relativePlan yet. Write the plan with /sdlc-plan and get it approved (label sdlc:plan-approved) before changing '$($codePaths[0])'."
    }
    if ((Get-FrontMatterStatus $plan) -ne 'approved') {
        Deny "Blocked by the SDLC guard: $relativePlan is not approved. Code under src/ and tests/ changes only after the plan has status: approved (label sdlc:plan-approved)."
    }
    exit 0
}
catch {
    [Console]::Error.WriteLine("sdlc-guard: internal error, allowing the action: $($_.Exception.Message)")
    exit 0
}
