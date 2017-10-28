unit uDepCacheInvalidator;

interface

{$I DelphiVersion_defines.inc}

uses
  Classes, SyncObjs, Types, SysUtils, Windows,
  uDepCache,
  SvcBusProducer, SvcBusPipeService, SvcBusConsumer, SvcBusMessage, MongoBson, SvcBusService;

const
  SERVICE_NAME = 'dep_cache_invalidator_listener';

type
  /// <summary>A class that keeps a list of TDepCache objects and listens for messages over svcbus. The messages contain a
  /// list of dependency strings. When messages are received the class invalidates the objects inside the caches that have
  /// such dependencies.</summary>
  TDepCacheInvalidatorListener = class(TThread)
  private
    FProducer: IProducer;
    fPipe: IPipeService;
    FCaches: TList;
    FMutex: TMutex;
    fPauseEvent: TSimpleEvent;
  protected
{$IFDEF DELPHIXE}
    procedure TerminatedSet; override;
{$ENDIF}
    procedure Execute; override;
  public
{$IFNDEF DELPHIXE}
    /// <summary> Signals the wait event and terminates. Automatically called by the destructor
    /// Necessary only on D2007 because there is no TerminatedSet method to override.</summary>
    procedure SignalAndTerminate;
{$ENDIF}
    constructor Create(const APipeService: IPipeService);
    destructor Destroy; override;
    /// <summary>Add a dependency cache object to the list that will be invalidated when a message is received.</summary>
    procedure Add(const ADepCache: TDepCacheBase);
    /// <summary>Remove a dependency cache object from the list that will be invalidated when a message is received.</summary>
    procedure Remove(const ADepCache: TDepCacheBase);
  end;

  /// <summary>Implements a simple way to send invalidation messages(with dependency strings) over svcbus to the
  /// TDepCacheInvalidatorListener.</summary>
  TDepCacheInvalidatorSender = class
  private
    fConsumer: IConsumer;
    fPipe: IPipeService;
  public
    constructor Create(const APipeService: IPipeService);
    destructor Destroy; override;
    /// <summary>Send a request to the listener to invalidate dep-cache objects with the supplied dependencies.</summary>
    procedure RequestInvalidation(aDependencies: array of String); overload;
    /// <summary>Send a request to the listener to invalidate dep-cache objects with the supplied dependencies.</summary>
    procedure RequestInvalidation(aDependency: String); overload;
  end;

implementation

{ TDepCacheInvalidatorListener }

{$IFDEF DELPHIXE}
procedure TDepCacheInvalidatorListener.TerminatedSet;
begin
  if Assigned(fPauseEvent) then
    fPauseEvent.SetEvent;  // Set the event to free the lock and allow the Execute to complete
  inherited;
end;
{$ENDIF}

{$IFNDEF DELPHIXE}
procedure TDepCacheInvalidatorListener.SignalAndTerminate;
begin
  Terminate;
  if Assigned(fPauseEvent) then
    fPauseEvent.SetEvent;  // Set the event to free the lock and allow the Execute to complete
end;
{$ENDIF}

procedure TDepCacheInvalidatorListener.Execute;
var
  msg: IMessage;
  it: IBsonIterator;
  I, J: Integer;
  Deps: TStringArray;
begin
  while not Terminated do
    try
      fPauseEvent.WaitFor(INFINITE);
      if Terminated then
        Exit;

      msg := FProducer.waitAndTake;
      it := msg.Bson.find('dependencies');
      if it <> nil then
      begin
        FMutex.Acquire;
        try
          Deps := it.AsStringArray;
          for I := 0 to FCaches.Count - 1 do
            for J := 0 to Length(Deps) - 1 do
              TDepCacheBase(FCaches[I]).RemoveByDep(Deps[J]);
        finally
          FMutex.Release;
        end;
      end;
    except
      on e: EProducer do
        if e.ErrorCode <> SvcBusProducer.WAIT_EXCEEDED then
          raise;  // Terminate the thread. The exception is stored in the FatalException property and should be checked by the calling thread
    end;
end;

constructor TDepCacheInvalidatorListener.Create(const APipeService: IPipeService);
begin
  inherited Create(false);
  fPipe := APipeService;  // Keep a live reference to the pipe, just in case
  FProducer := newProducer(APipeService, 'TDepCacheInvalidatorListener', newService(SERVICE_NAME, VOLATILE));
  FProducer.setRequestTimeout(1000);
  FMutex := TMutex.Create;
  FCaches := TList.Create;
  fPauseEvent := TSimpleEvent.Create(nil, true, false, '');
end;

destructor TDepCacheInvalidatorListener.Destroy;
begin
//  FProducer := nil;  // VG: Disposing explicitly causes some abort() error in the svc-bus dll
  fPipe := nil;
  FCaches.Free;
  FMutex.Free;
  inherited;
{$IFNDEF DELPHIXE}
  SignalAndTerminate;
{$ENDIF}
  FreeAndNil(fPauseEvent);  // After the inherited destructor that calls Terminate
end;

procedure TDepCacheInvalidatorListener.Add(const ADepCache: TDepCacheBase);
begin
  FMutex.Acquire;
  try
    if FCaches.IndexOf(ADepCache) <> -1 then
      raise Exception.Create('The same dependency cache object is already added to the invalidator');

    FCaches.Add(ADepCache);
    if FCaches.Count = 1 then
      fPauseEvent.SetEvent;
  finally
    FMutex.Release;
  end;
end;

procedure TDepCacheInvalidatorListener.Remove(const ADepCache: TDepCacheBase);
begin
  FMutex.Acquire;
  try
    FCaches.Remove(ADepCache);
    if FCaches.Count = 0 then
      fPauseEvent.ResetEvent;
  finally
    FMutex.Release;
  end;
end;

{ TDepCacheInvalidatorSender }

constructor TDepCacheInvalidatorSender.Create(const APipeService: IPipeService);
begin
  inherited Create;
  fPipe := APipeService;  // Keep a live reference to the pipe, just in case
  fConsumer := newConsumer(APipeService, 'TDepCacheInvalidatorSender', newService(SERVICE_NAME, VOLATILE));
end;

destructor TDepCacheInvalidatorSender.Destroy;
begin
  fConsumer := nil;
  fPipe := nil;
  inherited;
end;

procedure TDepCacheInvalidatorSender.RequestInvalidation(aDependencies: array of String);
var
  I: Integer;
  Msg: IMessage;
  Buf: IBsonBuffer;
  Bson: IBson;
  BsonArr: TStringArray;
begin
  // Copy the data into a Bson as array field and send the message
  SetLength(BsonArr, Length(aDependencies));
  for I := 0 to Length(aDependencies) - 1 do
    BsonArr[I] := aDependencies[I];
  Buf := NewBsonBuffer;
  Buf.appendArray('dependencies', BsonArr);
  Bson := Buf.finish;
  Msg := newMessage();
  Msg.addBson(Bson);
  fConsumer.send(Msg);
end;

procedure TDepCacheInvalidatorSender.RequestInvalidation(aDependency: String);
begin
  RequestInvalidation([aDependency]);
end;

end.
