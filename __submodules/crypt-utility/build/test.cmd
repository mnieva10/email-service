SETLOCAL

rd TestResults /s /q
md TestResults

robocopy %1\user\login-funcs-dll\ ..\ModelUT\bin LoginFuncs_d32_v1-0-1.dll /ZB /X /TEE 

SET PATH=%PATH%;C:\Program Files (x86)\NUnit 2.6.2\bin
nunit-console-x86.exe^
  ..\ModelUT\bin\ModelUT.dll^
  /xml=TestResults\nunit-result.xml

ENDLOCAL
