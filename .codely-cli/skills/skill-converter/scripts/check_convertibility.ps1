<#
.SYNOPSIS
    Check whether a downloaded source folder can be converted to a Codely CLI skill.

.PARAMETER Path
    Path to the downloaded source directory to inspect.

.EXAMPLE
    .\check_convertibility.ps1 -Path C:\tmp\skills\claude-api
#>

param(
    [Parameter(Mandatory=$true)]
    [string]$Path
)

$blockers = [System.Collections.ArrayList]::new()
$warnings = [System.Collections.ArrayList]::new()

function Add-Blocker { param([string]$Msg) [void]$script:blockers.Add($Msg) }
function Add-Warning { param([string]$Msg) [void]$script:warnings.Add($Msg) }

# ── 1. FORMAT CHECK ──────────────────────────────────────────────────

$hasSkillMd = Test-Path (Join-Path $Path 'SKILL.md')
$hasGeminiExt = Test-Path (Join-Path $Path 'gemini-extension.json')

# Check for Gemini CLI extension format (already compatible)
if ($hasGeminiExt) {
    try {
        $manifest = Get-Content (Join-Path $Path 'gemini-extension.json') -Raw | ConvertFrom-Json
        $contextFileName = if ($manifest.contextFileName) { $manifest.contextFileName } else { 'CODELY.md' }
        $hasContextFile = Test-Path (Join-Path $Path $contextFileName)
        
        if ($hasContextFile) {
            Write-Host "  Gemini Format      [PASS]  Already a Gemini CLI extension" -ForegroundColor Green
            Write-Host "  Context File       [PASS]  $contextFileName found" -ForegroundColor Green
            Write-Host ''
            Write-Host '  Result: ALREADY COMPATIBLE' -ForegroundColor Green
            Write-Host ''
            Write-Host '  This is already a Gemini CLI extension. No conversion needed.' -ForegroundColor Green
            Write-Host '  Copy directly to .codely-cli/extensions/' -ForegroundColor Yellow
            exit 0
        }
    } catch {
        # If JSON parsing fails, continue with normal checks
    }
}

# Platform config files (indicate multi-platform skill, NOT blockers)
$platformConfigFiles = @(
    @{ File = '.claude-plugin';      Platform = 'Claude Code' }
    @{ File = 'gemini-extension.json'; Platform = 'Gemini CLI' }
    @{ File = 'agents\openai.yaml';  Platform = 'Codex' }
    @{ File = 'POWER.md';            Platform = 'Kiro' }
)

$detectedPlatforms = [System.Collections.ArrayList]::new()
foreach ($pc in $platformConfigFiles) {
    if (Test-Path (Join-Path $Path $pc.File)) {
        [void]$detectedPlatforms.Add($pc.Platform)
    }
}

if (-not $hasSkillMd) {
    # No SKILL.md - check if it's a known platform-only format
    if ($detectedPlatforms.Count -gt 0) {
        $pJoined = $detectedPlatforms -join ', '
        Add-Blocker "No SKILL.md found. Detected platform-only format: $pJoined. Cannot convert without skill instructions."
    }
    else {
        Add-Blocker 'No SKILL.md found and no recognizable skill format detected.'
    }
}
elseif ($detectedPlatforms.Count -gt 0) {
    # Has SKILL.md + platform configs = multi-platform skill (CONVERTIBLE!)
    $pJoined = $detectedPlatforms -join ', '
    Add-Warning "Multi-platform skill detected: also supports [$pJoined]. Will convert to Codely CLI format."
}

# ── 2. PLATFORM STRUCTURE CHECK (HARD BLOCKERS ONLY) ─────────────────

# Claude slash-commands are Claude-specific runtime
$commandsDir = Join-Path $Path 'commands'
if (Test-Path $commandsDir -PathType Container) {
    $cmdFiles = Get-ChildItem $commandsDir -File -ErrorAction SilentlyContinue
    if ($cmdFiles.Count -gt 0) {
        Add-Blocker 'Contains Claude slash-commands (commands/*.md) — requires Claude runtime for command registration.'
    }
}

# Claude sub-agent definitions require Claude agent runtime
$agentsDir = Join-Path $Path 'agents'
if (Test-Path $agentsDir -PathType Container) {
    $agentMds = Get-ChildItem $agentsDir -Filter '*.md' -ErrorAction SilentlyContinue
    if ($agentMds.Count -gt 0) {
        Add-Blocker 'Contains Claude sub-agent definitions (agents/*.md) — requires Claude agent runtime.'
    }
    # Note: agents/openai.yaml is NOT a blocker - it's just a config file
}

# MCP server configuration requires MCP runtime
$mcpJson = Join-Path $Path 'mcp.json'
if (Test-Path $mcpJson) {
    Add-Blocker 'Contains mcp.json — requires MCP server runtime. Consider using skill-porter for MCP skills.'
}

# ── 3. SDK BINDING CHECK (HARD BLOCKER) ──────────────────────────────

# Collect code files from scripts/ and top-level only (skip references/docs)
$codeFilesToScan = [System.Collections.ArrayList]::new()
$scanRoots = @($Path, (Join-Path $Path 'scripts'))
foreach ($sr in $scanRoots) {
    if (-not (Test-Path $sr -PathType Container)) { continue }
    foreach ($ext in @('*.py', '*.js', '*.ts', '*.mjs', '*.cjs')) {
        $found = Get-ChildItem $sr -Filter $ext -ErrorAction SilentlyContinue
        foreach ($foundFile in $found) {
            [void]$codeFilesToScan.Add($foundFile.FullName)
        }
    }
}

# Check each file for platform SDK imports (HARD BLOCKER)
foreach ($codeFilePath in $codeFilesToScan) {
    $fc = Get-Content $codeFilePath -Raw -ErrorAction SilentlyContinue
    if (-not $fc) { continue }
    $relName = $codeFilePath.Substring($Path.Length).TrimStart('\/')

    # Anthropic SDK - Claude specific
    if ($fc -match '(?m)^\s*(import\s+anthropic|from\s+anthropic\s)') {
        Add-Blocker "SDK binding in ${relName}: imports Anthropic Python SDK (anthropic)."
    }
    if ($fc -match '@anthropic-ai/sdk') {
        Add-Blocker "SDK binding in ${relName}: imports Anthropic JS/TS SDK (@anthropic-ai/sdk)."
    }
    
    # Claude Agent SDK - Claude specific
    if ($fc -match '(?m)^\s*(from\s+claude_agent_sdk|import\s+claude_agent_sdk)') {
        Add-Blocker "SDK binding in ${relName}: imports Claude Agent SDK."
    }
    
    # OpenAI SDK - only block if it's for agent/assistant functionality
    # Note: OpenAI API calls for data fetching (like web search) are OK
    if ($fc -match '(?m)^\s*(from\s+openai\b|import\s+openai\b)') {
        # Check if it's used for assistant/agent functionality vs just API calls
        if ($fc -match 'assistant|Agent|thread|run\s*\(') {
            Add-Warning "OpenAI SDK import in ${relName}: uses OpenAI for AI functionality. May work if just API calls."
        }
    }
}

# ── 4. DESCRIPTION PLATFORM SYNTAX CHECK (WARNING ONLY) ──────────────

if ($hasSkillMd) {
    $skillRaw = Get-Content (Join-Path $Path 'SKILL.md') -Raw -Encoding UTF8
    $descText = ''
    if ($skillRaw -match '(?s)^---\r?\n(.+?)\r?\n---') {
        foreach ($fmLine in ($Matches[1] -split '\r?\n')) {
            if ($fmLine.Trim() -match '^description\s*:\s*(.+)$') {
                $descText = $Matches[1].Trim()
                break
            }
        }
    }

    if ($descText) {
        # Hard blocker patterns - indicate platform-specific behavior
        $blockerPatterns = @(
            @{ Regex = '\bTodoWrite\b'; Tag = '[Claude] TodoWrite tool' }
            @{ Regex = '\bspawn_agent\b'; Tag = '[Codex] spawn_agent' }
        )

        foreach ($bp in $blockerPatterns) {
            if ($descText -match $bp.Regex) {
                Add-Blocker "Description references platform-specific tool: $($bp.Tag)"
            }
        }

        # Warning patterns - indicate platform mentions but may still work
        $warningPatterns = @(
            @{ Regex = '\bTRIGGER\s+when\b'; Tag = '[Claude] TRIGGER when syntax' }
            @{ Regex = '\bDO\s+NOT\s+TRIGGER\b'; Tag = '[Claude] DO NOT TRIGGER syntax' }
            @{ Regex = '\bCLAUDE\.md\b'; Tag = '[Claude] CLAUDE.md reference' }
            @{ Regex = '\bAGENTS\.md\b'; Tag = '[Codex] AGENTS.md reference' }
            @{ Regex = '\bPOWER\.md\b'; Tag = '[Kiro] POWER.md reference' }
        )

        $descWarnings = [System.Collections.ArrayList]::new()
        foreach ($wp in $warningPatterns) {
            if ($descText -match $wp.Regex) {
                [void]$descWarnings.Add($wp.Tag)
            }
        }
        if ($descWarnings.Count -gt 0) {
            $dwJoined = $descWarnings -join '; '
            Add-Warning "Description contains platform-specific syntax: $dwJoined. Will attempt conversion."
        }
    }
}

# ── REPORT ───────────────────────────────────────────────────────────

Write-Host ''
Write-Host 'Convertibility Check' -ForegroundColor Cyan
Write-Host "  Source: $Path"
Write-Host ''

# Format check
if ($hasSkillMd) {
    Write-Host '  Skill Format        [PASS]  SKILL.md found' -ForegroundColor Green
}
else {
    Write-Host '  Skill Format        [FAIL]  No SKILL.md' -ForegroundColor Red
}

# Platform config (info only)
if ($detectedPlatforms.Count -gt 0) {
    $pcJoined = $detectedPlatforms -join ', '
    Write-Host "  Platform Config     [INFO]  Also supports: $pcJoined" -ForegroundColor Yellow
}
else {
    Write-Host '  Platform Config     [INFO]  Single platform (Codely only)' -ForegroundColor Green
}

# Structure blockers
$structBlockers = @($blockers | Where-Object { $_ -match 'commands/|agents/|mcp\.json' })
if ($structBlockers.Count -gt 0) {
    foreach ($item in $structBlockers) {
        Write-Host "  Platform Structure  [FAIL]  $item" -ForegroundColor Red
    }
}
else {
    Write-Host '  Platform Structure  [PASS]  No platform-bound structures' -ForegroundColor Green
}

# SDK blockers
$sdkBlockers = @($blockers | Where-Object { $_ -match 'SDK binding' })
if ($sdkBlockers.Count -gt 0) {
    foreach ($item in $sdkBlockers) {
        Write-Host "  SDK Binding         [FAIL]  $item" -ForegroundColor Red
    }
}
else {
    Write-Host '  SDK Binding         [PASS]  No platform SDK imports' -ForegroundColor Green
}

# Description issues
$descBlockers = @($blockers | Where-Object { $_ -match 'Description references' })
if ($descBlockers.Count -gt 0) {
    foreach ($item in $descBlockers) {
        Write-Host "  Description         [FAIL]  $item" -ForegroundColor Red
    }
}

$descWarnings = @($warnings | Where-Object { $_ -match 'Description contains' })
if ($descWarnings.Count -gt 0) {
    foreach ($item in $descWarnings) {
        Write-Host "  Description         [WARN]  $item" -ForegroundColor Yellow
    }
}
if ($descBlockers.Count -eq 0 -and $descWarnings.Count -eq 0) {
    Write-Host '  Description         [PASS]  No platform-specific tools referenced' -ForegroundColor Green
}

# Warnings summary
$otherWarnings = @($warnings | Where-Object { $_ -notmatch 'Description contains' })
if ($otherWarnings.Count -gt 0) {
    foreach ($item in $otherWarnings) {
        Write-Host "  Note                [WARN]  $item" -ForegroundColor Yellow
    }
}

Write-Host ''

# Final result
if ($blockers.Count -gt 0) {
    Write-Host '  Result: NOT CONVERTIBLE' -ForegroundColor Red
    Write-Host ''
    $uniqueBlockers = $blockers | Sort-Object -Unique
    foreach ($reason in $uniqueBlockers) {
        Write-Host "    - $reason" -ForegroundColor Red
    }
    Write-Host ''
    Write-Host '  This source cannot be converted to a Codely CLI skill.' -ForegroundColor Red
    exit 1
}
else {
    if ($warnings.Count -gt 0) {
        Write-Host '  Result: CONVERTIBLE (with notes)' -ForegroundColor Yellow
        Write-Host ''
        Write-Host '  The skill can be converted. Review warnings above for details.' -ForegroundColor Yellow
    }
    else {
        Write-Host '  Result: CONVERTIBLE' -ForegroundColor Green
    }
    exit 0
}