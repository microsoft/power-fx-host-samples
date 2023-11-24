$AOSMetadataPath = "K:\AOSService\PackagesLocalDirectory"

If(!(Test-Path -Path $AOSMetadataPath))
{
      $AOSMetadataPath = "C:\AOSService\PackagesLocalDirectory"
}

$RepoPath = "."
$RepoMetadataPath = $RepoPath + "\Metadata"
$RepoModelFolders = Get-ChildItem $RepoMetadataPath -Directory
foreach ($ModelFolder in $RepoModelFolders)
{
	$Target = "$RepoMetadataPath\$ModelFolder"
	New-Item -ItemType SymbolicLink -Path "$AOSMetadataPath" -Name "$ModelFolder" -Value "$Target"
}