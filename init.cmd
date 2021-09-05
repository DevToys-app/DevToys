@set PS1UnderCmd=1
powershell.exe -ExecutionPolicy bypass -Command "& '%~dpn0.ps1'" %*
@set PS1UnderCmd=
