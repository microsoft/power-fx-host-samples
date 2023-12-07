$buildPath = ".\PowerFxDemoLibrary\bin\Debug"
$dest = ".\references"

copy-item -path $buildPath\* -destination $dest -include *.dll
copy-item -path $buildPath\en-US\* -destination $dest -include *.dll