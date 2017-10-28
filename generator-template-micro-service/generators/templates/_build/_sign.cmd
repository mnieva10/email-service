set workspace=%~dp0
pushd X:\__Dev\__Source\build-toolbox.development\CodeSign

rem call SignIt_signtool.bat %workspace%..\bin\[...]\Sovos.<%= namespace %>.Model.dll
rem call SignIt_signtool.bat %workspace%..\bin\[...]\Sovos.<%= namespace %>.Persistence.dll
rem call SignIt_signtool.bat %workspace%..\bin\[...]\<%= dispatch %>.exe

popd
