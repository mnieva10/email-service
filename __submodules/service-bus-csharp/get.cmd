if not exist %2 (
	md %2
)
del %2\service-bus_%1_v*.dll
robocopy \\conveydev.com\files\dev\sdlc-ci\release\user\service-bus-dll %2 service-bus_%1_v2-19-1.dll /ZB /X /TEE

EXIT /B 0
