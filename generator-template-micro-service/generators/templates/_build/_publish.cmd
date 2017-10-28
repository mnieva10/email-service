@ECHO OFF

ECHO ### Publishing <%= name %> ###

SET VERSION=%MAJOR_NUMBER%.%MINOR_NUMBER%.%RELEASE_NUMBER%.%BUILD_NUMBER%
IF [%BRANCH%]==[development] (
  SET VERSION=latest
)
SET PRODUCT_ARTIFACT_DIR=\\conveydev.com\files\dev\sdlc-ci\artifacts\<%= name %>

SET TARGET_DIR=%PRODUCT_ARTIFACT_DIR%\%VERSION%
CALL :COPY
IF ERRORLEVEL 4 exit /B 1

EXIT /B 0

:COPY
robocopy  ..\bin\Win32\Release\ %TARGET_DIR% /PURGE /ZB /X /TEE /e /xf *.pdb *.manifest *vshost* App.config
IF ERRORLEVEL 4 EXIT /B 1
robocopy  ..\bin\Win32\Release\config %TARGET_DIR%\config *.config /ZB /X /TEE /xf App.config
IF ERRORLEVEL 4 EXIT /B 1
robocopy ..\<%= dispatch %> %TARGET_DIR% <%= dispatch %>.exe.config.erb /ZB /X /TEE
IF ERRORLEVEL 4 EXIT /B 1
robocopy ..\configuration %TARGET_DIR% <%= dispatch %>.cjson.erb /ZB /X /TEE
IF ERRORLEVEL 4 EXIT /B 1
robocopy ..\PersistenceUT\config %TARGET_DIR%\config properties.config.erb /ZB /X /TEE
IF ERRORLEVEL 4 EXIT /B 1
robocopy ..\release %TARGET_DIR% *.* /E /ZB /X /TEE
IF ERRORLEVEL 4 exit /B 1

GOTO :EOF