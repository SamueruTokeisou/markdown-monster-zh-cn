param(
    [Parameter(Mandatory=$true)][string]$MarkdownMonsterPath,
    [string]$DotnetRoot = "$env:ProgramFiles\dotnet"
)
$ErrorActionPreference = 'Stop'
if ($PSVersionTable.PSEdition -ne 'Core') { throw 'Run this script with PowerShell 7 (pwsh), not Windows PowerShell 5.1.' }
$root = Split-Path -Parent $PSScriptRoot
$assemblyPath = Join-Path $MarkdownMonsterPath 'MarkdownMonster.dll'
if (!(Test-Path -LiteralPath $assemblyPath)) { throw "MarkdownMonster.dll not found: $assemblyPath" }
# PowerShell 7 supplies Roslyn; no Markdown Monster assemblies are redistributed.
$references = [System.Collections.Generic.List[Microsoft.CodeAnalysis.MetadataReference]]::new()
foreach ($framework in @('Microsoft.NETCore.App', 'Microsoft.WindowsDesktop.App')) {
    $directory = Get-ChildItem -LiteralPath (Join-Path $DotnetRoot "shared\$framework") -Directory |
        Where-Object Name -Match '^11\.0\.\d+$' | Sort-Object { [version]$_.Name } -Descending | Select-Object -First 1
    if (!$directory) { throw "A .NET 11 runtime for $framework is required." }
    foreach ($file in Get-ChildItem -LiteralPath $directory.FullName -Filter *.dll) {
        try { [Reflection.AssemblyName]::GetAssemblyName($file.FullName) | Out-Null } catch { continue }
        $references.Add([Microsoft.CodeAnalysis.MetadataReference]::CreateFromFile($file.FullName))
    }
}
$references.Add([Microsoft.CodeAnalysis.MetadataReference]::CreateFromFile($assemblyPath))
$source = [IO.File]::ReadAllText((Join-Path $root 'src\ChineseAddin.cs'))
$tree = [Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree]::ParseText($source)
$options = [Microsoft.CodeAnalysis.CSharp.CSharpCompilationOptions]::new([Microsoft.CodeAnalysis.OutputKind]::DynamicallyLinkedLibrary)
$compilation = [Microsoft.CodeAnalysis.CSharp.CSharpCompilation]::Create('ChineseAddin', [Microsoft.CodeAnalysis.SyntaxTree[]]@($tree), $references, $options)
$output = Join-Path $root 'dist\ChineseLocalization'
New-Item -ItemType Directory -Path $output -Force | Out-Null
$stream = [IO.MemoryStream]::new()
try {
    $result = $compilation.Emit($stream)
    $result.Diagnostics | Where-Object Severity -in @('Warning','Error') | ForEach-Object { Write-Output $_.ToString() }
    if (!$result.Success) { throw 'Compilation failed.' }
    [IO.File]::WriteAllBytes((Join-Path $output 'ChineseAddin.dll'), $stream.ToArray())
} finally { $stream.Dispose() }
Copy-Item -LiteralPath (Join-Path $root 'translations.tsv'),(Join-Path $root 'version.json'),(Join-Path $root 'LICENSE'),(Join-Path $root 'README.md') -Destination $output -Force
$zipPath = Join-Path $root 'dist\markdown-monster-zh-cn-v0.1.0.zip'
Compress-Archive -Path $output -DestinationPath $zipPath -Force
Get-FileHash -LiteralPath $zipPath -Algorithm SHA256 | ForEach-Object { "$($_.Hash.ToLowerInvariant())  $([IO.Path]::GetFileName($zipPath))" } | Set-Content (Join-Path $root 'dist\SHA256SUMS.txt') -Encoding ascii
Write-Output "Built: $zipPath"
