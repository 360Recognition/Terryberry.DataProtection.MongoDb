Remove-Item *.nupkg
Remove-Item *.snupkg
if (Test-Path .\source\bin\Release) {
  Remove-Item .\source\bin\Release\*.nupkg
  Remove-Item .\source\bin\Release\*.snupkg
}

# get version number
$version = Read-Host -Prompt 'New Version Number'

if (!($version -match "^\d+\.\d+\.\d+$")) {
  Write-Host "bad"
  exit 1
}

# confirm that you know what you're doing
$confirm = Read-Host -Prompt 'This is a public NuGet package on nuget.org. Publishing cannot be undone. Are you sure? Y/(N)'

if (!($confirm -eq 'Y' -or $confirm -eq 'y')) {
  Write-Host "bad"
  exit 1
}

# update version number in projects that produce NuGet packages
Get-ChildItem -Recurse -Filter "Terryberry.DataProtection.MongoDb.csproj" | ForEach-Object {
  $filePath = $_.FullName
  $fileName = Split-Path $filePath -leaf
  $xml = New-Object XML
  $xml.PreserveWhitespace = $true
  $xml.Load($filePath)
  $xml.Project.PropertyGroup.Version = $version.ToString()
  $xml.Save($filePath)
  Write-Host "Set version of"$fileName" to "$version
}

# commit and push changes
git commit -a -m "NuGet v$($version)"
git tag -a "nuget/v$($version)" -m "nuget/v$($version)"
git push
git push --tags

# build project and publish packages
dotnet pack -c Release -p:Version=$version -p:IncludeSymbols=true -p:SymbolPackageFormat=snupkg
Copy-Item .\source\bin\Release\*.nupkg
Copy-Item .\source\bin\Release\*.snupkg
nuget push *.nupkg -Source "nuget.org"
Remove-Item *.nupkg
Remove-Item *.snupkg
