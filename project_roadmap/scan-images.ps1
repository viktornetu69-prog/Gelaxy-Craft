# scan-images.ps1
# Автоматически сканирует все папки references/*/images/
# и генерирует manifest.json с реальными файлами картинок

$basePath = Join-Path $PSScriptRoot "references"

# Форматы файлов которые поддерживаем
$allowedExtensions = @(".jpg", ".jpeg", ".png", ".gif", ".webp", ".bmp", ".svg", ".tga", ".ico")

Write-Host "🔍 Сканирование папок изображений..." -ForegroundColor Cyan
Write-Host "==================================" -ForegroundColor Cyan

$scanResults = @()

# Получаем все подпапки с images
$imagesFolders = Get-ChildItem -Path $basePath -Directory | ForEach-Object {
    Get-ChildItem -Path (Join-Path $_.FullName "images") -File -ErrorAction SilentlyContinue | Where-Object {
        $allowedExtensions -contains $_.Extension.ToLower()
    } | Select-Object -ExpandProperty Name
}

# Группируем по папкам
$gameFolders = Get-ChildItem -Path $basePath -Directory
foreach ($gameFolder in $gameFolders) {
    $imagesPath = Join-Path $gameFolder.FullName "images"
    if (Test-Path $imagesPath) {
        # Получаем все картинки в папке
        $files = Get-ChildItem -Path $imagesPath -File | Where-Object {
            $allowedExtensions -contains $_.Extension.ToLower()
        }
        
        $manifestFiles = @()
        foreach ($file in $files) {
            $manifestFiles += $file.Name
            $scanResults += [PSCustomObject]@{
                Game = $gameFolder.Name
                File = $file.Name
                Size = "{0:N2} KB" -f ($file.Length / 1KB)
            }
        }
        
        # Генерируем manifest.json
        $manifestPath = Join-Path $imagesPath "manifest.json"
        $jsonContent = $manifestFiles | ConvertTo-Json
        
        # Сохраняем manifest.json
        $jsonContent | Out-File -FilePath $manifestPath -Encoding utf8
    }
}

# Показываем результат
if ($scanResults.Count -gt 0) {
    Write-Host ""
    Write-Host "✅ Найдено $($scanResults.Count) изображений:" -ForegroundColor Green
    
    $scanResults | Format-Table -AutoSize
    
    Write-Host ""
    Write-Host "📁 Обновлено manifest.json для:" -ForegroundColor Yellow
    $scanResults | Select-Object -Property Game -Unique | ForEach-Object {
        Write-Host "   ✨ $($_.Game)" -ForegroundColor Green
    }
    
    Write-Host ""
    Write-Host "🔄 Обновите страницы референсов в браузере (Ctrl+F5)" -ForegroundColor Cyan
} else {
    Write-Host "⚠️ Изображения не найдены." -ForegroundColor Yellow
}

Write-Host ""
Write-Host "Нажмите Enter для выхода..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
