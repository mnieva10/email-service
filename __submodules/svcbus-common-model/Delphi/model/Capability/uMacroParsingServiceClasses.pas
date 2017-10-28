unit uMacroParsingServiceClasses;

interface

uses
  uCnvDictionary,
  SysUtils;

type
  {$M+}
  TRequestScript = class
  private
    FScriptPath, FScriptText, FDialect: string;
    FDefs, FVars: TCnvStringDictionary;
  public
    constructor Create; overload;
    constructor Create(const AScript: string); overload;
    destructor Destroy; override;
  published
    property Script: string read FScriptPath write FScriptPath;
    property Text: string read FScriptText write FScriptText;
    property Dialect: string read FDialect write FDialect;
    property Vars: TCnvStringDictionary read FVars write FVars;
    property Defs: TCnvStringDictionary read FDefs write FDefs;
  end;

  TRequestScriptArray = array of TRequestScript;

  TRequest = class
  private
    FSchema, FUserName: string;
    FScripts: TRequestScriptArray;
  public
    constructor Create(const ASchema, AUserName: string; var AScripts: TRequestScriptArray); overload;
    constructor Create(const ASchema, AUserName, AScript: string); overload;
    constructor Create(ASize: Cardinal); overload;
    destructor Destroy; override;
  published
    property Schema: string read FSchema write FSchema;
    property UserName: string read FUserName write FUserName;
    property Scripts: TRequestScriptArray read FScripts write FScripts;
  end;

  TStringArray = array of string;

  TResponseScript = class
  private
    FException: string;
    FSuccess: Boolean;
    FParsingErrors: TStringArray;
  public
    constructor Create(ASuccess: Boolean; AParsingErrors: TStringArray = nil); overload;
    constructor Create(const AException: Exception); overload;
  published
    property Exception: string read FException write FException;
    property Success: Boolean read FSuccess write FSuccess;
    property ParsingErrors: TStringArray read FParsingErrors write FParsingErrors;
  end;

  TParsedResponseScript = class(TResponseScript)
  private
    FText: string;
    procedure SetText(const AText: string);
  public
    constructor Create(ASuccess: Boolean; const AText: string;
      AParsingErrors: TStringArray); overload;
  published
    property Text: string read FText write SetText;
  end;

  TParsedResponseScriptArray = array of TParsedResponseScript;

  TParsedResponse = class
  private
    FResponses: TParsedResponseScriptArray;
  published
    constructor Create(const AResponses: TParsedResponseScriptArray); overload;
    destructor Destroy; override;
    property Responses: TParsedResponseScriptArray read FResponses write FResponses;
  end;
  
  TPlSqlResponseScript = class(TResponseScript)
  private
    FPlSqlBlocks: TStringArray;
  public
    constructor Create(ASuccess: Boolean; var APlSqlBlocks, AParsingErrors: TStringArray); overload;
  published
    property PlSqlBlocks: TStringArray read FPlSqlBlocks write FPlSqlBlocks;
  end;

  TPlSqlResponseScriptArray = array of TPlSqlResponseScript;

  TPlSqlResponse = class
  private
    FResponses: TPlSqlResponseScriptArray;
  published
    constructor Create(const AResponses: TPlSqlResponseScriptArray); overload;
    destructor Destroy; override;
    property Responses: TPlSqlResponseScriptArray read FResponses write FResponses;
  end;

  TClearCacheRequest = class
  private
    FSchema: string;
    FScripts: TStringArray;
  public
    constructor Create(const ASchema: string; const AScripts: TStringArray = nil); overload;
  published
    property Schema: string read FSchema write FSchema;
    property Scripts: TStringArray read FScripts write FScripts;
  end;

  TStringResponse = class
  private
    FResponse: string;
  published
    constructor Create(const AResponse: string); overload;
    property Response: string read FResponse write FResponse;
  end;
  {$M-}

implementation

uses
  MongoBsonSerializer;

{ TResponseScript }

constructor TResponseScript.Create(ASuccess: Boolean;
                                   AParsingErrors: TStringArray);
begin
  inherited Create;
  Success := ASuccess;
  ParsingErrors := AParsingErrors;
end;

constructor TResponseScript.Create(const AException: Exception);
begin
  inherited Create;
  Success := False;
  Exception := AException.ClassName + ': ' + AException.Message;
end;

{ TRequestScript }

constructor TRequestScript.Create(const AScript: string);
begin
  Create;
  FScriptPath := AScript;
end;

constructor TRequestScript.Create;
begin
  inherited;
  FVars := TCnvStringDictionary.Create;
  FDefs := TCnvStringDictionary.Create;
end;

destructor TRequestScript.Destroy;
begin
  FDefs.Free;
  FVars.Free;
  inherited;
end;

{ TRequest }

constructor TRequest.Create(const ASchema, AUserName: string;
  var AScripts: TRequestScriptArray);
begin
  inherited Create;
  FSchema := ASchema;
  FUserName := AUserName;
  FScripts := AScripts;
end;

constructor TRequest.Create(const ASchema, AUserName, AScript: string);
begin
  inherited Create;
  FSchema := ASchema;
  FUserName := AUserName;
  SetLength(FScripts, 1);
  FScripts[0] := TRequestScript.Create(AScript);
end;

constructor TRequest.Create(ASize: Cardinal);
var
  i: Integer;
begin
  inherited Create;
  SetLength(FScripts, ASize);
  for i:= 0 to ASize - 1 do
    FScripts[i] := TRequestScript.Create;
end;

destructor TRequest.Destroy;
var
  I: Integer;
begin
  for I := 0 to Length(FScripts) - 1 do
    FScripts[I].Free;
  inherited;
end;

{ TParsedResponse }

constructor TParsedResponse.Create(const AResponses: TParsedResponseScriptArray);
begin
  inherited Create;
  FResponses := AResponses;
end;

destructor TParsedResponse.Destroy;
var
  I: Integer;
begin
  for I := 0 to Length(FResponses) - 1 do
    FResponses[I].Free;
  inherited;
end;

{ TParsedResponseScript }

constructor TParsedResponseScript.Create(ASuccess: Boolean; const AText: string;
  AParsingErrors: TStringArray);
begin
  inherited Create(ASuccess, AParsingErrors);
  Text := AText;
end;

procedure TParsedResponseScript.SetText(const AText: string);
begin
  FText := AnsiToUtf8(AText);
end;

{ TPlSqlResponseScript }

constructor TPlSqlResponseScript.Create(ASuccess: Boolean;
  var APlSqlBlocks, AParsingErrors: TStringArray);
begin
  inherited Create(ASuccess, AParsingErrors);
  FPlSqlBlocks := APlSqlBlocks;
end;

{ TPlSqlResponse }

constructor TPlSqlResponse.Create(
  const AResponses: TPlSqlResponseScriptArray);
begin
  inherited Create;
  FResponses := AResponses;
end;

destructor TPlSqlResponse.Destroy;
var
  I: Integer;
begin
  for I := 0 to Length(FResponses) - 1 do
    FResponses[I].Free;
  inherited;
end;

{ TStringResponse }

constructor TStringResponse.Create(const AResponse: string);
begin
  inherited Create;
  FResponse := AResponse;
end;

{ TClearCacheRequest }

constructor TClearCacheRequest.Create(const ASchema: string;
  const AScripts: TStringArray);
begin
  inherited Create;
  FSchema := ASchema;
  FScripts := AScripts;
end;

{ Builders }

function BuildRequestScript(const AClassName: string; AContext: Pointer): TObject;
begin
  Result := TRequestScript.Create;
end;

function BuildRequest(const AClassName: string; AContext: Pointer): TObject;
begin
  Result := TRequest.Create;
end;

function BuildParseResponseScript(const AClassName: string;
  AContext: Pointer): TObject;
begin
  Result := TParsedResponseScript.Create;
end;

function BuildParseResponse(const AClassName: string;
  AContext: Pointer): TObject;
begin
  Result := TParsedResponse.Create;
end;

function BuildPlSqlResponseScript(const AClassName: string;
  AContext: Pointer): TObject;
begin
  Result := TPlSqlResponseScript.Create;
end;

function BuildPlSqlResponse(const AClassName: string;
  AContext: Pointer): TObject;
begin
  Result := TPlSqlResponse.Create;
end;

function BuildStringResponse(const AClassName: string;
  AContext: Pointer): TObject;
begin
  Result := TStringResponse.Create;
end;

initialization
  DictionarySerializationMode := ForceComplex;

  RegisterBuildableSerializableClass(TRequestScript.ClassName, BuildRequestScript);
  RegisterBuildableSerializableClass(TRequest.ClassName, BuildRequest);
  RegisterBuildableSerializableClass(TParsedResponseScript.ClassName, BuildParseResponseScript);
  RegisterBuildableSerializableClass(TParsedResponse.ClassName, BuildParseResponse);
  RegisterBuildableSerializableClass(TPlSqlResponseScript.ClassName, BuildPlSqlResponseScript);
  RegisterBuildableSerializableClass(TPlSqlResponse.ClassName, BuildPlSqlResponse);
  RegisterBuildableSerializableClass(TStringResponse.ClassName, BuildStringResponse);
finalization
  UnregisterBuildableSerializableClass(TStringResponse.ClassName);
  UnregisterBuildableSerializableClass(TPlSqlResponse.ClassName);
  UnregisterBuildableSerializableClass(TPlSqlResponseScript.ClassName);
  UnregisterBuildableSerializableClass(TParsedResponse.ClassName);
  UnregisterBuildableSerializableClass(TParsedResponseScript.ClassName);
  UnregisterBuildableSerializableClass(TRequest.ClassName);
  UnregisterBuildableSerializableClass(TRequestScript.ClassName);

end.