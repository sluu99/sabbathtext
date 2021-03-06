$root = (split-path -parent $MyInvocation.MyCommand.Definition)

$msbuild = "${env:ProgramFiles(x86)}\MSBuild\12.0\Bin\MSBuild.exe"
if (-not(Test-Path $msbuild)) {
    $msbuild = "${env:ProgramFiles}\MSBuild\12.0\Bin\MSBuild.exe"
}

$vstest = "${env:ProgramFiles(x86)}\Microsoft Visual Studio 12.0\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe"
if (-not(Test-Path $vstest)) {
    $vstest = "${env:ProgramFiles}\Microsoft Visual Studio 12.0\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe"
}

$solution = "$root\SabbathText.sln"
$testAssemblies = @(
    "$root\SabbathText.Tests\bin\Release\SabbathText.Tests.dll"
)

$command = "build"

if ($args.Count -gt 0) {
    $command = $args[0]
}

if (($command -like "all") -or ($command -like "clean")) {
    Write-Output("Cleaning the solution...")
    & $msbuild $solution /t:Clean /p:Configuration=Release /p:Platform="Any CPU" /clp:Verbosity=minimal
}

if (($command -like "all") -or ($command -like "build")) {

    # NuGet restore 
    Write-Output("Restoring NuGet packages")
    & "$root\nuget\Nuget.exe" restore $solution

    Write-Output("Rebuilding the solution")
    & $msbuild $solution /t:Rebuild /p:Configuration=Release /p:Platform="Any CPU" /clp:Verbosity=minimal
}

if ($command -like "test") {
    foreach ($ta in $testAssemblies) {
        Write-Output("Running test assembly " + $ta)
        & $vstest $ta
        
        if ($lastexitcode -ne 0) {
            throw "Test execution failed"
        }
        
        Write-Output("Completed!")
        Write-Output("")
    }
}