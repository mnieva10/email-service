@ECHO OFF

if [%MAJOR_NUMBER%]==[] SET MAJOR_NUMBER="2099"
if [%MINOR_NUMBER%]==[] SET MINOR_NUMBER="99"
if [%RELEASE_NUMBER%]==[] SET RELEASE_NUMBER="99"
if [%BUILD_NUMBER%]==[] SET BUILD_NUMBER="99"

set versionfile=..\AssemblyVersionInfo.cs
set replacestr=%MAJOR_NUMBER%.%MINOR_NUMBER%.%RELEASE_NUMBER%.%BUILD_NUMBER%
powershell -Command "& {(Get-Content %versionfile%) -replace '[0-9]*[.][0-9]*[.][0-9]*[.][0-9]*', '%replacestr%' | Set-Content %versionfile%}"
