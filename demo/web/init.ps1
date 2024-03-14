Import-Module Oraculum
Connect-Oraculum -ConfigFile "$(pwd)\oraculum.secret.conf"
Reset-Schema
$json = Get-Content ../OraculumLocalBox/qamoviedb.json | ConvertFrom-Json
$n = 1
$json |% { Write-Host "Adding Fact #$n";Add-Fact -Category movie -FactType faq -Title $_.q -Content $_.a -Citation ("FAQ#" + $n++); }