$ErrorActionPreference = "Stop"

[xml]$xml = Get-Content -Path "$PSScriptRoot\Directory.Build.Props"
$version = $xml.Project.PropertyGroup.Version

foreach ($platform in "ARM64", "x64")
{
    if (Test-Path -Path "$PSScriptRoot\Community.PowerToys.Run.Plugin.BrowserFavorite\bin")
    {
        Remove-Item -Path "$PSScriptRoot\Community.PowerToys.Run.Plugin.BrowserFavorite\bin\*" -Recurse
    }

    dotnet build $PSScriptRoot\Community.PowerToys.Run.Plugin.BrowserFavorite.sln -c Release /p:Platform=$platform

    Remove-Item -Path "$PSScriptRoot\Community.PowerToys.Run.Plugin.BrowserFavorite\bin\*" -Recurse -Include *.xml, *.pdb, PowerToys.*, Wox.*
    Rename-Item -Path "$PSScriptRoot\Community.PowerToys.Run.Plugin.BrowserFavorite\bin\$platform\Release" -NewName "BrowserFavorite"
    Copy-Item "$PSScriptRoot\Community.PowerToys.Run.Plugin.BrowserFavorite\Libs\$platform\Microsoft.Data.Sqlite.dll" -Destination "$PSScriptRoot\Community.PowerToys.Run.Plugin.BrowserFavorite\bin\$platform\BrowserFavorite"

    # Define the destination zip file path
    $zipPath = "$PSScriptRoot\BrowserFavorite-$version-$platform.zip"
    
    # Check if the zip file already exists, and remove it if it does
    if (Test-Path -Path $zipPath)
    {
       Remove-Item -Path $zipPath -Force
    }
    
    # Create the ZIP file
    Compress-Archive -Path "$PSScriptRoot\Community.PowerToys.Run.Plugin.BrowserFavorite\bin\$platform\BrowserFavorite" -DestinationPath $zipPath
}
