rd TestResults /s /q
md TestResults

nunit-console-x86.exe ..\ModelUT\bin\ModelUT.dll ..\PersistenceUT\bin\PersistenceUT.dll /xml=TestResults\nunit-result.xml

exit /b 0