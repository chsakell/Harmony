# Required status check "SDLC gates" for pull requests into develop, main and release branches.
# Local run: ./.github/scripts/sdlc-gates.ps1 -Branch feature/4711-x -Base develop -Title "AB#4711 x" -Labels sdlc:intent-approved

param(
    [string]$Branch = $env:HEAD_REF,
    [string]$Base = $env:BASE_REF,
    [string]$Title = $env:PR_TITLE,
    [string]$Body = $env:PR_BODY,
    [string]$PrNumber = $env:PR_NUMBER,
    [string]$BaseSha = $env:BASE_SHA,
    [string]$HeadSha = $env:HEAD_SHA,
    [string[]]$Labels = @(($env:PR_LABELS -split ',') | Where-Object { $_ }),
    [switch]$Draft = ($env:PR_DRAFT -eq 'true'),
    [string]$Root = '.'
)

$ErrorActionPreference = 'Stop'
$Labels = @($Labels | ForEach-Object { $_ -split ',' } | ForEach-Object { $_.Trim() } | Where-Object { $_ })
$errors = New-Object System.Collections.Generic.List[string]
$gateErrors = New-Object System.Collections.Generic.List[string]
$notes = New-Object System.Collections.Generic.List[string]

function Get-Status([string]$file) {
    $m = [regex]::Match((Get-Content -LiteralPath $file -Raw), '(?s)^\s*---\s*\r?\n(.*?)\r?\n---')
    if (-not $m.Success) { return '' }
    $s = [regex]::Match($m.Groups[1].Value, '(?m)^status:\s*([A-Za-z-]+)')
    if ($s.Success) { $s.Groups[1].Value.ToLowerInvariant() } else { '' }
}

$trackS = $Labels -contains 'sdlc:track-s'

if ("$Title`n$Body" -notmatch '(^|[^\w])#\d+') {
    $errors.Add('Reference the GitHub issue as #<id> in the pull request title or description.')
}

$kind = $null; $id = $null; $targets = @()
switch -Regex ($Branch) {
    '^feature/(\d+)-[\w.-]+$' { $kind = 'feature'; $id = $Matches[1]; $targets = @('develop'); break }
    '^bugfix/(\d+)-[\w.-]+$' { $kind = 'bugfix'; $id = $Matches[1]; $targets = @('develop', 'release/*'); break }
    '^hotfix/(\d+)-[\w.-]+$' { $kind = 'hotfix'; $id = $Matches[1]; $targets = @('main'); break }
    '^release/\d+\.\d+\.\d+$' { $kind = 'release'; $targets = @('main'); break }
    '^main$' { $kind = 'back-merge'; $targets = @('develop'); break }
    default {
        $errors.Add("Branch '$Branch' does not follow GitFlow naming: feature/<id>-<slug>, bugfix/<id>-<slug>, hotfix/<id>-<slug> or release/<x.y.z>.")
    }
}

if ($kind -and -not ($targets | Where-Object { $Base -like $_ })) {
    $errors.Add("A $kind branch must target $($targets -join ' or '), not '$Base'.")
}

$changed = @()
if ($BaseSha -and $HeadSha) { $changed = @(git -C $Root diff --name-only "$BaseSha...$HeadSha") }
$codeChanged = @($changed | Where-Object { $_ -match '^(src|tests)/' })

if ($kind -eq 'feature' -and $trackS) {
    $notes.Add('Track S (label sdlc:track-s): no feature folder required.')
}
elseif ($kind -eq 'feature') {
    $featuresDir = Join-Path $Root 'docs/features'
    $folder = $null
    if (Test-Path -LiteralPath $featuresDir) {
        $folder = Get-ChildItem -LiteralPath $featuresDir -Directory | Where-Object { $_.Name -like "$id-*" } | Select-Object -First 1
    }
    if (-not $folder) {
        $gateErrors.Add("No docs/features/$id-<slug>/ folder. Run /sdlc-intent, or add the label sdlc:track-s if this is a small change.")
    }
    else {
        $rel = "docs/features/$($folder.Name)"
        $approved = @{}
        foreach ($artifact in 'intent', 'spec', 'plan') {
            $file = Join-Path $folder.FullName "$artifact.md"
            $label = "sdlc:$artifact-approved"
            $hasFile = Test-Path -LiteralPath $file
            $status = if ($hasFile) { Get-Status $file } else { 'missing' }
            $approved[$artifact] = ($status -eq 'approved') -and ($Labels -contains $label)
            if (-not $hasFile) { $gateErrors.Add("$rel/$artifact.md is missing.") }
            elseif ($status -ne 'approved') { $gateErrors.Add("$rel/$artifact.md has status '$status'; it must be 'approved'.") }
            if ($Labels -notcontains $label) { $gateErrors.Add("Label $label is missing.") }
            $notes.Add("$artifact.md: status '$status', label $(if ($Labels -contains $label) { 'present' } else { 'missing' })")
        }
        if ($codeChanged.Count -gt 0 -and -not $approved['plan']) {
            $errors.Add("Code under src/ or tests/ changed before the plan was approved ($($codeChanged[0]) and $($codeChanged.Count - 1) more). Approve $rel/plan.md first.")
        }
    }
}
elseif (($kind -in @('bugfix', 'hotfix')) -and -not $trackS) {
    $notes.Add('Add the label sdlc:track-s to bug fix pull requests.')
}

if ($Labels -contains 'sdlc:high-risk') {
    $notes.Add('sdlc:high-risk: a tech lead or architect must approve each gate and the pull request.')
}

$audit = @()
if ($PrNumber -and $env:GITHUB_REPOSITORY) {
    try {
        $audit = @(gh api "repos/$($env:GITHUB_REPOSITORY)/issues/$PrNumber/events" --paginate `
                --jq '.[] | select((.event == "labeled" or .event == "unlabeled") and (.label.name | startswith("sdlc:"))) | [.created_at, .event, .label.name, .actor.login] | @tsv')
    }
    catch { $notes.Add("Could not read the label history: $($_.Exception.Message)") }
}

if ($Draft -and $gateErrors.Count -gt 0) {
    foreach ($e in $gateErrors) { $notes.Add("Pending (draft): $e") }
    $gateErrors.Clear()
}
$allErrors = @($errors) + @($gateErrors)

$summary = New-Object System.Text.StringBuilder
[void]$summary.AppendLine('## SDLC gates')
[void]$summary.AppendLine("Branch ``$Branch`` -> ``$Base``$(if ($Draft) { ' (draft)' })")
[void]$summary.AppendLine()
if ($allErrors.Count -eq 0) { [void]$summary.AppendLine('**Result: pass**') } else { [void]$summary.AppendLine('**Result: fail**') }
foreach ($e in $allErrors) { [void]$summary.AppendLine("- :x: $e") }
foreach ($n in $notes) { [void]$summary.AppendLine("- $n") }
if ($audit.Count -gt 0) {
    [void]$summary.AppendLine()
    [void]$summary.AppendLine('### Gate history')
    [void]$summary.AppendLine('| When (UTC) | Event | Label | By |')
    [void]$summary.AppendLine('|---|---|---|---|')
    foreach ($line in $audit) { [void]$summary.AppendLine('| ' + ($line -replace "`t", ' | ') + ' |') }
}

if ($env:GITHUB_STEP_SUMMARY) { $summary.ToString() | Out-File -FilePath $env:GITHUB_STEP_SUMMARY -Append -Encoding utf8 }
Write-Output $summary.ToString()

if ($allErrors.Count -gt 0) { exit 1 }
exit 0
