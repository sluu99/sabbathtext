$root = (split-path -parent $MyInvocation.MyCommand.Definition) + '\..'
$version = [System.Reflection.Assembly]::LoadFile("$root\bin\net45\Norm.dll").GetName().Version
$versionStr = "{0}.{1}.{2}" -f ($version.Major, $version.Minor, $version.Build)

$content = (Get-Content $root\nuget\Norm.nuspec) 
$content = $content -replace '\$version\$',$versionStr

$content | Out-File $root\nuget\Norm.compiled.nuspec

& $root\nuget\NuGet.exe pack $root\nuget\Norm.compiled.nuspec