@echo off

set loginSource=\\conveydev.com\files\dev\sdlc-ci\development\user\login-funcs-dll\
set login_version=v1-0-1
set CURHOMEDIR=%CD%

robocopy %loginSource% ..\bin\Win32\Debug LoginFuncs_d32_%login_version%.dll /ZB /X /TEE
robocopy %loginSource% ..\bin\Win32\Release LoginFuncs_r32_%login_version%.dll /ZB /X /TEE
robocopy %loginSource% ..\ModelUT\bin LoginFuncs_d32_%login_version%.dll /ZB /X /TEE 

EXIT /B 0



