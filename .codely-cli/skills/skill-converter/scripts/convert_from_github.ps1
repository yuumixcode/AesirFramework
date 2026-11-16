<#
.SYNOPSIS
    Convert a GitHub repository or subfolder into a Codely CLI skill or extension.

.DESCRIPTION
    Downloads only the specified subfolder (via git sparse-checkout) and converts
    it to a Codely CLI skill or extension based on content analysis.
    
    - Single MD file only → Skill (installed to .codely-cli/skills/)
    - Complex content → Extension (installed to .codely-cli/extensions/)

.PARAMETER Url
    GitHub URL to convert. Supports formats like:
    - https://github.com/{owner}/{repo}/tree/{branch}/{path}
    - https://github.com/{owner}/{repo}

.PARAMETER Scope
    Install scope: 'project' (default) or 'global'.
    - project: installs to <cwd>/.codely-cli/
    - global:  installs to ~/.codely-cli/

.PARAMETER Output
    Custom output directory (overrides Scope).

.PARAMETER Name
    Custom skill/extension name. Default is derived from the folder name.

.PARAMETER Overwrite
    Allow overwriting an existing directory.

.PARAMETER AsExtension
    Force conversion as extension (skip auto-detection).

.PARAMETER AsSkill
    Force conversion as skill (skip auto-detection).

.EXAMPLE
    .\convert_from_github.ps1 -Url "https://github.com/anthropics/skills/tree/main/skills/algorithmic-art"
.EXAMPLE
    .\convert_from_github.ps1 -Url "https://github.com/user/repo" -AsExtension -Scope global
#>

param(
    [Parameter(Mandatory=$true)]
    [string]$Url,

    [ValidateSet('project', 'global')]
    [string]$Scope = 'project',

    [string]$Output,

    [string]$Name,

    [switch]$Overwrite,

    [switch]$AsExtension,

    [switch]$AsSkill
)

# ── Helpers ──────────────────────────────────────────────────────────

function ConvertTo-KebabCase {
    param([string]$InputString)
    $result = $InputString -replace '[\s_]+', '-'
    $result = $result -replace '[^a-zA-Z0-9-]', ''
    $result = $result.ToLower().Trim('-')
    return $result
}

function Parse-GitHubUrl {
    param([string]$GitUrl)

    if ($GitUrl -match 'github\.com/([^/]+)/([^/]+)/tree/([^/]+)/(.+)') {
        return @{
            Owner  = $Matches[1]
            Repo   = $Matches[2]
            Branch = $Matches[3]
            Path   = $Matches[4].TrimEnd('/')
        }
    }
    elseif ($GitUrl -match 'github\.com/([^/]+)/([^/]+)/?$') {
        return @{
            Owner  = $Matches[1]
            Repo   = $Matches[2]
            Branch = 'main'
            Path   = ''
        }
    }

    throw "Invalid GitHub URL format: $GitUrl"
}

function Test-GitAvailable {
    try {
        $null = git --version 2>&1
        return $LASTEXITCODE -eq 0
    } catch {
        return $false
    }
}

function Get-ContentCounts {
    param([string]$Path)
    
    # Exclude hidden items, node_modules, .git, etc.
    $excludePatterns = @('node_modules', '.git', '__pycache__', '.DS_Store', 'Thumbs.db')
    
    $allFiles = Get-ChildItem -Path $Path -Recurse -File -ErrorAction SilentlyContinue | 
        Where-Object { 
            $dir = $_.DirectoryName
            -not ($excludePatterns | Where-Object { $dir -like "*\$_*" })
        }
    
    $mdFiles = $allFiles | Where-Object { $_.Extension -eq '.md' }
    $otherFiles = $allFiles | Where-Object { $_.Extension -ne '.md' }
    
    return @{
        MdFiles = $mdFiles
        OtherFiles = $otherFiles
        MdCount = $mdFiles.Count
        OtherCount = $otherFiles.Count
        TotalCount = $allFiles.Count
    }
}

function Test-IsSimpleSkill {
    param([string]$Path)
    
    # Check if has SKILL.md and NO skills/ subdirectory
    # If there's no skills/ subdir, it's a simple skill regardless of other content
    $skillMdPath = Join-Path $Path 'SKILL.md'
    $skillsDirPath = Join-Path $Path 'skills'
    
    # Must have SKILL.md
    if (-not (Test-Path $skillMdPath -PathType Leaf)) {
        return $false
    }
    
    # Must NOT have skills/ subdirectory
    # If skills/ exists, this is an extension containing sub-skills
    if (Test-Path $skillsDirPath -PathType Container) {
        return $false
    }
    
    # Has SKILL.md but no skills/ subdirectory → simple skill
    return $true
}

function New-ExtensionManifest {
    param(
        [string]$Name,
        [string]$Description,
        [string]$OutputPath
    )
    
    $manifest = @{
        name = $Name
        version = "1.0.0"
        description = $Description
        contextFileName = "CODELY.md"
        excludeTools = @()
    }
    
    $jsonPath = Join-Path $OutputPath 'gemini-extension.json'
    $manifest | ConvertTo-Json -Depth 10 | Set-Content -Path $jsonPath -Encoding UTF8
    
    return $jsonPath
}

function New-CodelyMd {
    param(
        [string]$Name,
        [string]$Description,
        [string]$Body,
        [string]$OutputPath
    )
    
    $content = @"


# $Name - Codely CLI Extension

$Description

$Body

---

*This extension was converted using skill-converter*
"@
    
    $mdPath = Join-Path $OutputPath 'CODELY.md'
    Set-Content -Path $mdPath -Value $content -Encoding UTF8
    
    return $mdPath
}

function Convert-SkillMdToExtension {
    param(
        [string]$SkillMdPath,
        [string]$OutputPath,
        [string]$Name
    )
    
    $content = Get-Content $SkillMdPath -Raw -Encoding UTF8
    
    # Parse frontmatter
    $description = ""
    $body = $content
    
    if ($content -match '(?s)^---\r?\n(.+?)\r?\n---(.*)$') {
        $frontmatter = $Matches[1]
        $body = $Matches[2].Trim()
        
        # Extract description from frontmatter
        if ($frontmatter -match 'description\s*:\s*(.+?)(\r?\n|$)') {
            $description = $Matches[1].Trim()
        }
    }
    
    if (-not $description) {
        $description = "Extension for $Name."
    }
    
    # Create manifest
    New-ExtensionManifest -Name $Name -Description $Description -OutputPath $OutputPath
    
    # Create CODELY.md
    New-CodelyMd -Name $Name -Description $Description -Body $body -OutputPath $OutputPath
}

function Install-AsSkill {
    param(
        [string]$SourcePath,
        [string]$OutputPath,
        [string]$Name
    )
    
    Write-Host "Installing as SKILL..." -ForegroundColor Cyan
    
    # Copy all files
    Copy-Item -Path $SourcePath -Destination $OutputPath -Recurse
    
    # Ensure SKILL.md exists
    $skillMdPath = Join-Path $OutputPath 'SKILL.md'
    if (-not (Test-Path $skillMdPath)) {
        $readmePath = Join-Path $OutputPath 'README.md'
        $description = "Skill for $Name."
        
        if (Test-Path $readmePath) {
            $lines = (Get-Content $readmePath -Raw) -split '\r?\n'
            $paragraph = ''
            $inParagraph = $false
            foreach ($line in $lines) {
                $trimmed = $line.Trim()
                if ($trimmed.StartsWith('#')) { if ($inParagraph) { break }; continue }
                if ($trimmed -eq '')          { if ($inParagraph) { break }; continue }
                if (-not $inParagraph) { $inParagraph = $true }
                $paragraph += ' ' + $trimmed
            }
            if ($paragraph.Trim()) {
                $description = $paragraph.Trim().Substring(0, [Math]::Min(200, $paragraph.Trim().Length))
            }
        }
        
        $skillMdContent = @"
---
name: $Name
description: $description
---

# $($Name.Replace('-', ' ').ToUpper())

This skill was converted from a GitHub repository.
"@
        
        Set-Content -Path $skillMdPath -Value $skillMdContent -Encoding UTF8
    }
    
    return $OutputPath
}

function Install-GeminiExtension {
    param(
        [string]$SourcePath,
        [string]$OutputPath,
        [string]$Name
    )
    
    Write-Host "Installing as GEMINI EXTENSION (direct copy)..." -ForegroundColor Cyan
    
    # Create output directory
    New-Item -ItemType Directory -Path $OutputPath -Force | Out-Null
    
    # Copy all relevant files (exclude .github, workflows, etc.)
    $excludePatterns = @('.github', '.git', 'node_modules', '__pycache__', '.DS_Store', 'Thumbs.db')
    
    $items = Get-ChildItem -Path $SourcePath -ErrorAction SilentlyContinue | 
        Where-Object { $excludePatterns -notcontains $_.Name }
    
    foreach ($item in $items) {
        $dst = Join-Path $OutputPath $item.Name
        if ($item.PSIsContainer) {
            Copy-Item -Path $item.FullName -Destination $dst -Recurse -Force
        } else {
            Copy-Item -Path $item.FullName -Destination $dst -Force
        }
        Write-Host "  Copied: $($item.Name)" -ForegroundColor Gray
    }
    
    return $OutputPath
}

function Install-AsExtension {
    param(
        [string]$SourcePath,
        [string]$OutputPath,
        [string]$Name
    )
    
    Write-Host "Installing as EXTENSION..." -ForegroundColor Cyan
    
    # Create output directory
    New-Item -ItemType Directory -Path $OutputPath -Force | Out-Null
    
    # Check for existing SKILL.md
    $skillMdPath = Join-Path $SourcePath 'SKILL.md'
    
    if (Test-Path $skillMdPath) {
        # Convert SKILL.md to extension format
        Convert-SkillMdToExtension -SkillMdPath $skillMdPath -OutputPath $OutputPath -Name $Name
    } else {
        # Generate from README or create minimal
        $readmePath = Join-Path $SourcePath 'README.md'
        $description = "Extension for $Name."
        $body = ""
        
        if (Test-Path $readmePath) {
            $readmeContent = Get-Content $readmePath -Raw -Encoding UTF8
            $lines = $readmeContent -split '\r?\n'
            $paragraph = ''
            $inParagraph = $false
            foreach ($line in $lines) {
                $trimmed = $line.Trim()
                if ($trimmed.StartsWith('#')) { if ($inParagraph) { break }; continue }
                if ($trimmed -eq '')          { if ($inParagraph) { break }; continue }
                if (-not $inParagraph) { $inParagraph = $true }
                $paragraph += ' ' + $trimmed
            }
            if ($paragraph.Trim()) {
                $description = $paragraph.Trim().Substring(0, [Math]::Min(200, $paragraph.Trim().Length))
            }
            # Use README body
            $body = $readmeContent
        }
        
        New-ExtensionManifest -Name $Name -Description $Description -OutputPath $OutputPath
        New-CodelyMd -Name $Name -Description $Description -Body $body -OutputPath $OutputPath
    }
    
    # Copy other files (scripts, references, templates, assets, etc.)
    $dirsToCopy = @('scripts', 'references', 'templates', 'assets', 'src', 'examples', 'shared')
    foreach ($dir in $dirsToCopy) {
        $srcDir = Join-Path $SourcePath $dir
        if (Test-Path $srcDir -PathType Container) {
            $dstDir = Join-Path $OutputPath $dir
            Copy-Item -Path $srcDir -Destination $dstDir -Recurse -Force
            Write-Host "  Copied: $dir/" -ForegroundColor Gray
        }
    }
    
    # Copy README if exists
    $readmeSrc = Join-Path $SourcePath 'README.md'
    if (Test-Path $readmeSrc) {
        Copy-Item -Path $readmeSrc -Destination (Join-Path $OutputPath 'README.md') -Force
    }
    
    return $OutputPath
}

# ── Download: sparse-checkout (preferred) ────────────────────────────

function Get-SparseCheckout {
    param(
        [hashtable]$Info,
        [string]$TempDir
    )

    $repoUrl  = "https://github.com/$($Info.Owner)/$($Info.Repo).git"
    $cloneDir = Join-Path $TempDir 'repo'

    Write-Host "Downloading (sparse-checkout): $($Info.Path)" -ForegroundColor Cyan

    git clone --depth 1 --filter=blob:none --sparse `
        --branch $Info.Branch $repoUrl $cloneDir 2>&1 | Out-Null
    if ($LASTEXITCODE -ne 0) { return $null }

    Push-Location $cloneDir
    try {
        if ($Info.Path) {
            git sparse-checkout set $Info.Path 2>&1 | Out-Null
        }
        if ($LASTEXITCODE -ne 0) { return $null }
    }
    finally {
        Pop-Location
    }

    if ($Info.Path) {
        return Join-Path $cloneDir $Info.Path
    }
    return $cloneDir
}

# ── Download: full archive (fallback) ────────────────────────────────

function Get-FullArchive {
    param(
        [hashtable]$Info,
        [string]$TempDir
    )

    $archiveUrl  = "https://github.com/$($Info.Owner)/$($Info.Repo)/archive/refs/heads/$($Info.Branch).zip"
    $archivePath = Join-Path $TempDir "$($Info.Repo).zip"

    Write-Host "Downloading (full archive): $archiveUrl" -ForegroundColor Yellow

    try {
        Invoke-WebRequest -Uri $archiveUrl -OutFile $archivePath -UseBasicParsing
    } catch {
        Write-Host "Error downloading: $_" -ForegroundColor Red
        return $null
    }

    $extractPath = Join-Path $TempDir 'extracted'
    Expand-Archive -Path $archivePath -DestinationPath $extractPath -Force

    $root = Join-Path $extractPath "$($Info.Repo)-$($Info.Branch)"
    if ($Info.Path) {
        return Join-Path $root $Info.Path
    }
    return $root
}

# ── Main ─────────────────────────────────────────────────────────────

$githubInfo = Parse-GitHubUrl -GitUrl $Url
Write-Host "Parsed: owner=$($githubInfo.Owner), repo=$($githubInfo.Repo), branch=$($githubInfo.Branch), path=$($githubInfo.Path)"

# Create temp directory
$tempDir = Join-Path $env:TEMP "skill-converter-$(Get-Date -Format 'yyyyMMddHHmmss')"
New-Item -ItemType Directory -Path $tempDir -Force | Out-Null

# Try sparse-checkout first, fall back to full archive
$sourcePath = $null

if ($githubInfo.Path -and (Test-GitAvailable)) {
    $sourcePath = Get-SparseCheckout -Info $githubInfo -TempDir $tempDir
    if (-not $sourcePath -or -not (Test-Path $sourcePath)) {
        Write-Host 'Sparse-checkout failed, falling back to full archive...' -ForegroundColor Yellow
        $sourcePath = $null
    }
}

if (-not $sourcePath) {
    $sourcePath = Get-FullArchive -Info $githubInfo -TempDir $tempDir
}

if (-not $sourcePath -or -not (Test-Path $sourcePath)) {
    Write-Host "Error: Path not found in repository: $($githubInfo.Path)" -ForegroundColor Red
    Remove-Item $tempDir -Recurse -Force -ErrorAction SilentlyContinue
    exit 1
}

# ── Pre-install: convertibility check ─────────────────────────────────

$checkScript = Join-Path $PSScriptRoot 'check_convertibility.ps1'
if (Test-Path $checkScript) {
    & $checkScript -Path $sourcePath
    if ($LASTEXITCODE -ne 0) {
        Remove-Item $tempDir -Recurse -Force -ErrorAction SilentlyContinue
        exit 1
    }
}
else {
    Write-Host 'Warning: check_convertibility.ps1 not found, skipping pre-install check.' -ForegroundColor Yellow
}

# Determine name
if ($Name) {
    $itemName = ConvertTo-KebabCase -InputString $Name
} else {
    $itemName = ConvertTo-KebabCase -InputString (Split-Path $sourcePath -Leaf)
}
Write-Host "Name: $itemName"

# ── Check if already Gemini CLI extension ──────────────────────────────

$geminiExtPath = Join-Path $sourcePath 'gemini-extension.json'
$isGeminiExt = $false

if (Test-Path $geminiExtPath) {
    try {
        $manifest = Get-Content $geminiExtPath -Raw | ConvertFrom-Json
        $contextFileName = if ($manifest.contextFileName) { $manifest.contextFileName } else { 'CODELY.md' }
        $contextFilePath = Join-Path $sourcePath $contextFileName
        
        if (Test-Path $contextFilePath) {
            $isGeminiExt = $true
            Write-Host "Type: GEMINI EXTENSION (already compatible)" -ForegroundColor Green
        }
    } catch {
        # Continue with normal detection
    }
}

# ── Determine output type ─────────────────────────────────────────────

$isSkill = $false

if ($isGeminiExt) {
    # Already a Gemini extension, copy directly
    $isSkill = $false
}
elseif ($AsSkill) {
    $isSkill = $true
    Write-Host "Type: SKILL (forced)" -ForegroundColor Yellow
}
elseif ($AsExtension) {
    $isSkill = $false
    Write-Host "Type: EXTENSION (forced)" -ForegroundColor Yellow
}
else {
    # Auto-detect
    if (Test-IsSimpleSkill -Path $sourcePath) {
        $isSkill = $true
        Write-Host "Type: SKILL (only SKILL.md, no other content)" -ForegroundColor Green
    } else {
        $isSkill = $false
        Write-Host "Type: EXTENSION (has additional content beyond SKILL.md)" -ForegroundColor Green
    }
}

# Determine output path
$typeDir = if ($isSkill) { 'skills' } else { 'extensions' }

if ($Output) {
    $outputPath = Join-Path $Output $itemName
} elseif ($Scope -eq 'global') {
    $outputPath = Join-Path $HOME ".codely-cli\$typeDir\$itemName"
} else {
    $projectRoot = Get-Location
    $outputPath = Join-Path $projectRoot ".codely-cli\$typeDir\$itemName"
}
Write-Host "Output path: $outputPath"

# ── Overwrite protection ─────────────────────────────────────────────

if (Test-Path $outputPath) {
    if (-not $Overwrite) {
        Write-Host ''
        Write-Host "Error: '$outputPath' already exists." -ForegroundColor Red
        Write-Host 'Use -Overwrite to replace it, or delete the directory manually.' -ForegroundColor Red
        Remove-Item $tempDir -Recurse -Force -ErrorAction SilentlyContinue
        exit 1
    }
    Write-Host "Overwriting existing directory at: $outputPath" -ForegroundColor Yellow
    Remove-Item $outputPath -Recurse -Force
}

# Create output parent directory
$outputDir = Split-Path $outputPath -Parent
if (-not (Test-Path $outputDir)) {
    New-Item -ItemType Directory -Path $outputDir -Force | Out-Null
}

# ── Install ──────────────────────────────────────────────────────────

if ($isGeminiExt) {
    Install-GeminiExtension -SourcePath $sourcePath -OutputPath $outputPath -Name $itemName
} elseif ($isSkill) {
    Install-AsSkill -SourcePath $sourcePath -OutputPath $outputPath -Name $itemName
} else {
    Install-AsExtension -SourcePath $sourcePath -OutputPath $outputPath -Name $itemName
}

# Cleanup temp
Remove-Item $tempDir -Recurse -Force -ErrorAction SilentlyContinue

Write-Host ''
Write-Host "Success: $($itemName) installed to $outputPath" -ForegroundColor Green
Write-Host ''

# Validate - use appropriate validator
if ($isSkill) {
    $validateScript = Join-Path $PSScriptRoot 'validate_skill.ps1'
} else {
    $validateScript = Join-Path $PSScriptRoot 'validate_extension.ps1'
}

if (Test-Path $validateScript) {
    Write-Host 'Running validation...' -ForegroundColor Cyan
    & $validateScript -Path $outputPath
} else {
    Write-Host 'Warning: validation script not found, skipping validation.' -ForegroundColor Yellow
}

Write-Host ''
Write-Host "Run '/skills reload' or '/extensions reload' in Codely CLI to enable." -ForegroundColor Yellow