<#
.SYNOPSIS
    Convert a local folder into a Codely CLI skill or extension.

.DESCRIPTION
    Analyzes local folder content and converts to:
    - Skill (single MD file only) → .codely-cli/skills/
    - Extension (complex content) → .codely-cli/extensions/

.PARAMETER Path
    Local folder path to convert.

.PARAMETER Scope
    Install scope: 'project' (default) or 'global'.

.PARAMETER Output
    Custom output directory (overrides Scope).

.PARAMETER Name
    Custom skill/extension name.

.PARAMETER Overwrite
    Allow overwriting an existing directory.

.PARAMETER AsExtension
    Force conversion as extension.

.PARAMETER AsSkill
    Force conversion as skill.

.EXAMPLE
    .\convert_from_local.ps1 -Path ".\my-skill"
.EXAMPLE
    .\convert_from_local.ps1 -Path "C:\skills\my-extension" -AsExtension
#>

param(
    [Parameter(Mandatory=$true)]
    [string]$Path,

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

function Get-ContentCounts {
    param([string]$SourcePath)
    
    $excludePatterns = @('node_modules', '.git', '__pycache__', '.DS_Store', 'Thumbs.db')
    
    $allFiles = Get-ChildItem -Path $SourcePath -Recurse -File -ErrorAction SilentlyContinue | 
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
    param([string]$SourcePath)
    
    # Check if has SKILL.md and NO skills/ subdirectory
    # If there's no skills/ subdir, it's a simple skill regardless of other content
    $skillMdPath = Join-Path $SourcePath 'SKILL.md'
    $skillsDirPath = Join-Path $SourcePath 'skills'
    
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
    
    $description = ""
    $body = $content
    
    if ($content -match '(?s)^---\r?\n(.+?)\r?\n---(.*)$') {
        $frontmatter = $Matches[1]
        $body = $Matches[2].Trim()
        
        if ($frontmatter -match 'description\s*:\s*(.+?)(\r?\n|$)') {
            $description = $Matches[1].Trim()
        }
    }
    
    if (-not $description) {
        $description = "Extension for $Name."
    }
    
    New-ExtensionManifest -Name $Name -Description $Description -OutputPath $OutputPath
    New-CodelyMd -Name $Name -Description $Description -Body $body -OutputPath $OutputPath
}

function Install-AsSkill {
    param(
        [string]$SourcePath,
        [string]$OutputPath,
        [string]$Name
    )
    
    Write-Host "Installing as SKILL..." -ForegroundColor Cyan
    
    Copy-Item -Path $SourcePath -Destination $OutputPath -Recurse
    
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

This skill was converted from a local folder.
"@
        
        Set-Content -Path $skillMdPath -Value $skillMdContent -Encoding UTF8
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
    
    New-Item -ItemType Directory -Path $OutputPath -Force | Out-Null
    
    $skillMdPath = Join-Path $SourcePath 'SKILL.md'
    
    if (Test-Path $skillMdPath) {
        Convert-SkillMdToExtension -SkillMdPath $skillMdPath -OutputPath $OutputPath -Name $Name
    } else {
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
            $body = $readmeContent
        }
        
        New-ExtensionManifest -Name $Name -Description $Description -OutputPath $OutputPath
        New-CodelyMd -Name $Name -Description $Description -Body $body -OutputPath $OutputPath
    }
    
    $dirsToCopy = @('scripts', 'references', 'templates', 'assets', 'src', 'examples', 'shared')
    foreach ($dir in $dirsToCopy) {
        $srcDir = Join-Path $SourcePath $dir
        if (Test-Path $srcDir -PathType Container) {
            $dstDir = Join-Path $OutputPath $dir
            Copy-Item -Path $srcDir -Destination $dstDir -Recurse -Force
            Write-Host "  Copied: $dir/" -ForegroundColor Gray
        }
    }
    
    $readmeSrc = Join-Path $SourcePath 'README.md'
    if (Test-Path $readmeSrc) {
        Copy-Item -Path $readmeSrc -Destination (Join-Path $OutputPath 'README.md') -Force
    }
    
    return $OutputPath
}

# ── Main ─────────────────────────────────────────────────────────────

$sourcePath = $Path

if (-not (Test-Path $sourcePath)) {
    Write-Host "Error: Path not found: $sourcePath" -ForegroundColor Red
    exit 1
}

# Determine name
if ($Name) {
    $itemName = ConvertTo-KebabCase -InputString $Name
} else {
    $itemName = ConvertTo-KebabCase -InputString (Split-Path $sourcePath -Leaf)
}
Write-Host "Name: $itemName"

# ── Pre-install: convertibility check ─────────────────────────────────

$checkScript = Join-Path $PSScriptRoot 'check_convertibility.ps1'
if (Test-Path $checkScript) {
    & $checkScript -Path $sourcePath
    if ($LASTEXITCODE -ne 0) {
        exit 1
    }
}

# ── Determine output type ─────────────────────────────────────────────

$isSkill = $false

if ($AsSkill) {
    $isSkill = $true
    Write-Host "Type: SKILL (forced)" -ForegroundColor Yellow
}
elseif ($AsExtension) {
    $isSkill = $false
    Write-Host "Type: EXTENSION (forced)" -ForegroundColor Yellow
}
else {
    if (Test-IsSimpleSkill -SourcePath $sourcePath) {
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
        Write-Host 'Use -Overwrite to replace it.' -ForegroundColor Red
        exit 1
    }
    Write-Host "Overwriting existing directory..." -ForegroundColor Yellow
    Remove-Item $outputPath -Recurse -Force
}

# Create output parent directory
$outputDir = Split-Path $outputPath -Parent
if (-not (Test-Path $outputDir)) {
    New-Item -ItemType Directory -Path $outputDir -Force | Out-Null
}

# ── Install ──────────────────────────────────────────────────────────

if ($isSkill) {
    Install-AsSkill -SourcePath $sourcePath -OutputPath $outputPath -Name $itemName
} else {
    Install-AsExtension -SourcePath $sourcePath -OutputPath $outputPath -Name $itemName
}

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
}

Write-Host ''
Write-Host "Run '/skills reload' or '/extensions reload' in Codely CLI to enable." -ForegroundColor Yellow