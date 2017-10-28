unit uConsoleMicroservice;

interface

uses
  SvcBusDispatcher;

const
  NewCommandPromt = 'Dispatcher > ';

  procedure Start(ADispatcherName: string; AClass: TDispatchInterfaceClass;
    AUserData: Pointer = nil);

implementation

uses
  SysUtils, Windows,
  uCmdInterpreter, MongoBson, MongoBsonSerializer, superobject;
  
var
  serializer: TBaseBsonSerializer;

procedure Start(ADispatcherName: string; AClass: TDispatchInterfaceClass;
  AUserData: Pointer);
var
  buf, input: string;
  charsReaded: Cardinal;
  interpreter: TCmdInterpreter;
  response: TObject;
  procedure WriteSupportedCommands;
  begin
    WriteLn('Supported commands:' + #13#10 +
      'Stop' + #13#10 +
      interpreter.DispatchInterfaceHelp);
  end;
begin
  interpreter := TCmdInterpreter.Create(AClass, AUserData);
  SetLength(buf, 8096);
  try
    WriteLn('Welcome to ', ADispatcherName, ' Dispatcher');
    WriteSupportedCommands;
    while true do
    begin
      Write(#13#10 + NewCommandPromt);

      ReadConsole(GetStdHandle(STD_INPUT_HANDLE), @buf[1], Length(buf), charsReaded, nil);
      input := TrimRight(Copy(buf, 1, charsReaded));
      try
        if (input = '') or
           (input = '/?') or
           (input = '/h') or
           (input = 'h') then
          WriteSupportedCommands
        else if LowerCase(input) = 'stop' then
        begin
          WriteLn('Service is gracefully stopping...');
          Exit;
        end
        else
        begin
          response := interpreter.ProcessCommand(input);
          serializer.Target := NewBsonBuffer;
          serializer.Serialize('', response);
          Writeln(SO(serializer.Target.finish.asJson).AsJSon(true, false));
        end;
      except
        on e: Exception do
          WriteLn(e.Message);
      end;
    end;
  finally
    interpreter.Free;
  end;
end;

initialization
  serializer := CreateSerializer(TObject);
finalization
  serializer.Free;

end.
