$sum = @()
git log --oneline | ForEach-Object { 
    if ($_ -match ".*\[([\d.]+).*\].*"){
        $sum += [decimal]$matches[1];
    } else{
        $sum += 1.5
    }
} 
$sum | Measure-Object -average -sum -minimum -maximum

Write-Host -NoNewLine 'Press any key to continue...';
$null = $Host.UI.RawUI.ReadKey('NoEcho,IncludeKeyDown');