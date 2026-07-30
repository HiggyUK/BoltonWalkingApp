<#
.SYNOPSIS
    Interactively adds a new walking route to the BWOAS app's Firestore database.

.DESCRIPTION
    Prompts for each field of a route, signs in as the admin user (Firebase
    Authentication - Email/Password), and writes the route as a new document
    in the "routes" collection via the Firestore REST API. Reads are public;
    writes require this sign-in (see firestore.rules).

.EXAMPLE
    .\Add-BwoasRoute.ps1
#>

$ErrorActionPreference = "Stop"

$ProjectId = "bwoas-85868"
$ApiKey = "AIzaSyB2XJJ4soD2BH7AoZw-Umb3Fc3pT6GhjX8"
$BaseUrl = "https://firestore.googleapis.com/v1/projects/$ProjectId/databases/(default)/documents"

function Get-IdToken {
    Write-Host "=== Sign in ===" -ForegroundColor Cyan
    $email = Read-Host "Admin email"
    $securePassword = Read-Host "Password" -AsSecureString
    $bstr = [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($securePassword)
    $password = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto($bstr)

    $body = @{ email = $email; password = $password; returnSecureToken = $true } | ConvertTo-Json
    try {
        $response = Invoke-RestMethod -Uri "https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key=$ApiKey" `
            -Method Post -Body $body -ContentType "application/json"
        return $response.idToken
    }
    catch {
        Write-Error "Sign-in failed: $($_.Exception.Message)"
        exit 1
    }
}

function ConvertTo-FirestoreString($value) { @{ stringValue = "$value" } }
function ConvertTo-FirestoreNumber($value) { @{ doubleValue = [double]$value } }

function ConvertTo-FirestoreStringArray($items) {
    @{ arrayValue = @{ values = @($items | ForEach-Object { ConvertTo-FirestoreString $_ }) } }
}

function ConvertTo-FirestoreFilesArray($items) {
    @{
        arrayValue = @{
            values = @($items | ForEach-Object {
                @{
                    mapValue = @{
                        fields = @{
                            fileName    = ConvertTo-FirestoreString $_.FileName
                            url         = ConvertTo-FirestoreString $_.Url
                            description = ConvertTo-FirestoreString $_.Description
                        }
                    }
                }
            })
        }
    }
}

function Get-NextRouteId {
    param($Headers)
    $existing = Invoke-RestMethod -Uri "$BaseUrl/routes" -Headers $Headers
    $maxId = 0
    if ($existing.documents) {
        foreach ($doc in $existing.documents) {
            $id = 0
            if ([int]::TryParse(($doc.name -split '/')[-1], [ref]$id) -and $id -gt $maxId) {
                $maxId = $id
            }
        }
    }
    return $maxId + 1
}

function Read-RouteFromPrompts {
    Write-Host "`n=== New route ===" -ForegroundColor Cyan
    $route = [ordered]@{
        Name              = Read-Host "Route name"
        Latitude          = Read-Host "Latitude (e.g. 53.6006)"
        Longitude         = Read-Host "Longitude (e.g. -2.5495)"
        Difficulty        = Read-Host "Difficulty (Easy / Moderate / Hard)"
        DifficultyBadge   = Read-Host "Difficulty badge (e.g. a green/orange/red circle emoji)"
        Stats             = Read-Host "Stats line (e.g. '7km - 200m ascent - 2 hours')"
        ShortDescription  = Read-Host "Short description (shown on the map popup)"
        TerrainNotes      = Read-Host "Terrain notes"
        Venue             = Read-Host "Venue / starting pub"
        Address           = Read-Host "Address"
        What3Words        = Read-Host "what3words (word.word.word)"
        GridReference     = Read-Host "Grid reference (e.g. SD635118)"
        TransportNotes    = Read-Host "Transport notes"
        ParkingNotes      = Read-Host "Parking notes"
    }

    $photoUrls = @()
    Write-Host "`nPhoto URLs - paste one at a time, blank line to finish:"
    while ($true) {
        $url = Read-Host "  Photo URL"
        if ([string]::IsNullOrWhiteSpace($url)) { break }
        $photoUrls += $url
    }
    $route["PhotoUrls"] = $photoUrls

    $files = @()
    Write-Host "`nDownloadable files (GPX/PDF) - blank file name to finish:"
    while ($true) {
        $fileName = Read-Host "  File name (e.g. my-route.gpx)"
        if ([string]::IsNullOrWhiteSpace($fileName)) { break }
        $url = Read-Host "  File URL"
        $description = Read-Host "  Description (e.g. 'GPX track')"
        $files += [pscustomobject]@{ FileName = $fileName; Url = $url; Description = $description }
    }
    $route["Files"] = $files

    return $route
}

# --- Main ---

$idToken = Get-IdToken
$headers = @{ Authorization = "Bearer $idToken" }

$nextId = Get-NextRouteId -Headers $headers
Write-Host "`nThis will be saved as routes/$nextId" -ForegroundColor Yellow

$route = Read-RouteFromPrompts

$fields = @{
    name             = ConvertTo-FirestoreString $route.Name
    latitude         = ConvertTo-FirestoreNumber $route.Latitude
    longitude        = ConvertTo-FirestoreNumber $route.Longitude
    difficulty       = ConvertTo-FirestoreString $route.Difficulty
    difficultyBadge  = ConvertTo-FirestoreString $route.DifficultyBadge
    shortDescription = ConvertTo-FirestoreString $route.ShortDescription
    stats            = ConvertTo-FirestoreString $route.Stats
    terrainNotes     = ConvertTo-FirestoreString $route.TerrainNotes
    venue            = ConvertTo-FirestoreString $route.Venue
    address          = ConvertTo-FirestoreString $route.Address
    what3Words       = ConvertTo-FirestoreString $route.What3Words
    gridReference    = ConvertTo-FirestoreString $route.GridReference
    transportNotes   = ConvertTo-FirestoreString $route.TransportNotes
    parkingNotes     = ConvertTo-FirestoreString $route.ParkingNotes
    photoUrls        = ConvertTo-FirestoreStringArray $route.PhotoUrls
    files            = ConvertTo-FirestoreFilesArray $route.Files
}

$body = @{ fields = $fields } | ConvertTo-Json -Depth 20

Write-Host "`nSaving..." -ForegroundColor Cyan
try {
    Invoke-RestMethod -Uri "$BaseUrl/routes`?documentId=$nextId" -Method Post -Headers $headers -Body $body -ContentType "application/json" | Out-Null
    Write-Host "Saved: routes/$nextId - $($route.Name)" -ForegroundColor Green
}
catch {
    Write-Error "Failed to save route: $($_.Exception.Message)"
    exit 1
}
