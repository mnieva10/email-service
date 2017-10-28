@echo off

set loginSource=\\conveydev.com\files\dev\sdlc-ci\development\user\login-funcs-dll\
set login_version=v1-0-2

robocopy %loginSource% ..\bin\Win32\Debug LoginFuncs_d32_%login_version%.dll /ZB /X /TEE
robocopy %loginSource% ..\bin\Win32\Release LoginFuncs_r32_%login_version%.dll /ZB /X /TEE
robocopy %loginSource% ..\ModelUT\bin LoginFuncs_d32_%login_version%.dll /ZB /X /TEE 
robocopy %loginSource% ..\PersistenceUT\bin LoginFuncs_d32_%login_version%.dll /ZB /X /TEE 
robocopy %loginSource% ..\<%= test %>\bin LoginFuncs_d32_%login_version%.dll /ZB /X /TEE
robocopy %loginSource% ..\<%= test %>\bin LoginFuncs_r32_%login_version%.dll /ZB /X /TEE

call ../__submodules/service-bus-csharp/get d32 "..\bin\Win32\Debug"
call ../__submodules/service-bus-csharp/get r32 "..\bin\Win32\Release"
call ../__submodules/service-bus-csharp/get d32 "..\<%= test %>\bin"
call ../__submodules/service-bus-csharp/get r32 "..\<%= test %>\bin"

EXIT /B 0
