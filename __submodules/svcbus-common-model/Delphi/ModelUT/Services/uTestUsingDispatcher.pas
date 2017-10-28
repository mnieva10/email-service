unit uTestUsingDispatcher;

interface

uses
  TestFramework, SvcBusPipeService;

const
  MongoConnectionString = 'mongodb://localhost/testdb';

type
  TTestUsingDispatcher = class(TTestCase)
  protected
    FPs: IPipeService;
    procedure SetUp; override;
    procedure TearDown; override;
  end;

implementation

{ TTestUsingDispatcher }

procedure TTestUsingDispatcher.SetUp;
begin
  inherited;
  FPs := NewPipeService(MongoConnectionString);
end;

procedure TTestUsingDispatcher.TearDown;
begin
  FPs := nil;
  inherited;
end;

end.
