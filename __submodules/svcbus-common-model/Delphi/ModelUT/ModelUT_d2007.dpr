program ModelUT_d2007;

{$IFDEF DCC_ConsoleTarget}
{$APPTYPE CONSOLE}
{$ENDIF}

uses
  SvcBusAPI,
  LibbsonAPI,
  uLibMongocAPI,
  uCnvDunitUtilities in '..\..\..\dunitgroup-dunit\convey-src\uCnvDunitUtilities.pas',
  GTestStyleTestRunner in '..\..\..\dunitgroup-dunit\src\GTestStyleTestRunner.pas',
  uDestinationDispatcherConfigurableTest in 'Services\uDestinationDispatcherConfigurableTest.pas',
  uDestinationDispatcherConfigurable in '..\model\Capability\uDestinationDispatcherConfigurable.pas',
  uConsoleMicroservice in '..\model\services\uConsoleMicroservice.pas',
  uFileLogger in '..\model\services\uFileLogger.pas',
  uDestinationRequestProcessingStrategy in '..\model\Strategies\uDestinationRequestProcessingStrategy.pas',
  uPingRequestStrategy in '..\model\Strategies\uPingRequestStrategy.pas',
  uPingRequestStrategyTest in 'Services\uPingRequestStrategyTest.pas',
  uTestUsingDispatcher in 'Services\uTestUsingDispatcher.pas',
  uCmdInterpreter in '..\model\services\uCmdInterpreter.pas',
  uCmdInterpreterTest in 'Services\uCmdInterpreterTest.pas',
  uStringExtensions in '..\model\Extensions\uStringExtensions.pas',
  uStringExtensionsTest in 'Extensions\uStringExtensionsTest.pas',
  uMutexedRequestProcessingStrategyTest in 'Services\uMutexedRequestProcessingStrategyTest.pas',
  uMutexedRequestProcessingStrategy in '..\model\Strategies\uMutexedRequestProcessingStrategy.pas',
  uLogFileStream in '..\model\services\uLogFileStream.pas',
  uLogFileStreamTest in 'Services\uLogFileStreamTest.pas',
  uDepCacheInvalidatorTest in 'DepCache\uDepCacheInvalidatorTest.pas';

begin
  LoadSvcBusLibrary(SvcBusDLL);
  LoadLibbsonLibrary(SvcBusDLL);

  RunDunitApp;
end.
