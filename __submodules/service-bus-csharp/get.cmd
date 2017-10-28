if not exist %2 (
	md %2
)
del %2\service-bus_%1_v*.dll
robocopy C:\Users\Tincho-PC\Desktop\Dev\email-service\libs %2 service-bus_%1_v2-19-1.dll /ZB /X /TEE

EXIT /B 0
