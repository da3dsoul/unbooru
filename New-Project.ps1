if ($args.Length -lt 1) {
    Write-Host "Please Specify a New Module Name in the first argument"
    return
}

$replacePattern = "Template"
$newName = $args[0]
$destPath = [System.IO.Path]::Combine((Get-ChildItem -Path $cwd | Select-Object -First 1).Parent.FullName, $newName)

Get-ChildItem -Path $cwd | ForEach-Object {
    if ($_.Name -eq $replacePattern) {
        [void](mkdir $newName)
        $_.GetFileSystemInfos() | ForEach-Object {
            Copy-Item $_.FullName $newName
        }
    }
}

Get-ChildItem -Path $destPath | ForEach-Object {
    if ($_.Name.Contains($replacePattern)) {
        $newFileName = $_.Name.Replace($replacePattern, $newName)
        $newPath = [System.IO.Path]::Combine($destPath, $newFileName)

        $_.MoveTo($newPath)
    }
}

Get-ChildItem -Path $destPath -File | ForEach-Object {
    $content = [System.IO.File]::ReadAllText($_.FullName)
    if ($content.Contains($replacePattern)) {
        $content = $content.Replace($replacePattern, $newName)
        [System.IO.File]::WriteAllText($_.FullName, $content)
    }
}
