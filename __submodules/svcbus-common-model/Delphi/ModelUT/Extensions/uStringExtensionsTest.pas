unit uStringExtensionsTest;

interface

uses
  TestFramework;

type
  {$M+}
  TStringExtensionsTest = class(TTestCase)
  published
    procedure TrimMatchingQuotesTest;
    procedure SplitCommandLineTest;
  end;
  {$M-}

implementation

uses
  uStringExtensions, Classes;

{ TStringExtensionsTest }

procedure TStringExtensionsTest.SplitCommandLineTest;
const
  command = '/t:tcommand -u:ucommand /z:"some z command"';
var
  splitCommand: TStringList;
begin
  splitCommand := SplitCommandLine(command);
  try
  CheckEquals(3, splitCommand.Count);
  CheckEqualsString('/t:tcommand', splitCommand[0]);
  CheckEqualsString('-u:ucommand', splitCommand[1]);
  CheckEqualsString('/z:"some z command"', splitCommand[2]);
  finally
    splitCommand.Free;
  end;
end;

procedure TStringExtensionsTest.TrimMatchingQuotesTest;
begin
  CheckEqualsString('test test', TrimMatchingQuotes('"test test"', '"'));
  CheckEqualsString('test test', TrimMatchingQuotes('!test test!', '!'));
end;

initialization
  RegisterTest(TStringExtensionsTest.Suite);

end.
