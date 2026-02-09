param(
    [string]$ModelsPath = (Join-Path $PSScriptRoot "..\models"),
    [string]$TestProject = (Join-Path $PSScriptRoot "..\tests\Chonkie.Embeddings.Integration.Tests\Chonkie.Embeddings.Integration.Tests.csproj"),
    [string]$Filter = "FullyQualifiedName~SentenceTransformerEmbeddingsIntegrationTests",
    [switch]$RunAllProvidersPerModel,
    [switch]$ContinueOnFailure
)

$ErrorActionPreference = "Stop"

if (-not (Test-Path -Path $ModelsPath))
{
    throw "Models folder not found: $ModelsPath"
}

if (-not (Test-Path -Path $TestProject))
{
    throw "Test project not found: $TestProject"
}

$models = Get-ChildItem -Path $ModelsPath -Directory | Sort-Object Name
if ($models.Count -eq 0)
{
    throw "No model folders found under: $ModelsPath"
}

$resultsRoot = Join-Path $PSScriptRoot "..\tests\Chonkie.Embeddings.Integration.Tests\TestResults\Models"
New-Item -Path $resultsRoot -ItemType Directory -Force | Out-Null

$failedModels = New-Object System.Collections.Generic.List[string]

foreach ($model in $models)
{
    $modelOnnxPath = Join-Path $model.FullName "model.onnx"
    if (-not (Test-Path -Path $modelOnnxPath))
    {
        Write-Warning "Skipping '$($model.Name)' because model.onnx was not found."
        continue
    }

    $env:CHONKIE_SENTENCE_TRANSFORMER_MODEL_PATH = $model.FullName

    $modelResultsDir = Join-Path $resultsRoot $model.Name
    New-Item -Path $modelResultsDir -ItemType Directory -Force | Out-Null

    $dotnetArgs = @(
        "test",
        $TestProject,
        "--results-directory",
        $modelResultsDir,
        "--logger",
        "trx;LogFileName=EmbeddingIntegration_$($model.Name).trx"
    )

    if (-not $RunAllProvidersPerModel)
    {
        $dotnetArgs += @("--filter", $Filter)
    }

    Write-Host "Running embeddings integration tests for model '$($model.Name)'..."
    & dotnet @dotnetArgs

    if ($LASTEXITCODE -ne 0)
    {
        $failedModels.Add($model.Name) | Out-Null
        if (-not $ContinueOnFailure)
        {
            $env:CHONKIE_SENTENCE_TRANSFORMER_MODEL_PATH = $null
            exit $LASTEXITCODE
        }
    }
}

$env:CHONKIE_SENTENCE_TRANSFORMER_MODEL_PATH = $null

if ($failedModels.Count -gt 0)
{
    Write-Host "One or more models failed: $($failedModels -join ', ')"
    exit 1
}

Write-Host "All model runs completed successfully."
