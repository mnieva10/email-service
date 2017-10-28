unit uStringExtensions;

interface

uses
  Classes;

function SplitCommandLine(const ACommandLine: string): TStringList;
function TrimMatchingQuotes(const s: string; quote: char): string;
procedure SplitByFirstChar(const s: string; delimiter: char; var before, after: string);

implementation

uses
  SysUtils, StrUtils;

function SplitCommandLine(const ACommandLine: string): TStringList;
var
  inQuotes: Boolean;
  bracketCount, i, beg: Integer;
  arg: string;
begin
  inQuotes := false;
  bracketCount := 0;
  beg := 1;
  Result := TStringList.Create;

  try
    for i := 1 to Length(ACommandLine) do
    begin
      case ACommandLine[i] of
      '"': inQuotes := not inQuotes;
      '{': Inc(bracketCount);
      '}': Dec(bracketCount);
      end;
      if ((not inQuotes) and (bracketCount = 0) and (ACommandLine[i] = ' ')) or
         (i = Length(ACommandLine)) then
      begin
        arg := TrimMatchingQuotes(Trim(Copy(ACommandLine, beg, i - beg + 1)), '"');
        if arg <> '' then
          Result.Add(arg);
        beg := i;
      end;
    end;

  except
    Result.Free;
    raise;
  end;
end;

function TrimMatchingQuotes(const s: string; quote: char): string;
var
  len: Integer;
begin
  Result := s;
  len := Length(s);
  if (len >= 2) and (s[1] = quote) and (s[len] = quote) then
    Result := Copy(s, 2, len - 2);
end;

procedure SplitByFirstChar(const s: string; delimiter: char; var before, after: string);
var
  splitPos: Integer;
begin
  splitPos := Pos(delimiter, s);
  if splitPos = 0 then
  begin
    before := s;
    Exit;
  end;

  before := LeftStr(s, splitPos - 1);
  after := RightStr(s, Length(s) - splitPos);
end;

end.
