<#
.SYNOPSIS
    One-time migration: re-hosts every route's photos/files on Firebase
    Storage instead of wherever they're pasted from today (the club's Wix
    site), and updates the routes collection to point at the new URLs.

.DESCRIPTION
    Signs in as the admin user, lists every route, downloads each
    photo/file that isn't already on Storage, re-uploads it to Storage, and
    patches the route's photoUrls/files fields with the new URLs. Safe to
    re-run - anything already pointing at firebasestorage.googleapis.com is
    left alone.

.PARAMETER BucketName
    The project's default Storage bucket name. Depends on exactly how/when
    Storage was enabled for this project - check the Firebase console
    (Storage tab) if the default here doesn't work.

.EXAMPLE
    .\Migrate-BwoasFilesToStorage.ps1
#>

param(
    [string]$BucketName = "bwoas-85868.firebasestorage.app"
)

$ErrorActionPreference = "Stop"

$ProjectId = "bwoas-85868"
$ApiKey = "AIzaSyB2XJJ4soD2BH7AoZw-Umb3Fc3pT6GhjX8"
$BaseUrl = "https://firestore.googleapis.com/v1/projects/$ProjectId/databases/(default)/documents"
$StorageBaseUrl = "https://firebasestorage.googleapis.com/v0/b/$BucketName/o"
$StorageHost = "firebasestorage.googleapis.com"

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
        # Invoke-RestMethod's exception message is just the HTTP status line
        # ("(400) Bad Request") - the actual reason (wrong password, unknown
        # email, disabled account, ...) is in the response body, which we
        # have to read separately.
        $reason = $_.Exception.Message
        if ($_.Exception.Response) {
            try {
                $stream = $_.Exception.Response.GetResponseStream()
                $stream.Position = 0
                $errorBody = (New-Object System.IO.StreamReader($stream)).ReadToEnd() | ConvertFrom-Json
                if ($errorBody.error.message) { $reason = $errorBody.error.message }
            } catch { }
        }
        Write-Error "Sign-in failed: $reason"
        exit 1
    }
}

function ConvertTo-FirestoreString($value) { @{ stringValue = "$value" } }

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
                            fileName    = ConvertTo-FirestoreString $_.fileName
                            url         = ConvertTo-FirestoreString $_.url
                            description = ConvertTo-FirestoreString $_.description
                        }
                    }
                }
            })
        }
    }
}

function Get-MimeType($fileName) {
    switch -Regex ($fileName) {
        '\.jpe?g$' { return 'image/jpeg' }
        '\.png$'   { return 'image/png' }
        '\.gpx$'   { return 'application/gpx+xml' }
        '\.pdf$'   { return 'application/pdf' }
        default    { return 'application/octet-stream' }
    }
}

# Downloads $Url and re-uploads it to Storage, returning the new public URL.
# Already-migrated URLs (pointing at Storage already) are returned as-is -
# this is what makes the script safe to re-run.
function Move-UrlToStorage {
    param([string]$Url, [string]$FileName, [hashtable]$Headers)

    if ($Url -match [regex]::Escape($StorageHost)) {
        return $Url
    }

    $tempPath = Join-Path $env:TEMP "bwoas-migrate-$FileName"
    Write-Host "    Downloading $FileName..."
    Invoke-WebRequest -Uri $Url -OutFile $tempPath -UseBasicParsing
    $bytes = [System.IO.File]::ReadAllBytes($tempPath)
    Remove-Item $tempPath -ErrorAction SilentlyContinue

    $objectName = "route-files/$FileName"
    $encodedName = [uri]::EscapeDataString($objectName)
    $mimeType = Get-MimeType $FileName

    Write-Host "    Uploading to Storage as $objectName ($($bytes.Length) bytes)..."
    $uploadHeaders = $Headers.Clone()
    $uploadHeaders["Content-Type"] = $mimeType
    Invoke-RestMethod -Uri "$StorageBaseUrl`?uploadType=media&name=$encodedName" `
        -Method Post -Headers $uploadHeaders -Body $bytes | Out-Null

    return "https://firebasestorage.googleapis.com/v0/b/$BucketName/o/$encodedName`?alt=media"
}

# --- Main ---

$idToken = Get-IdToken
$headers = @{ Authorization = "Bearer $idToken" }

Write-Host "`nFetching routes..." -ForegroundColor Cyan
$routes = Invoke-RestMethod -Uri "$BaseUrl/routes" -Headers $headers

$migrated = 0
$skipped = 0
$failed = 0

foreach ($doc in $routes.documents) {
    $routeId = ($doc.name -split '/')[-1]
    $routeName = $doc.fields.name.stringValue
    Write-Host "`nRoute $routeId - $routeName" -ForegroundColor Yellow

    $changed = $false

    $photoUrls = @()
    foreach ($v in $doc.fields.photoUrls.arrayValue.values) {
        $url = $v.stringValue
        if ($url -match [regex]::Escape($StorageHost)) {
            $photoUrls += $url
            $skipped++
            continue
        }
        try {
            $fileName = ($url -split '\?')[0].Split('/')[-1]
            $photoUrls += Move-UrlToStorage -Url $url -FileName $fileName -Headers $headers
            $migrated++
        }
        catch {
            Write-Warning "    Failed to migrate photo $url : $($_.Exception.Message)"
            $photoUrls += $url
            $failed++
        }
        $changed = $true
    }

    $files = @()
    foreach ($v in $doc.fields.files.arrayValue.values) {
        $f = $v.mapValue.fields
        $url = $f.url.stringValue
        $fileName = $f.fileName.stringValue
        $description = $f.description.stringValue
        if ($url -match [regex]::Escape($StorageHost)) {
            $files += [pscustomobject]@{ fileName = $fileName; url = $url; description = $description }
            $skipped++
            continue
        }
        try {
            $newUrl = Move-UrlToStorage -Url $url -FileName $fileName -Headers $headers
            $files += [pscustomobject]@{ fileName = $fileName; url = $newUrl; description = $description }
            $migrated++
        }
        catch {
            Write-Warning "    Failed to migrate file $fileName : $($_.Exception.Message)"
            $files += [pscustomobject]@{ fileName = $fileName; url = $url; description = $description }
            $failed++
        }
        $changed = $true
    }

    if (-not $changed) {
        Write-Host "  Nothing to migrate." -ForegroundColor DarkGray
        continue
    }

    $patchBody = @{
        fields = @{
            photoUrls = ConvertTo-FirestoreStringArray $photoUrls
            files     = ConvertTo-FirestoreFilesArray $files
        }
    } | ConvertTo-Json -Depth 20

    Invoke-RestMethod -Uri "$BaseUrl/routes/$routeId`?updateMask.fieldPaths=photoUrls&updateMask.fieldPaths=files" `
        -Method Patch -Headers $headers -Body $patchBody -ContentType "application/json" | Out-Null
    Write-Host "  Saved routes/$routeId" -ForegroundColor Green
}

Write-Host "`n=== Done ===" -ForegroundColor Cyan
Write-Host "Migrated: $migrated   Already on Storage: $skipped   Failed: $failed"
