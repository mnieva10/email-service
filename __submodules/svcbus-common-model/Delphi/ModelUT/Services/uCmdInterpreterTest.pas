unit uCmdInterpreterTest;

interface

uses
  TestFramework, uCmdInterpreter;

type
  {$M+}
  TTestCmdInterpreter = class(TTestCase)
  private
    FInterpreter: TCmdInterpreter;
  public
    procedure SetUp; override;
    procedure TearDown; override;
  published
    procedure CmdInterpreterConstructor_InvalidDI;
    procedure ProcessCommand_MethodNotFound;
    procedure ProcessCommand_ShouldReturnDispatcherResponse;
    procedure ProcessCommandNotExistingCmd_ShouldRaise;
    procedure ProcessCommandInvalidJson_ShouldRaise;
    procedure ProcessCommand_ShouldDeserialize;
    procedure CustomFormatProperty_ShouldDeserialize;
    procedure CustomFormatNestedProperty_ShouldDeserialize;
    procedure CustomFormatList_ShouldDeserialize;
    procedure CustomFormatDictionary_ShouldDeserialize;
    procedure CustomFormatObjList_ShouldDeserialize;
  end;
  {$M-}

implementation

uses
  SvcBusDispatcher, MongoBson, MongoBsonSerializer, SvcBusDispatcherResponse, SvcBusMessage,
  SysUtils, SvcBus, DateUtils, Types, uCnvDictionary, superobject;

type
  {$M+}
  TCmdInterpreterDispatchInterface = class(TDispatchInterface)
  published
    function Echo(msg : IMessage): TObject;
    function Deserialize(msg : IMessage): TObject;
  end;

  TDiAbstractStub = class(TDispatchInterface)
  public
    constructor Create(AUserData: Pointer); override;
  end;

  TUser = class
  private
    FAge: Integer;
    FUsername, FPassword: string;
  published
    property Age: Integer read FAge write FAge default 0;
    property UserName: string read FUsername write FUsername;
    property Password: string read FPassword write FPassword;
  end;

  TNestedTestClass = class
  private
    FDict: TCnvStringDictionary;
    FList: TIntegerArray;
  public
    constructor Create;
    destructor Destroy; override;
  published
    property Dict: TCnvStringDictionary read FDict write FDict;
    property List: TIntegerArray read FList write FList;
  end;

  TUserArray = array of TUser;
  TTestClass = class
  private
    FTestString: string;
    FUser: TUser;
    FTestInt: Integer;
    FTestBool: Boolean;
    FTestDateTime: TDateTime;
    FTestDictionary: TCnvStringDictionary;
    FNested: TNestedTestClass;
    FList: TIntegerArray;
    FStrList: TStringArray;
    FUserList: TUserArray;
  public
    constructor Create;
    destructor Destroy; override;
  published
    property TestString: string read FTestString write FTestString;
    property TestInt: Integer read FTestInt write FTestInt;
    property TestBool: Boolean read FTestBool write FTestBool;
    property TestDateTime: TDateTime read FTestDateTime write FTestDateTime;
    property User: TUser read FUser write FUser;
    property TestDictionary: TCnvStringDictionary read FTestDictionary write FTestDictionary;
    property Nested: TNestedTestClass read FNested write FNested;
    property List: TIntegerArray read FList write FList;
    property StrList: TStringArray read FStrList write FStrList;
    property UserList: TUserArray read FUserList write FUserList;
  end;
  {$M-}

function BuildTTestClass(const AClassName: string; AContext: Pointer): TObject;
begin
  Result := TTestClass.Create;
end;

function BuildTUser(const AClassName: string; AContext: Pointer): TObject;
begin
  Result := TUser.Create;
end;

{ TTestCmdInterpreter }

procedure TTestCmdInterpreter.ProcessCommandNotExistingCmd_ShouldRaise;
begin
  StartExpectingException(EDispatchInterfaceMethodNotFound);
  FInterpreter.ProcessCommand('invalidCmd /t:Object /j:{a: 1}');
  StopExpectingException;
end;

procedure TTestCmdInterpreter.ProcessCommand_MethodNotFound;
begin
  StartExpectingException(EDispatchInterfaceMethodNotFound);
  FInterpreter.ProcessCommand('NoSuchMethod params');
  StopExpectingException;
end;

procedure TTestCmdInterpreter.ProcessCommand_ShouldDeserialize;
var
  obj: TTestClass;
  i: Integer;
  s: string;
begin
  obj := TTestClass(FInterpreter.ProcessCommand('Deserialize /t:TestClass /j:{TestString:"test string", '+
    'TestDateTime: "8/27/2014", User: {Age: 5}, Nested: { Dict: {a: 1, b: c} } }'));
  CheckEqualsString('test string', obj.TestString);
  CheckEquals(5, obj.User.Age);
  Check(EqualsValue = CompareDateTime(EncodeDateTime(2014, 08, 27, 0, 0, 0, 0), obj.TestDateTime));
  Check(obj.Nested.Dict.TryGetValue('a', i));
  CheckEquals(1, i);
  Check(obj.Nested.Dict.TryGetValue('b', s));
  CheckEquals('c', s);
end;

procedure TTestCmdInterpreter.ProcessCommand_ShouldReturnDispatcherResponse;
var
  response: TStatusResponse;
begin
  response := TStatusResponse(FInterpreter.ProcessCommand('Ping /t:Object /j:{"a": 1}'));
  CheckEqualsString('ok', response.Status);
end;

procedure TTestCmdInterpreter.ProcessCommandInvalidJson_ShouldRaise;
begin
  StartExpectingException(EInvalidJson);
  FInterpreter.ProcessCommand('Ping /t:Object /j:{invalid json}');
  StopExpectingException;
end;

procedure TTestCmdInterpreter.CmdInterpreterConstructor_InvalidDI;
begin
  StartExpectingException(EDispatchInterfaceCreateInstanceException);
  TCmdInterpreter.Create(TDiAbstractStub);
  StopExpectingException;
end;

procedure TTestCmdInterpreter.CustomFormatDictionary_ShouldDeserialize;
var
  obj: TTestClass;
  s: string;
begin
  obj := TTestClass(FInterpreter.ProcessCommand('Deserialize /t:TestClass ' +
    '/TestDictionary~:a~b /TestDictionary~:c~d'));

  Check(obj.TestDictionary.TryGetValue('a', s));
  CheckEquals('b', s);
  Check(obj.TestDictionary.TryGetValue('c', s));
  CheckEquals('d', s);
end;

procedure TTestCmdInterpreter.CustomFormatList_ShouldDeserialize;
var
  obj: TTestClass;
begin
  obj := TTestClass(FInterpreter.ProcessCommand('Deserialize /t:TestClass ' +
    '/List^:1 /StrList^:a /TestObject.TestInt:5 /StrList^:"long string" /List^:2'));
  CheckEquals(1, obj.List[0]);
  CheckEquals(2, obj.List[1]);
  // notice order is changed according to c# implementation
  CheckEqualsString('long string', obj.StrList[0]);
  CheckEqualsString('a', obj.StrList[1]);
end;

procedure TTestCmdInterpreter.CustomFormatNestedProperty_ShouldDeserialize;
var
  obj: TTestClass;
  i: Integer;
begin
  obj := TTestClass(FInterpreter.ProcessCommand('Deserialize /t:TestClass ' +
    '/User.Age:23 /User.UserName:Alex /User.Password:"111Four" /Nested.List^:8 /Nested.List^:9 /Nested.Dict~:a~8 /Nested.Dict~:b~9'));
  CheckEquals(23, obj.User.Age);
  CheckEquals('Alex', obj.User.UserName);
  CheckEquals('111Four', obj.User.Password);
  CheckEquals(8, obj.Nested.List[0]);
  CheckEquals(9, obj.Nested.List[1]);

  Check(obj.Nested.Dict.TryGetValue('a', i));
  CheckEquals(8, i);
  Check(obj.Nested.Dict.TryGetValue('b', i));
  CheckEquals(9, i);
end;

procedure TTestCmdInterpreter.CustomFormatObjList_ShouldDeserialize;
var
  obj: TTestClass;
begin
  obj := TTestClass(FInterpreter.ProcessCommand('Deserialize /t:TestClass ' +
    '/UserList^.Age:1 /UserList^.Age:2'));
  CheckEquals(1, obj.UserList[0].Age);
  CheckEquals(2, obj.UserList[1].Age);
end;

procedure TTestCmdInterpreter.CustomFormatProperty_ShouldDeserialize;
var
  obj: TTestClass;
begin
  obj := TTestClass(FInterpreter.ProcessCommand('Deserialize /t:TestClass ' +
    '/TestString:"test string" /TestBool:true /TestInt:12345 /TestDateTime:"8/27/2014"'));
  CheckEqualsString('test string', obj.TestString);
  Check(obj.TestBool);
  CheckEquals(12345, obj.TestInt);
  Check(EqualsValue = CompareDateTime(EncodeDateTime(2014, 08, 27, 0, 0, 0, 0), obj.TestDateTime));
end;

procedure TTestCmdInterpreter.SetUp;
begin
  FInterpreter := TCmdInterpreter.Create(TCmdInterpreterDispatchInterface);
end;

procedure TTestCmdInterpreter.TearDown;
begin
  FInterpreter.Free;
end;

{ TCmdInterpreterDispatchInterface }

function TCmdInterpreterDispatchInterface.Deserialize(msg: IMessage): TObject;
begin
  Result := nil;
  DeserializeRequest(msg, Result);
end;

function TCmdInterpreterDispatchInterface.Echo(msg: IMessage): TObject;
var
  echoRequest, echoResponse: TStatusResponse;
begin
  echoRequest := nil;
  DeserializeRequest(msg, TObject(echoRequest), TStatusResponse);
  echoResponse := TStatusResponse.Create;
  echoResponse.Status := echoRequest.Status;
  Result := echoResponse;
end;

{ TDiAbstractStub }

constructor TDiAbstractStub.Create(AUserData: Pointer);
begin
  raise Exception.Create('');
end;

{ TTestClass }

constructor TTestClass.Create;
begin
  FUser := TUser.Create;
  FTestDictionary := TCnvStringDictionary.Create(true);
  FNested := TNestedTestClass.Create;
end;

destructor TTestClass.Destroy;
begin
  FUser.Free;
  FTestDictionary.Free;
  FNested.Free;
end;

{ TNestedTestClass }

constructor TNestedTestClass.Create;
begin
  FDict := TCnvStringDictionary.Create(true);
end;

destructor TNestedTestClass.Destroy;
begin
  FDict.Free;
end;

initialization
  RegisterTest(TTestCmdInterpreter.Suite);
  RegisterBuildableSerializableClass(TTestClass.ClassName, BuildTTestClass);
  RegisterBuildableSerializableClass(TUser.ClassName, BuildTUser);

finalization
  UnregisterBuildableSerializableClass(TTestClass.ClassName);
  UnregisterBuildableSerializableClass(TUser.ClassName);

end.
