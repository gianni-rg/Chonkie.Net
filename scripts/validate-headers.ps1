# Copyright 2025-2026 Gianni Rosa Gallina and Contributors
#
# Licensed under the Apache License, Version 2.0 (the "License");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
#
#     http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.

#!/usr/bin/env pwsh
# Validate and optionally fix Apache License headers in source files
# Usage:
#   pwsh validate-headers.ps1                    # Validate staged files (pre-commit mode)
#   pwsh validate-headers.ps1 -All               # Validate all source files
#   pwsh validate-headers.ps1 -All -Fix          # Auto-fix all missing headers
#   pwsh validate-headers.ps1 -Fix               # Auto-fix staged files only

param(
    [switch]$All = $false,
    [switch]$Fix = $false,
    [switch]$Verbose = $false
)

$projectRoot = git rev-parse --show-toplevel
$csharpHeader = @"
// Copyright 2025-2026 Gianni Rosa Gallina and Contributors
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
"@

$pythonHeader = @"
# Copyright 2025-2026 Gianni Rosa Gallina and Contributors
#
# Licensed under the Apache License, Version 2.0 (the "License");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
#
#     http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.
"@

$missingHeaders = @()
$fixedFiles = @()
$filesChecked = 0

# Determine which files to check
if ($All) {
    # Get all source files in the repository (C# and Python only - no Markdown)
    $filesToCheck = Get-ChildItem -Path $projectRoot -Recurse -File -Include "*.cs", "*.py" |
        Where-Object {
            $_.FullName -notmatch '(\.github|\.vscode|models|packages|bin|obj|\.vs|\.git|\.generated\.cs|scripts.hooks)' -and
            $_.FullName -notmatch '\\\..*' # Skip hidden directories
        }
}
else {
    # Get staged files only (pre-commit mode)
    $stagedFiles = git diff --cached --name-only --diff-filter=ACM
    $filesToCheck = $stagedFiles | Where-Object {
        $_ -match '\.(cs|py)$' -and
        $_ -notmatch '(^\.github/|^\.vscode/|^models/|\.generated\.cs$|^\.)' -and
        $_ -notmatch '(packages/|bin/|obj/|\.vs/)'
    }
}

foreach ($file in $filesToCheck) {
    $filesChecked++

    # Build full path
    if ($All) {
        $fullPath = $file.FullName
        $displayPath = $file.FullName -replace [regex]::Escape($projectRoot), "." -replace "\\", "/"
    }
    else {
        $fullPath = Join-Path $projectRoot $file
        $displayPath = $file
    }

    if (-not (Test-Path $fullPath)) {
        continue
    }

    $content = Get-Content -Path $fullPath -Raw -Encoding UTF8

    # Skip empty files
    if ([string]::IsNullOrWhiteSpace($content)) {
        continue
    }

    $hasHeader = $false
    $headerToAdd = ""
    $extension = [IO.Path]::GetExtension($fullPath)

    if ($extension -eq ".cs") {
        $hasHeader = $content.StartsWith("// Copyright 2025-2026")
        $headerToAdd = $csharpHeader
    }
    elseif ($extension -eq ".py") {
        $hasHeader = $content.StartsWith("# Copyright 2025-2026")
        $headerToAdd = $pythonHeader
    }

    if (-not $hasHeader) {
        if ($Fix) {
            # Add header to file
            $newContent = $headerToAdd + "`n`n" + $content
            Set-Content -Path $fullPath -Value $newContent -Encoding UTF8 -NoNewline
            $fixedFiles += $displayPath
            if ($Verbose) { Write-Host "✏️  Fixed: $displayPath" -ForegroundColor Cyan }
        }
        else {
            $missingHeaders += $displayPath
        }
    }
}

if ($Fix -and $fixedFiles.Count -gt 0) {
    Write-Host "✅ License header fixes completed" -ForegroundColor Green
    Write-Host "Fixed $($fixedFiles.Count) file(s):" -ForegroundColor Green
    $fixedFiles | ForEach-Object { Write-Host "  ✓ $_" }
    Write-Host ""

    # For pre-commit, ask to stage the changes
    if (-not $All) {
        Write-Host "⚠️  Please stage the fixed files before committing:" -ForegroundColor Yellow
        Write-Host "  git add $($fixedFiles -join ' ')" -ForegroundColor Cyan
    }
    exit 0
}
elseif ($missingHeaders.Count -gt 0) {
    Write-Host "❌ License header validation failed" -ForegroundColor Red
    Write-Host ""
    Write-Host "The following files are missing Apache License headers:" -ForegroundColor Yellow
    foreach ($file in $missingHeaders) {
        Write-Host "  - $file"
    }
    Write-Host ""

    if ($All) {
        Write-Host "To automatically fix these files, run:" -ForegroundColor Cyan
        Write-Host "  pwsh scripts/validate-headers.ps1 -All -Fix" -ForegroundColor Cyan
    }
    else {
        Write-Host "To automatically fix staged files, run:" -ForegroundColor Cyan
        Write-Host "  pwsh scripts/validate-headers.ps1 -Fix" -ForegroundColor Cyan
    }

    Write-Host ""
    Write-Host "Or manually add headers using templates from LICENSE_HEADERS.md" -ForegroundColor Cyan
    exit 1
}

$mode = if ($All) { "all source files" } else { "$filesChecked staged file(s)" }
Write-Host "✅ License header validation passed ($mode)" -ForegroundColor Green
exit 0
