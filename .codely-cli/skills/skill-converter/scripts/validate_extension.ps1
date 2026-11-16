<#
.SYNOPSIS
    Validate a Codely CLI extension directory.

.PARAMETER Path
    Path to the extension directory to validate.

.EXAMPLE
    .\validate_extension.ps1 -Path C:\project\.codely-cli\extensions\my-extension
#>

param(
    [Parameter(Mandatory=$true)]
    [string]$Path
)

$errors = [System.Collections.ArrayList]::new()
$warnings = [System.Collections.ArrayList]::new()

function Add-Error { param([string]$Msg) [void]$script:errors.Add($Msg) }
function Add-Warning { param([string]$Msg) [void]$script:warnings.Add($Msg) }

# ── 1. REQUIRED FILES CHECK ───────────────────────────────────────────

$manifestPath = Join-Path $Path 'gemini-extension.json'
$skillMdPath = Join-Path $Path 'SKILL.md'

# gemini-extension.json
if (-not (Test-Path $manifestPath)) {
    Add-Error "Missing gemini-extension.json"
} else {
    Write-Host "  Manifest           [PASS]  gemini-extension.json found" -ForegroundColor Green
    
    # Parse and validate manifest
    try {
        $manifest = Get-Content $manifestPath -Raw | ConvertFrom-Json
        
        # name field
        if (-not $manifest.name) {
            Add-Error "Manifest missing 'name' field"
        } elseif ($manifest.name -notmatch '^[a-z0-9-]+$') {
            Add-Warning "Manifest 'name' should be kebab-case: $($manifest.name)"
        } else {
            Write-Host "  Manifest.name      [PASS]  $($manifest.name)" -ForegroundColor Green
        }
        
        # version field
        if (-not $manifest.version) {
            Add-Warning "Manifest missing 'version' field"
        } else {
            Write-Host "  Manifest.version   [PASS]  $($manifest.version)" -ForegroundColor Green
        }
        
        # description field
        if (-not $manifest.description) {
            Add-Error "Manifest missing 'description' field"
        } elseif ($manifest.description.Length -lt 20) {
            Add-Warning "Manifest 'description' is short (< 20 chars)"
        } else {
            Write-Host "  Manifest.desc      [PASS]  Length: $($manifest.description.Length) chars" -ForegroundColor Green
        }
        
        # contextFileName - determine the context file
        $contextFileName = if ($manifest.contextFileName) { $manifest.contextFileName } else { 'CODELY.md' }
        $contextFilePath = Join-Path $Path $contextFileName
        
        if ($manifest.contextFileName -and $manifest.contextFileName -ne 'CODELY.md') {
            Write-Host "  Context File Name  [INFO]  Using custom: $contextFileName" -ForegroundColor Yellow
        }
        
        # excludeTools validation
        if ($manifest.excludeTools) {
            Write-Host "  Manifest.tools     [INFO]  Excluded tools: $($manifest.excludeTools.Count)" -ForegroundColor Yellow
        }
        
        # settings validation
        if ($manifest.settings) {
            Write-Host "  Manifest.settings  [INFO]  Settings defined: $($manifest.settings.Count)" -ForegroundColor Yellow
        }
        
        # mcpServers validation
        if ($manifest.mcpServers) {
            $mcpCount = ($manifest.mcpServers | Get-Member -MemberType NoteProperty).Count
            Write-Host "  Manifest.mcp       [INFO]  MCP servers: $mcpCount" -ForegroundColor Yellow
        }
        
    } catch {
        Add-Error "Invalid JSON in gemini-extension.json: $_"
    }
}

# Context file (CODELY.md or custom contextFileName)
if ($manifest -and $manifest.contextFileName) {
    $contextFileName = $manifest.contextFileName
} else {
    $contextFileName = 'CODELY.md'
}

$codelyMdPath = Join-Path $Path $contextFileName

if (-not (Test-Path $codelyMdPath)) {
    # Also check for default CODELY.md if using custom name
    $defaultPath = Join-Path $Path 'CODELY.md'
    if (-not (Test-Path $defaultPath)) {
        Add-Error "Missing context file: $contextFileName"
    }
} else {
    Write-Host "  Context File       [PASS]  $contextFileName found" -ForegroundColor Green
    
    $codelyMd = Get-Content $codelyMdPath -Raw -Encoding UTF8
    
    if ($codelyMd.Length -lt 100) {
        Add-Warning "Context file content is very short (< 100 chars)"
    } else {
        Write-Host "  Context Content    [PASS]  Length: $($codelyMd.Length) chars" -ForegroundColor Green
    }
    
    # Check for TODO markers
    if ($codelyMd -match 'TODO') {
        Add-Warning "Context file contains TODO markers"
    }
    
    # Check for placeholders
    if ($codelyMd -match '<[A-Z_]+>') {
        Add-Warning "Context file contains unfilled placeholders"
    }
}

# ── 2. OPTIONAL FILES CHECK ───────────────────────────────────────────

$readmePath = Join-Path $Path 'README.md'
if (Test-Path $readmePath) {
    Write-Host "  README             [INFO]  README.md found" -ForegroundColor Yellow
}

$skillMdPath = Join-Path $Path 'SKILL.md'
if (Test-Path $skillMdPath) {
    Add-Warning "Extension has SKILL.md (should use CODELY.md as primary)"
}

# ── 3. DIRECTORY STRUCTURE CHECK ──────────────────────────────────────

$expectedDirs = @('scripts', 'references', 'templates', 'assets', 'src', 'skills')
$foundDirs = @()

foreach ($dir in $expectedDirs) {
    $dirPath = Join-Path $Path $dir
    if (Test-Path $dirPath -PathType Container) {
        $foundDirs += $dir
    }
}

if ($foundDirs.Count -gt 0) {
    Write-Host "  Directories        [INFO]  Found: $($foundDirs -join ', ')" -ForegroundColor Yellow
} else {
    Write-Host "  Directories        [INFO]  No resource directories" -ForegroundColor Gray
}

# ── 4. SKILLS SUBDIRECTORY CHECK ──────────────────────────────────────

$skillsDir = Join-Path $Path 'skills'
if (Test-Path $skillsDir -PathType Container) {
    $skillFolders = Get-ChildItem $skillsDir -Directory -ErrorAction SilentlyContinue
    if ($skillFolders.Count -gt 0) {
        Write-Host "  Sub-skills         [INFO]  Found $($skillFolders.Count) skill(s)" -ForegroundColor Yellow
        
        foreach ($skillFolder in $skillFolders) {
            $subSkillMd = Join-Path $skillFolder.FullName 'SKILL.md'
            if (-not (Test-Path $subSkillMd)) {
                Add-Warning "Sub-skill '$($skillFolder.Name)' missing SKILL.md"
            }
        }
    }
}

# ── 5. SCRIPTS VALIDATION ─────────────────────────────────────────────

$scriptsDir = Join-Path $Path 'scripts'
if (Test-Path $scriptsDir -PathType Container) {
    $scripts = Get-ChildItem $scriptsDir -File -Include '*.ps1','*.py','*.js','*.ts' -ErrorAction SilentlyContinue
    
    foreach ($script in $scripts) {
        $content = Get-Content $script.FullName -Raw -ErrorAction SilentlyContinue
        
        # Check for platform SDK imports
        if ($content -match 'import\s+anthropic|from\s+anthropic\s') {
            Add-Error "Script imports Anthropic SDK: $($script.Name)"
        }
        if ($content -match '@anthropic-ai/sdk') {
            Add-Error "Script imports Anthropic JS SDK: $($script.Name)"
        }
        if ($content -match 'from\s+claude_agent_sdk|import\s+claude_agent_sdk') {
            Add-Error "Script imports Claude Agent SDK: $($script.Name)"
        }
    }
    
    if ($errors.Count -eq 0) {
        Write-Host "  Scripts            [PASS]  No platform SDK imports" -ForegroundColor Green
    }
}

# ── REPORT ───────────────────────────────────────────────────────────

Write-Host ''
Write-Host 'Validation Summary' -ForegroundColor Cyan
Write-Host "  Source: $Path"
Write-Host ''

if ($errors.Count -gt 0) {
    Write-Host "  Errors: $($errors.Count)" -ForegroundColor Red
    foreach ($err in $errors) {
        Write-Host "    - $err" -ForegroundColor Red
    }
}

if ($warnings.Count -gt 0) {
    Write-Host "  Warnings: $($warnings.Count)" -ForegroundColor Yellow
    foreach ($warn in $warnings) {
        Write-Host "    - $warn" -ForegroundColor Yellow
    }
}

Write-Host ''

if ($errors.Count -gt 0) {
    Write-Host '  Result: FAILED' -ForegroundColor Red
    Write-Host ''
    Write-Host '  Fix the errors above before using this extension.' -ForegroundColor Red
    exit 1
}
elseif ($warnings.Count -gt 0) {
    Write-Host '  Result: PASSED (with warnings)' -ForegroundColor Yellow
    Write-Host ''
    Write-Host '  Extension is usable. Review warnings above.' -ForegroundColor Yellow
    exit 0
}
else {
    Write-Host '  Result: PASSED' -ForegroundColor Green
    Write-Host ''
    Write-Host '  Extension is ready to use.' -ForegroundColor Green
    exit 0
}
