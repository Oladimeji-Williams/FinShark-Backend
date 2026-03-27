# FinShark health-check script
# Run from repo root: .\health-check.ps1

$apiBase = "https://localhost:7235/api"
$authBase = "$apiBase/auth"
$random = Get-Random -Maximum 99999
$email = "healthcheck+$random@example.com"
$password = "Test123!"

Write-Host "[0] Checking API health"
$health = Invoke-RestMethod -Uri "$apiBase/health" -Method Get -SkipCertificateCheck
Write-Host " Health result: $($health.message)"

Write-Host "[1] Register user $email"
$register = Invoke-RestMethod `
    -Uri "$authBase/register" `
    -Method Post `
    -Body (@{
        Email = $email
        Password = $password
        UserName = "hcuser$random"
        FirstName = "Health"
        LastName = "Check"
    } | ConvertTo-Json) `
    -ContentType application/json `
    -SkipCertificateCheck
Write-Host " Register result: $($register.message)"

Write-Host "[2] Login before email confirmation"
try {
    $loginResponse = Invoke-RestMethod `
        -Uri "$authBase/login" `
        -Method Post `
        -Body (@{ Email = $email; Password = $password } | ConvertTo-Json) `
        -ContentType application/json `
        -SkipCertificateCheck `
        -ErrorAction Stop

    if ($loginResponse.success) {
        Write-Host "Unexpected login success before email confirmation" -ForegroundColor Yellow
    }
}
catch {
    Write-Host " Expected login failure before email confirmation"
}

Write-Host "[3] Resend confirmation email"
$resend = Invoke-RestMethod `
    -Uri "$authBase/resend-confirmation" `
    -Method Post `
    -Body (@{ Email = $email } | ConvertTo-Json) `
    -ContentType application/json `
    -SkipCertificateCheck
Write-Host " Resend result: $($resend.message)"

Write-Host "[4] Validate confirm-email endpoint with invalid token"
$userId = $register.data.user.id
try {
    $confirm = Invoke-RestMethod `
        -Uri "$authBase/confirm-email?userId=$userId&token=invalid-token" `
        -Method Get `
        -SkipCertificateCheck `
        -ErrorAction Stop

    if ($confirm.success) {
        Write-Host "Unexpected confirmation success with invalid token" -ForegroundColor Yellow
    }
}
catch {
    Write-Host " Confirm endpoint rejected invalid token as expected"
}

Write-Host "Health-check complete."
