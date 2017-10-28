@echo off
if %%1 == "" (
  goto :help
)
if %%2 == "" (
  goto :help
)
if %%3 == "" (
  goto :help
)
if %%4 == "" (
  goto :help
)
if %%5 == "" (
  goto :help
)
if %%6 == "" (
  goto :help
)

powershell -Command "& {(Get-Content ..\Model\Services\LoginUtility.cs) -replace '%1-%2-%3', '%4-%5-%6' | Set-Content ..\Model\Services\LoginUtility.cs}"

goto :end

:help

echo --
echo syntax: bump_version CurrentMajor CurrentMinor CurrentRelease NewMajor NewMinor NewRelease
echo --

:end