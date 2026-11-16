<#
.SYNOPSIS
    Validate a converted Codely CLI skill's SKILL.md.

.PARAMETER Path
    Path to the skill directory (must contain SKILL.md).

.EXAMPLE
    .\validate_skill.ps1 -Path ".codely-cli/skills/algorithmic-art"
#>

param(
    [Parameter(Mandatory=$true)]
    [string]$Path
)

$exitCode = 0
$warnings = [System.Collections.ArrayList]::new()
$failures = [System.Collections.ArrayList]::new()

function Add-Failure {
    param([string]$Message)
    [void]$script:failures.Add($Message)
    $script:exitCode = 1
}

function Add-Warning {
    param([string]$Message)
    [void]$script:warnings.Add($Message)
}

# ── 1. SKILL.md exists ──
$skillMdPath = Join-Path $Path 'SKILL.md'
if (-not (Test-Path $skillMdPath)) {
    Write-Host ''
    Write-Host 'Skill Validation Report' -ForegroundColor Cyan
    Write-Host "  Path: $Path"
    Write-Host ''
    Write-Host '  SKILL.md exists     [FAIL]  File not found' -ForegroundColor Red
    Write-Host ''
    Write-Host '  Result: FAIL' -ForegroundColor Red
    exit 1
}

$content = Get-Content $skillMdPath -Raw -Encoding UTF8

# ── 2. Parse frontmatter ──
$hasFrontmatter = $false
$frontmatterRaw = ''
$bodyContent = ''
$frontmatterFields = @{}

if ($content -match '(?s)^---\r?\n(.+?)\r?\n---\r?\n(.*)$') {
    $hasFrontmatter = $true
    $frontmatterRaw = $Matches[1]
    $bodyContent = $Matches[2]

    foreach ($fmLine in ($frontmatterRaw -split '\r?\n')) {
        $trimmed = $fmLine.Trim()
        if ($trimmed -eq '' -or $trimmed.StartsWith('#')) { continue }
        if ($trimmed -match '^([a-zA-Z_-]+)\s*:\s*(.*)$') {
            $key = $Matches[1].Trim()
            $value = $Matches[2].Trim()
            $frontmatterFields[$key] = $value
        }
    }
}
else {
    Add-Failure 'YAML frontmatter not found or malformed. Expected --- delimiters.'
}

# ── 3. Required fields: name ──
if ($hasFrontmatter) {
    if (-not $frontmatterFields.ContainsKey('name') -or $frontmatterFields['name'] -eq '') {
        Add-Failure 'Frontmatter missing required field: name'
    }
    else {
        $nameValue = $frontmatterFields['name']
        if ($nameValue -notmatch '^[a-z0-9]+(-[a-z0-9]+)*$') {
            Add-Warning "Skill name '$nameValue' is not kebab-case (expected lowercase letters, digits, hyphens)"
        }
    }
}

# ── 4. Required fields: description ──
if ($hasFrontmatter) {
    if (-not $frontmatterFields.ContainsKey('description') -or $frontmatterFields['description'] -eq '') {
        Add-Failure 'Frontmatter missing required field: description'
    }
    else {
        $descValue = $frontmatterFields['description']
        if ($descValue.Length -lt 20) {
            Add-Warning "Description is very short ($($descValue.Length) chars). Consider making it more descriptive."
        }
    }
}

# ── 5. Platform-specific content in description ──
$platformHits = [System.Collections.ArrayList]::new()
if ($hasFrontmatter -and $frontmatterFields.ContainsKey('description')) {
    $desc = $frontmatterFields['description']

    $platformPatterns = @(
        @{ Pattern = '\bTRIGGER\s+when\b';                       Platform = 'Claude'; Detail = 'TRIGGER when' }
        @{ Pattern = '\bDO\s+NOT\s+TRIGGER\b';                   Platform = 'Claude'; Detail = 'DO NOT TRIGGER' }
        @{ Pattern = '\bUse\s+the\s+(Bash|Read|Write|Edit|Task|WebSearch)\s+tool\b'; Platform = 'Claude'; Detail = 'Claude tool reference' }
        @{ Pattern = '\b(TodoWrite|TodoRead)\b';                  Platform = 'Claude'; Detail = 'TodoWrite/TodoRead' }
        @{ Pattern = '\bclaude\.ai\b';                            Platform = 'Claude'; Detail = 'claude.ai' }
        @{ Pattern = '\bCLAUDE\.md\b';                            Platform = 'Claude'; Detail = 'CLAUDE.md' }
        @{ Pattern = '\bInvoke\s+with\s+\$';                     Platform = 'Codex';  Detail = 'Invoke with $' }
        @{ Pattern = '\bspawn_agent\b';                           Platform = 'Codex';  Detail = 'spawn_agent' }
        @{ Pattern = '\bAGENTS\.md\b';                            Platform = 'Codex';  Detail = 'AGENTS.md' }
        @{ Pattern = '\.codex/skills/';                           Platform = 'Codex';  Detail = '.codex/skills/' }
        @{ Pattern = '\bPOWER\.md\b';                             Platform = 'Kiro';   Detail = 'POWER.md' }
        @{ Pattern = '\.kiro/powers/';                            Platform = 'Kiro';   Detail = '.kiro/powers/' }
    )

    foreach ($pp in $platformPatterns) {
        if ($desc -match $pp.Pattern) {
            [void]$platformHits.Add("[$($pp.Platform)] $($pp.Detail)")
        }
    }

    if ($platformHits.Count -gt 0) {
        $hitList = $platformHits -join '; '
        Add-Failure "Platform-specific content in description: $hitList. This skill may not work correctly in Codely CLI."
    }
}

# ── 6. No extra frontmatter fields ──
$validFields = @('name', 'description')
$extraFields = [System.Collections.ArrayList]::new()
if ($hasFrontmatter) {
    foreach ($key in $frontmatterFields.Keys) {
        if ($key -notin $validFields) {
            [void]$extraFields.Add($key)
        }
    }
    if ($extraFields.Count -gt 0) {
        $joined = $extraFields -join ', '
        Add-Warning "Extra frontmatter fields found: $joined. Only name and description are standard."
    }
}

# ── 6. Body is non-empty ──
if ($hasFrontmatter) {
    $trimmedBody = $bodyContent.Trim()
    if ($trimmedBody -eq '') {
        Add-Failure 'SKILL.md body is empty. Instructions are required after frontmatter.'
    }
}

# ── Helper: detect lines to skip (code blocks, markdown tables) ──
$contentLines = $content -split '\r?\n'
$inCodeBlock = $false
$skipLine = [bool[]]::new($contentLines.Count)
for ($i = 0; $i -lt $contentLines.Count; $i++) {
    if ($contentLines[$i] -match '^\s*```') {
        $inCodeBlock = -not $inCodeBlock
        $skipLine[$i] = $true
        continue
    }
    if ($inCodeBlock) {
        $skipLine[$i] = $true
        continue
    }
    # Skip markdown table rows
    if ($contentLines[$i].TrimStart().StartsWith('|')) {
        $skipLine[$i] = $true
    }
}

# ── 7. Check for unfilled placeholders ──
$placeholders = [System.Collections.ArrayList]::new()
for ($i = 0; $i -lt $contentLines.Count; $i++) {
    if ($skipLine[$i]) { continue }
    $cLine = $contentLines[$i]
    $pMatches = [regex]::Matches($cLine, '<([A-Z_][A-Z_0-9-]*?)>')
    foreach ($pm in $pMatches) {
        [void]$placeholders.Add("Line $($i + 1): $($pm.Value)")
    }
}
if ($placeholders.Count -gt 0) {
    $pList = $placeholders -join '; '
    Add-Warning "Unfilled placeholders found: $pList"
}

# ── 8. Check for TODO markers ──
$todos = [System.Collections.ArrayList]::new()
for ($i = 0; $i -lt $contentLines.Count; $i++) {
    if ($skipLine[$i]) { continue }
    $cLine = $contentLines[$i]
    if ($cLine -match '\bTODO\b') {
        [void]$todos.Add("Line $($i + 1): $($cLine.Trim())")
    }
}
if ($todos.Count -gt 0) {
    $tList = $todos -join '; '
    Add-Failure "TODO markers found: $tList"
}

# ── 9. Check for # REVIEW flags ──
$reviews = [System.Collections.ArrayList]::new()
for ($i = 0; $i -lt $contentLines.Count; $i++) {
    if ($skipLine[$i]) { continue }
    $cLine = $contentLines[$i]
    if ($cLine -match '#\s*REVIEW') {
        [void]$reviews.Add("Line $($i + 1): $($cLine.Trim())")
    }
}

# ── Report ──
Write-Host ''
Write-Host 'Skill Validation Report' -ForegroundColor Cyan
Write-Host "  Path: $skillMdPath"
Write-Host ''

# Frontmatter
if (-not $hasFrontmatter) {
    Write-Host '  Frontmatter         [FAIL]  Missing or malformed' -ForegroundColor Red
}
else {
    $fmStatus = 'PASS'
    $fmColor = 'Green'
    $fmDetail = "name: $($frontmatterFields['name'])"
    foreach ($f in $failures) {
        if ($f -match 'Frontmatter missing') {
            $fmStatus = 'FAIL'
            $fmColor = 'Red'
            $fmDetail = $f
            break
        }
    }
    Write-Host "  Frontmatter         [$fmStatus]  $fmDetail" -ForegroundColor $fmColor
}

# Platform-specific content
if ($platformHits.Count -gt 0) {
    $phJoined = $platformHits -join '; '
    Write-Host "  Platform Content    [FAIL]  $phJoined" -ForegroundColor Red
}
else {
    Write-Host '  Platform Content    [PASS]  No platform-specific content' -ForegroundColor Green
}

# Extra fields
if ($extraFields.Count -gt 0) {
    $efJoined = $extraFields -join ', '
    Write-Host "  Extra Fields        [WARN]  $efJoined" -ForegroundColor Yellow
}
else {
    Write-Host '  Extra Fields        [PASS]  Only name + description' -ForegroundColor Green
}

# Description
$descStatus = 'PASS'
$descColor = 'Green'
$descDetail = ''
foreach ($f in $failures) {
    if ($f -match 'description') {
        $descStatus = 'FAIL'
        $descColor = 'Red'
        $descDetail = $f
        break
    }
}
if ($descStatus -eq 'PASS') {
    foreach ($w in $warnings) {
        if ($w -match 'Description is very short') {
            $descStatus = 'WARN'
            $descColor = 'Yellow'
            $descDetail = $w
            break
        }
    }
}
if ($descDetail -eq '' -and $frontmatterFields.ContainsKey('description')) {
    $dLen = [Math]::Min(60, $frontmatterFields['description'].Length)
    $descDetail = $frontmatterFields['description'].Substring(0, $dLen) + '...'
}
Write-Host "  Description         [$descStatus]  $descDetail" -ForegroundColor $descColor

# Body
$bodyStatus = 'PASS'
$bodyColor = 'Green'
$bodyDetail = ''
foreach ($f in $failures) {
    if ($f -match 'body is empty') {
        $bodyStatus = 'FAIL'
        $bodyColor = 'Red'
        $bodyDetail = $f
        break
    }
}
foreach ($f in $failures) {
    if ($f -match 'TODO') {
        $bodyStatus = 'FAIL'
        $bodyColor = 'Red'
        $bodyDetail = $f
        break
    }
}
if ($bodyStatus -eq 'PASS' -and $reviews.Count -gt 0) {
    $bodyStatus = 'WARN'
    $bodyColor = 'Yellow'
    $bodyDetail = "$($reviews.Count) REVIEW flag(s) found"
}
if ($bodyDetail -eq '') {
    $bodyDetail = 'Instructions present, no issues'
}
Write-Host "  Body                [$bodyStatus]  $bodyDetail" -ForegroundColor $bodyColor

# Placeholders
if ($placeholders.Count -gt 0) {
    Write-Host "  Placeholders        [WARN]  $($placeholders.Count) unfilled placeholder(s)" -ForegroundColor Yellow
}
else {
    Write-Host '  Placeholders        [PASS]  None found' -ForegroundColor Green
}

# TODOs
if ($todos.Count -gt 0) {
    Write-Host "  TODOs               [FAIL]  $($todos.Count) TODO(s)" -ForegroundColor Red
}
else {
    Write-Host '  TODOs               [PASS]  None' -ForegroundColor Green
}

# REVIEW flags
if ($reviews.Count -gt 0) {
    Write-Host "  REVIEW flags        [WARN]  $($reviews.Count) line(s) need review" -ForegroundColor Yellow
    foreach ($r in $reviews) {
        Write-Host "    $r" -ForegroundColor Yellow
    }
}
else {
    Write-Host '  REVIEW flags        [PASS]  None' -ForegroundColor Green
}

# Overall result
Write-Host ''
if ($failures.Count -gt 0) {
    Write-Host '  Result: FAIL' -ForegroundColor Red
    Write-Host ''
    foreach ($f in $failures) {
        Write-Host "    - $f" -ForegroundColor Red
    }
}
elseif ($warnings.Count -gt 0) {
    Write-Host '  Result: PASS WITH WARNINGS' -ForegroundColor Yellow
    Write-Host ''
    foreach ($w in $warnings) {
        Write-Host "    - $w" -ForegroundColor Yellow
    }
}
else {
    Write-Host '  Result: PASS' -ForegroundColor Green
}

Write-Host ''
exit $exitCode