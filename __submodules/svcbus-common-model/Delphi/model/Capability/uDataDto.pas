unit uDataDto;

interface

uses
  Classes;

type
  {$M+}
  TDataDto = class(TObject)
  private
    FDataBool: boolean;
    FDataInt: Integer;
    FDataList: TStringList;
    FDataString: string;
    FDomain: string;
    FSchema: string;
    FTablePrefix: string;
  public
    constructor Create;
    destructor Destroy; override;
  published
    property DataBool: boolean read FDataBool write FDataBool;
    property DataInt: Integer read FDataInt write FDataInt;
    property DataList: TStringList read FDataList write FDataList;
    property DataString: string read FDataString write FDataString;
    property Domain: string read FDomain write FDomain;
    property Schema: string read FSchema write FSchema;
    property TablePrefix: string read FTablePrefix write FTablePrefix;
  end;
  {$M-}

implementation

uses
  MongoBsonSerializer;

function BuildDataDtoResponseObject(const AClassName: string; AContext: Pointer): TObject;
begin
  Result := TDataDto.Create;
end;

constructor TDataDto.Create;
begin
  inherited;
  FDataList := TStringList.Create;
end;

destructor TDataDto.Destroy;
begin
  FDataList.Free;
  inherited;
end;

initialization
  RegisterBuildableSerializableClass(TDataDto.ClassName, BuildDataDtoResponseObject);

finalization
  UnRegisterBuildableSerializableClass(TDataDto.ClassName);

end.
