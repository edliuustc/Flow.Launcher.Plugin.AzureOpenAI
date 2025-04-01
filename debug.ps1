dotnet publish Flow.Launcher.Plugin.AzureOpenAI -c Debug -r win-x64 --no-self-contained

$AppDataFolder = [Environment]::GetFolderPath("ApplicationData")
$flowLauncherExe = "$env:LOCALAPPDATA\FlowLauncher\Flow.Launcher.exe"

if (Test-Path $flowLauncherExe) {
    Stop-Process -Name "Flow.Launcher" -Force -ErrorAction SilentlyContinue
    Start-Sleep -Seconds 2

    if (Test-Path "$AppDataFolder\FlowLauncher\Plugins\AzureOpenAI") {
        Remove-Item -Recurse -Force "$AppDataFolder\FlowLauncher\Plugins\AzureOpenAI"
    }

    # Define the source and destination paths
    $SourcePath = "Flow.Launcher.Plugin.AzureOpenAI\bin\Debug\net7.0-windows\win-x64\publish"
    $DestinationPath = "$AppDataFolder\FlowLauncher\Plugins\AzureOpenAI"

    # Copy the compiled plugin files to the destination folder
    Copy-Item $SourcePath $DestinationPath -Recurse -Force

    # Copy the Plugin.json file to the destination folder
    Copy-Item "Flow.Launcher.Plugin.AzureOpenAI\Plugin.json" $DestinationPath -Force

    # Rename the folder if necessary
    if (Test-Path "$AppDataFolder\FlowLauncher\Plugins\publish") {
        Rename-Item -Path "$AppDataFolder\FlowLauncher\Plugins\publish" -NewName "AzureOpenAI"
    }

    Start-Sleep -Seconds 2
    Start-Process $flowLauncherExe
} else {
    Write-Host "Flow.Launcher.exe not found. Please install Flow Launcher first"
}