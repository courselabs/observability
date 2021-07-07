function Copy-LinuxStyle {
	param (	
		[Parameter(Position=0)]
		[switch] $f,
		[Parameter(Position=1)]
		[string] $source,
		[Parameter(Position=2)]
		[string] $target

	)
	if ($f){
		Copy-Item -Force -Path $source -Destination $target
	}
	else {
		Copy-Item -Path $source -Destination $target
	}
}

# Linux-style cp
Remove-Item Alias:cp -ErrorAction Ignore
Set-Alias cp Copy-LinuxStyle

# grep
Set-Alias -Name grep -Value Select-String

# use the real curl
Remove-Item Alias:curl -ErrorAction Ignore
