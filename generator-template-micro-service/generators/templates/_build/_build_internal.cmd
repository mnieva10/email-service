SETLOCAL

rem SET PATH=%PATH%;C:\Windows\Microsoft.NET\Framework\v4.0.30319
call "%VS100COMNTOOLS%"\\vsvars32.bat
msbuild ..\<%= name %>.sln /p:Configuration=%1 /p:Platform=%2

ENDLOCAL

EXIT /B 0