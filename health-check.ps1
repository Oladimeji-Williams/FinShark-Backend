# Health-check script for FinShark auth endpoints
# Run from repo root: .\health-check.ps1

$baseUrl = "https://localhost:5001/api/auth"
$random = Get-Random -Maximum 99999
$email = "healthcheck+$random@example.com"
$password = "Test123!"

Write-Host "[1] Register user $email"
$register = Invoke-RestMethod -Uri "$baseUrl/register" -Method Post -Body (@{ Email = $email; Password = $password; UserName = "hcuser$random"; FirstName = "Health"; LastName = "Check" } | ConvertTo-Json) -ContentType application/json -SkipCertificateCheck
Write-Host " Register result: $($register.message)"

Write-Host "[2] Login (should fail because email not confirmed)"
$loginResponse = Invoke-RestMethod -Uri "$baseUrl/login" -Method Post -Body (@{ Email = $email; Password = $password } | ConvertTo-Json) -ContentType application/json -SkipCertificateCheck -ErrorAction SilentlyContinue
if ($loginResponse -and $loginResponse.success) {
    Write-Host "Unexpected login success before email confirm" -ForegroundColor Yellow
} else {
    Write-Host "Expected login failure (unconfirmed): $($loginResponse.message)"
}

Write-Host "[3] Resend confirmation"
$resend = Invoke-RestMethod -Uri "$baseUrl/resend-confirmation" -Method Post -Body (@{ Email = $email } | ConvertTo-Json) -ContentType application/json -SkipCertificateCheck
Write-Host " Resend result: $($resend.message)"

Write-Host "[4] Validate confirm-email endpoint with invalid token"
$dummyUserId = $register.data.user.id
$dummyToken = "invalid-token"
$confirm = Invoke-RestMethod -Uri "$baseUrl/confirm-email?userId=$dummyUserId&token=$dummyToken" -Method Get -SkipCertificateCheck -ErrorAction SilentlyContinue
if ($confirm -and $confirm.success) {
    Write-Host "Unexpected success with invalid token" -ForegroundColor Yellow
} else {
    Write-Host "Confirm endpoint responded (expected failure): $($confirm.message)"
}

Write-Host "Health-check complete. For full email confirmation flow, use token from sent email link."
