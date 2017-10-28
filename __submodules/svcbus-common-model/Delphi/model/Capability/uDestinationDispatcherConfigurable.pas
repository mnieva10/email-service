unit uDestinationDispatcherConfigurable;

interface

function GetComputerNameAsString: string;

type
  {$M+}
  TDestinationDispatcherConfigurable = class
  private
    FMachineName: string;
    FPid: Integer;
    function GetMatchCurrent: Boolean;
  published
    property MachineName: string read FMachineName write FMachineName;
    property Pid: Integer read FPid write FPid;
  public
    constructor Create;
    property MatchCurrent: Boolean read GetMatchCurrent;
  end;
  {$M-}

implementation

uses
  Windows, MongoBsonSerializer;

function GetComputerNameAsString: string;
var
  len: Cardinal;
begin
  len := MAX_COMPUTERNAME_LENGTH + 1;
  Result := StringOfChar(#0, len);
  GetComputerName(PChar(Result), len);
  SetLength(Result, len);
end;

function BuildDestinationDispatcherConfigurableObject(const AClassName: string;
  AContext: Pointer): TObject;
begin
  Result := TDestinationDispatcherConfigurable.Create;
end;

{ TDestinationDispatcherConfigurable }

constructor TDestinationDispatcherConfigurable.Create;
begin
  MachineName := GetComputerNameAsString;
  Pid := GetCurrentProcessId;
end;

function TDestinationDispatcherConfigurable.GetMatchCurrent: Boolean;
begin
  if (Length(MachineName) <> 0) and (MachineName <> GetComputerNameAsString) then
  begin
    Result := false;
    Exit;
  end;

  if (Pid <> 0) and (Pid <> Integer(GetCurrentProcessId)) then
  begin
    Result := false;
    Exit;
  end;

  Result := true;
end;

initialization
  RegisterBuildableSerializableClass(TDestinationDispatcherConfigurable.ClassName,
    BuildDestinationDispatcherConfigurableObject);
finalization
  UnregisterBuildableSerializableClass(TDestinationDispatcherConfigurable.ClassName);

end.
