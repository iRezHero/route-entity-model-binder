
$PROJ_NAME = "EntityModelBinder"

# === Prompt user ===
# Prompt for version (non-empty)
do {
    $Version = Read-Host 'Enter the package version'
    if ([string]::IsNullOrWhiteSpace($Version)) {
        Write-Host 'Package version cannot be empty. Please enter a version (e.g. 1.0.0).' -ForegroundColor Yellow
    }
} while ([string]::IsNullOrWhiteSpace($Version))

#Prompt for GitHub username
do {
    $gitHubUsername = Read-Host 'Enter your GitHub username'
    if ([string]::IsNullOrWhiteSpace($gitHubUsername)) {
        Write-Host 'GitHub username cannot be empty. Please enter your username.' -ForegroundColor Yellow
    }
} while ([string]::IsNullOrWhiteSpace($gitHubUsername))

# Prompt for token as secure string
do {
    $secureToken = Read-Host 'Enter your GitHub Personal Access Token' -AsSecureString
    if ([string]::IsNullOrWhiteSpace($secureToken)) {
        Write-Host 'GitHub Personal Access Token cannot be empty. Please enter your token.' -ForegroundColor Yellow
    }
} while ([string]::IsNullOrWhiteSpace($secureToken))

# Convert secure token to plain (only while invoking dotnet). We'll clear it afterwards.
$gitHubApiKeyPlain = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto([System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($secureToken))
do {
    $nugetKey = Read-Host 'Enter your NuGet API Key' -AsSecureString
    if ([string]::IsNullOrWhiteSpace($nugetKey)) {
        Write-Host 'NuGet API Key cannot be empty. Please enter your key.' -ForegroundColor Yellow
    }
} while ([string]::IsNullOrWhiteSpace($nugetKey))

$nugetKeyPlain = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto([System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($nugetKey))

# Configurazione - Modifica queste variabili
$NUPKG_PATH = "bin/Release/$PROJ_NAME.$Version.nupkg"
$GITHUB_SOURCE = "https://nuget.pkg.github.com/${gitHubUsername}/index.json"

# 1. Build e Pack
Write-Host "📦 Creazione del pacchetto..." -ForegroundColor Cyan
dotnet pack -c Release /p:PackageVersion=$Version

# 2. Push su NuGet.org
Write-Host "🚀 Pubblicazione su NuGet.org..." -ForegroundColor Magenta
dotnet nuget push $NUPKG_PATH --api-key $nugetKeyPlain --source https://api.nuget.org/v3/index.json

# 3. Push su GitHub Packages
Write-Host "🚀 Pubblicazione su GitHub..." -ForegroundColor Blue
dotnet nuget push $NUPKG_PATH --api-key $gitHubApiKeyPlain --source $GITHUB_SOURCE