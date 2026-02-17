$ErrorActionPreference = 'Stop'

dotnet build src/Perch.Desktop/Perch.Desktop.csproj -c Debug

$id = [System.IO.Path]::GetRandomFileName().Split('.')[0]
$copyDir = "src\Perch.Desktop\bin\Debug\$id"
Copy-Item "src\Perch.Desktop\bin\Debug\net10.0-windows" $copyDir -Recurse

Write-Host "Launching from $copyDir"
Start-Process "$copyDir\Perch.Desktop.exe"
