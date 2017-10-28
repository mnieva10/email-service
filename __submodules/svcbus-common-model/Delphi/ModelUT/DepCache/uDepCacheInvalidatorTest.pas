unit uDepCacheInvalidatorTest;

{$I DelphiVersion_defines.inc}

interface

uses
  TestFramework, Classes, SysUtils, Windows,
  uDepCacheInvalidator,
  SvcBusPipeService,
  uDepcache;

const
  DEFAULT_CONNECTION_STRING = 'mongodb://localhost/testdb';

type
  {$M+}
  TDepCacheInvalidatorTest = class(TTestCase)
  private
    fPipe: IPipeService;
    fListener: TDepCacheInvalidatorListener;
    fSender: TDepCacheInvalidatorSender;
    fDepCache: TDepCacheBase;
  public
    procedure SetUp; override;
    procedure TearDown; override;
  published
    procedure TestListenerCreate;
    procedure TestSenderCreate;
    procedure TestListenerAddDepCache;
    procedure TestMessageAndInvalidation;
  end;
  {$M-}

  TDummyObject = class
    Int: Integer;
    Text: String;
  end;

  TDepCacheBuilderDummy = class(TInterfacedObject, IDepCacheable)
    function CreateObj(const AArgs: array of string; ADeps: TDependencies): TObject;
    procedure DestroyObj(AObject: TObject);
  end;

const
  ARGS: array[0..2] of String = ('11', '22', '33');
  DEP1 = 'dep1';
  DEP2 = 'dep2';

implementation

{ TDepCacheInvalidatorTest }

procedure TDepCacheInvalidatorTest.SetUp;
begin
  inherited;
  fPipe := newPipeService(DEFAULT_CONNECTION_STRING);
  fListener := TDepCacheInvalidatorListener.Create(fPipe);
  fSender := TDepCacheInvalidatorSender.Create(fPipe);
  fDepCache := TDepCachePooled{$IFDEF DELPHIXE}<TDummyObject>{$ENDIF}.Create(TDepCacheBuilderDummy.Create, 0, DEP_CACHE_NONE); // No expiration of dep-cache objects
end;

procedure TDepCacheInvalidatorTest.TearDown;
begin
{$IFDEF DELPHIXE}
  fListener.Terminate;
{$ELSE}
  fListener.SignalAndTerminate;
{$ENDIF}
  fListener.WaitFor;
  if fListener.FatalException <> nil then
    raise fListener.FatalException;
  fListener.Free;
  fSender.Free;
  fDepCache.Free;
  fPipe := nil;
  inherited;
end;

procedure TDepCacheInvalidatorTest.TestListenerCreate;
var
  Listener: TDepCacheInvalidatorListener;
  Pipe: IPipeService;
begin
  Pipe := newPipeService(DEFAULT_CONNECTION_STRING);
  Listener := TDepCacheInvalidatorListener.Create(Pipe);
  Listener.Free;
  Pipe := nil;
  CheckTrue(Listener <> nil, 'No exceptions should be raised');
end;

procedure TDepCacheInvalidatorTest.TestSenderCreate;
var
  Sender: TDepCacheInvalidatorSender;
  Pipe: IPipeService;
begin
  Pipe := newPipeService(DEFAULT_CONNECTION_STRING);
  Sender := TDepCacheInvalidatorSender.Create(Pipe);
  Sender.Free;
  Pipe := nil;
  CheckTrue(Sender <> nil, 'No exceptions should be raised');
end;

procedure TDepCacheInvalidatorTest.TestListenerAddDepCache;
begin
  fListener.Add(fDepCache);
  CheckTrue(True, 'No exceptions should be raised');
end;

procedure TDepCacheInvalidatorTest.TestMessageAndInvalidation;
var
  Obj1, Obj2: TDummyObject;
  Time1, Time2: Integer;
begin
  fListener.Add(fDepCache);

  // Acquire and release object, then send a msg to the listener to invalidate this object
  // Give it some time and check the object has been invalidated (a new one should be acquired)
  Obj1 := fDepCache.Acquire(ARGS);
  Time1 := Obj1.Int;
  fDepCache.Release(Obj1);

  fSender.RequestInvalidation(['OtherDep', DEP1]);

  Sleep(2000);
  Obj2 := fDepCache.Acquire(ARGS);
  Time2 := Obj2.Int;
  fDepCache.Release(Obj2);

  CheckTrue(Time1 <> Time2, 'The two objects should be different');
end;

{ TDepCacheBuilderDummy }

function TDepCacheBuilderDummy.CreateObj(const AArgs: array of string; ADeps: TDependencies): TObject;
var
  Obj: TDummyObject;
begin
  Obj := TDummyObject.Create;
  Obj.Int := GetTickCount;
  Obj.Text := FormatDateTime('ddmmyy_hhnnsszzz', Now);
  ADeps.AddDependency(DEP1);
  ADeps.AddDependency(DEP2);
  Result := Obj;
end;

procedure TDepCacheBuilderDummy.DestroyObj(AObject: TObject);
begin
  AObject.Free;
end;

initialization
  RegisterTest(TDepCacheInvalidatorTest.Suite);

end.
