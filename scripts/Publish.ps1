$workspaceFolder = (Get-Item $PSScriptRoot).Parent.FullName

$SimpleProjFSSample = Join-Path $workspaceFolder "SimpleProjFSSample" "SimpleProjFSSample.csproj"
$ProjFSFeatureController = Join-Path $workspaceFolder "ProjFSFeatureController" "ProjFSFeatureController.csproj"

$outputPath = Join-Path $workspaceFolder "PublishOutput"
if (Test-Path $outputPath) { Remove-Item -LiteralPath $outputPath -Force -Recurse }

$buildArgs = "--output:'$outputPath' --runtime:win-x64 /p:GenerateFullPaths=true"

foreach ($arg in $args) {
    if ($arg -eq "-sc" || $arg -eq "--self-contained") {
        $buildArgs = "$buildArgs /p:SelfContained=true"
    }
    elseif ($arg -eq "-sf" || $arg -eq "--single-file") {
        $buildArgs = "$buildArgs /p:PublishSingleFile=true /p:IncludeNativeLibraryForSelfExtract=true"
    }
}


# TODO: passing args without qouting (still no solution after googling a few hours)
dotnet.exe publish $SimpleProjFSSample $buildArgs
dotnet.exe publish $ProjFSFeatureController $buildArgs