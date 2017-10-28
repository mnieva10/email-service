unit uCmdInterpreter;

interface

uses
  SvcBusDispatcher, SysUtils, Classes;

type
  ECmdInterpreter = class(Exception)
  end;

  EDispatchInterfaceMethodNotFound = class(ECmdInterpreter)
    constructor Create(const AMethod: string);
  end;

  EDispatchInterfaceCreateInstanceException = class(ECmdInterpreter)
    constructor Create(const AMessage, ADiType: string);
  end;

  ETypeNotFound = class(ECmdInterpreter)
    constructor Create;
  end;

  TCmdInterpreter = class
  private
    FDispatchInterface: TDispatchInterface;
    function ExtractParameter(var args: TStringList; const AKey: string): string;
    function ConvertCustomFormatToJson(const args: TStringList): string; overload;
    function ConvertCustomFormatToJson(const keys, values: TStringList): string; overload;
    function GetDispatchMethod(const AName: string): TMethod;
  public
    constructor Create(AClass: TDispatchInterfaceClass; AUserData: Pointer = nil);
    destructor Destroy; override;
    function DispatchInterfaceHelp: string;
    function ProcessCommand(const ACommand: string): TObject;
  end;

implementation

uses
  SvcBus, MongoBson, MongoBsonSerializer, SvcBusMessage,
  uStringExtensions, StrUtils, TypInfo, DateUtils, uCnvDictionary,
  superobject, Windows, uScope;

{ TCmdInterpreter }

function ToMongodbExtendedJson(const s: string): string;
var
  obj: ISuperObject;
  d: TDateTime;
  item: TSuperObjectIter;
  fmt: TFormatSettings;
  str: string;
begin
  GetLocaleFormatSettings(GetThreadLocale, fmt);
  obj := SO(s);
  if ObjectFindFirst(obj, item) then
  repeat
    if item.val.DataType <> stString then
      continue;
    str := item.val.AsString;
    // TryStrToDateTime parse 2-digit number as valid date we need to prevent this
    if ContainsText(str, fmt.DateSeparator) and
       TryStrToDateTime(str, d, fmt) then
      item.Ite.Current.Value := SO(Format('{"$date":{"$numberLong":"%d"}}', [DateTimeToUnix(d) * 1000]))
  until not ObjectFindNext(item);
  ObjectFindClose(item);
  Result := obj.AsString;
end;

// This is to silence erroneous warning about function result undefined
{$Warnings Off}
function TCmdInterpreter.ConvertCustomFormatToJson(
  const keys, values: TStringList): string;
var
  i: Integer;
  nestedKey, key, nestedJson, dictkey, dictValue: string;
  nestedKeys, nestedValues, nestedKeys2, nestedValues2: TStringList;
  j: Integer;
  LocalScope : IScope;
begin
  Result := '';
  LocalScope := NewScope;
  nestedKeys := LocalScope.Add(TStringList.Create);
  nestedValues := LocalScope.Add(TStringList.Create);
  nestedKeys2 := LocalScope.Add(TStringList.Create);
  nestedValues2 := LocalScope.Add(TStringList.Create);
  for i := 0 to keys.Count - 1 do
  begin
    SplitByFirstChar(keys[i], '.', key, nestedKey);
    if Length(nestedKey) > 0 then
    begin
      nestedKeys.Add(nestedKey);
      nestedValues.Add(values[i]);
    end;

    if (nestedKeys.Count > 0) and ((i = keys.Count - 1) or (not StartsStr(key, keys[i + 1]))) then
    begin
      if EndsStr('^', key) then
      begin
        Delete(key, Length(key), 1);
        Result := Result + key + ':[';
        for j := 0 to nestedKeys.Count - 1 do
        begin
          nestedKeys2.Add(nestedKeys[j]);
          nestedValues2.Add(nestedValues[j]);
          Result := Result + '{' + ConvertCustomFormatToJson(nestedKeys2, nestedValues2) + '}';
          if j <> nestedKeys.Count - 1 then
            Result := Result + ',';
          nestedKeys2.Clear;
          nestedValues2.Clear;
        end;
        Result := Result + ']';
      end
      else
      begin
        nestedJson := ConvertCustomFormatToJson(nestedKeys, nestedValues);
        Result := Result + key + ':{' + nestedJson + '}';
      end;
      nestedKeys.Clear;
      nestedValues.Clear;
    end;

    if (Length(nestedKey) = 0) then
    begin
      if EndsStr('^', key) then
      begin
        Delete(key, Length(key), 1);
        if (i = 0) or (not StartsStr(key, keys[i - 1])) then
          Result := Result + key + ':[';
        Result := Result + values[i];
        if (i = keys.Count - 1) or (not StartsStr(key, keys[i + 1])) then
          Result := Result + ']';
      end
      else if EndsStr('~', key) then
      begin
        Delete(key, Length(key), 1);
        if (i = 0) or (not StartsStr(key, keys[i - 1])) then
          Result := Result + key + ':{';
        SplitByFirstChar(values[i], '~', dictkey, dictValue);
        if Length(dictValue) > 0 then
          Result := Result + dictkey + ':' + dictValue
        else
          Result := Result + values[i];
        if (i = keys.Count - 1) or (not StartsStr(key, keys[i + 1])) then
          Result := Result + '}';
      end
      else
        Result := Result + key + ':' + values[i];
    end;
    if (Length(Result) > 0) and (Result[Length(Result)] <> ',') and (i <> keys.Count - 1) then
      Result := Result + ',';
  end;
end;
{$Warnings On}

function TCmdInterpreter.ConvertCustomFormatToJson(
  const args: TStringList): string;
var
  i: Integer;
  key, value: string;
  keys, values: TStringList;
begin
  keys := TStringList.Create;
  values := TStringList.Create;
  try
    for i := 0 to args.Count - 1 do
      if StartsStr('/', args[i]) then
      begin
        SplitByFirstChar(args[i], ':', key, value);
        Delete(key, 1, 1);
        keys.Add(key);
        values.Add(value);
      end;
    Result := '{' + ConvertCustomFormatToJson(keys, values) + '}';
  finally
    keys.Free;
    values.Free;
  end;
end;

constructor TCmdInterpreter.Create(AClass: TDispatchInterfaceClass; AUserData: Pointer);
begin
  try
    FDispatchInterface := AClass.Create(AUserData);
  except
    on e: exception do
      raise EDispatchInterfaceCreateInstanceException.Create(e.Message, AClass.ClassName);
  end;
end;

destructor TCmdInterpreter.Destroy;
begin
  FDispatchInterface.Free;
  inherited;
end;

function TCmdInterpreter.ExtractParameter(var args: TStringList;
  const AKey: string): string;
var
  i: Integer;
begin
  Result := '';
  for i := 0 to args.Count - 1 do
    if StartsStr(AKey, args[i]) then
    begin
      Result := TrimMatchingQuotes(Copy(args[i], Length(AKey) + 1,
        Length(args[i]) - Length(AKey)), '"');
      args.Delete(i);
      Exit;
    end;
end;

function ExtractCommand(var args: TStringList): string;
begin
  if args.Count = 0 then
    Result := 'help'
  else
  begin
    Result := args[0];
    args.Delete(0);
  end;
end;

function TCmdInterpreter.GetDispatchMethod(const AName: string): TMethod;
begin
  Result.Code := FDispatchInterface.MethodAddress(AName);
  if Result.Code = nil then
    raise EDispatchInterfaceMethodNotFound.Create(AName);
  Result.Data := FDispatchInterface;
end;

function TCmdInterpreter.DispatchInterfaceHelp: string;
begin
  Result := FDispatchInterface.Help;
end;

function TCmdInterpreter.ProcessCommand(const ACommand: string): TObject;
var
  className, json, extendedJson: string;
  method: TMethod;
  request: IMessage;
  args: TStringList;
  b: IBsonBuffer;
begin
  args := SplitCommandLine(ACommand);
  try
    method := GetDispatchMethod(ExtractCommand(args));

    className := ExtractParameter(args, '/t:');
    if className = '' then
      raise ETypeNotFound.Create;

    request := newMessage();
    b := NewBsonBuffer;
    b.startObject(SVCBUS_DISPATCHER_DESCRIBE_PARAMETERS);
    b.appendStr(SERIALIZED_ATTRIBUTE_ACTUALTYPE, className);
    json := ExtractParameter(args, '/j:');
    if json = '' then
    begin
      args.Sort;
      json := ConvertCustomFormatToJson(args);
    end;
    extendedJson := ToMongodbExtendedJson(json);
    b.append(NewBson(extendedJson));
    b.finishObject;
    request.addBson(b.finish);
  finally
    args.Free
  end;

  Result := TDispatchMethod(method)(request);
end;

{ EDispatchInterfaceMethodNotFound }

constructor EDispatchInterfaceMethodNotFound.Create(const AMethod: string);
begin
  inherited Create(Format('Method "%s" was not found in DispatchInterface.', [AMethod]));
end;

{ EDispatchInterfaceCreateInstanceException }

constructor EDispatchInterfaceCreateInstanceException.Create(const AMessage,
  ADiType: string);
begin
  inherited Create(Format('Error instantiating Dispatch Interface of type %s: %s',
    [ADiType, AMessage]));
end;

{ ETypeNotFound }

constructor ETypeNotFound.Create;
begin
  inherited Create('Message type parameter </t:object type> was not found.');
end;

end.
