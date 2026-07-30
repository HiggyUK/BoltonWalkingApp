<#
.SYNOPSIS
    Interactively adds a scheduled walk (event) to the BWOAS app's Firestore database.

.DESCRIPTION
    Lists existing routes so you can pick one by id, prompts for the date/time
    and Ticket Tailor link, signs in as the admin user (Firebase Authentication
    - Email/Password), and writes the event as a new document in the "events"
    collection via the Firestore REST API.

.EXAMPLE
    .\Add-BwoasEvent.ps1
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

# Club policy: Monday walks open for booking the prior Thursday at 18:00;
# Wednesday walks open the prior Sunday at 18:00. Other days have no
# standard rule, so this returns $null and the admin enters one manually.
function Get-SuggestedBookingOpensAt([datetime]$walkDate) {
    switch ($walkDate.DayOfWeek) {
        'Monday' { return $walkDate.Date.AddDays(-4).AddHours(18) }
        'Wednesday' { return $walkDate.Date.AddDays(-3).AddHours(18) }
        default { return $null }
    }
}

function Get-NextDocumentId {
    param($Headers, $Collection)
    $existing = Invoke-RestMethod -Uri "$BaseUrl/$Collection" -Headers $Headers
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

# --- Main ---

$idToken = Get-IdToken
$headers = @{ Authorization = "Bearer $idToken" }

Write-Host "`nFetching routes..." -ForegroundColor Cyan
$routesResponse = Invoke-RestMethod -Uri "$BaseUrl/routes" -Headers $headers
$routes = @()
foreach ($doc in $routesResponse.documents) {
    $id = ($doc.name -split '/')[-1]
    $name = $doc.fields.name.stringValue
    $routes += [pscustomobject]@{ Id = [int]$id; Name = $name }
}
$routes = $routes | Sort-Object Id

Write-Host "`n=== Routes ===" -ForegroundColor Cyan
foreach ($r in $routes) { Write-Host ("  {0,3}  {1}" -f $r.Id, $r.Name) }

Write-Host "`n=== New event ===" -ForegroundColor Cyan
$routeId = 0
while ($true) {
    $routeIdInput = Read-Host "Route id (from the list above)"
    if ([int]::TryParse($routeIdInput, [ref]$routeId) -and ($routes.Id -contains $routeId)) { break }
    Write-Host "  Not a valid route id - try again." -ForegroundColor Yellow
}

$date = Read-Host "Date (YYYY-MM-DD)"
$startTime = Read-Host "Start time (HH:mm, 24-hour)"
$endTime = Read-Host "End time (HH:mm, 24-hour)"
$ticketLink = Read-Host "Ticket Tailor link"

$startDateTime = $null
$endDateTime = $null
if (-not [datetime]::TryParse("$date $startTime", [ref]$startDateTime)) {
    Write-Error "Could not parse date/start time."
    exit 1
}
if (-not [datetime]::TryParse("$date $endTime", [ref]$endDateTime)) {
    Write-Error "Could not parse date/end time."
    exit 1
}

# The app itself applies the Monday/Wednesday rule automatically whenever
# this field is left as the "not set" sentinel - so the normal path here is
# just to accept that default. Only type a value if this walk needs a
# different opening time than usual (an override).
$suggested = Get-SuggestedBookingOpensAt -walkDate $startDateTime
if ($suggested) {
    Write-Host "`n$($startDateTime.DayOfWeek) walk - the app will automatically open booking $($suggested.ToString('ddd d MMM, HH:mm'))." -ForegroundColor Cyan
} else {
    Write-Host "`n$($startDateTime.DayOfWeek) doesn't have a standard booking-opens rule - this walk will be bookable immediately unless you set a time." -ForegroundColor Yellow
}
$bookingOpensInput = Read-Host "Override booking-opens time? (YYYY-MM-DD HH:mm, blank to use the automatic rule above)"

$bookingOpensAt = [datetime]::MinValue
if (-not [string]::IsNullOrWhiteSpace($bookingOpensInput) -and -not [datetime]::TryParse($bookingOpensInput, [ref]$bookingOpensAt)) {
    Write-Error "Could not parse booking-opens date/time."
    exit 1
}

$nextId = Get-NextDocumentId -Headers $headers -Collection "events"
Write-Host "`nThis will be saved as events/$nextId" -ForegroundColor Yellow

$fields = @{
    routeId        = ConvertTo-FirestoreNumber $routeId
    startDateTime  = ConvertTo-FirestoreString $startDateTime.ToString("yyyy-MM-ddTHH:mm:ss")
    endDateTime    = ConvertTo-FirestoreString $endDateTime.ToString("yyyy-MM-ddTHH:mm:ss")
    ticketLink     = ConvertTo-FirestoreString $ticketLink
    bookingOpensAt = ConvertTo-FirestoreString $bookingOpensAt.ToString("yyyy-MM-ddTHH:mm:ss")
}

$body = @{ fields = $fields } | ConvertTo-Json -Depth 10

Write-Host "`nSaving..." -ForegroundColor Cyan
try {
    Invoke-RestMethod -Uri "$BaseUrl/events`?documentId=$nextId" -Method Post -Headers $headers -Body $body -ContentType "application/json" | Out-Null
    $routeName = ($routes | Where-Object { $_.Id -eq $routeId }).Name
    $bookingNote = if ($bookingOpensAt -ne [datetime]::MinValue) {
        "booking opens $($bookingOpensAt.ToString('ddd d MMM, HH:mm')) (override)"
    } elseif ($suggested) {
        "booking opens $($suggested.ToString('ddd d MMM, HH:mm')) (automatic rule)"
    } else {
        "bookable immediately"
    }
    Write-Host "Saved: events/$nextId - $routeName on $($startDateTime.ToString('ddd d MMM')), $($startDateTime.ToString('HH:mm'))-$($endDateTime.ToString('HH:mm')) - $bookingNote" -ForegroundColor Green
}
catch {
    Write-Error "Failed to save event: $($_.Exception.Message)"
    exit 1
}
