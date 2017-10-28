unit uLongRunningJobServiceClasses;

interface

uses
  Classes,
  SvcBusDispatcherResponse,
  uConnectionRepository,
  MongoBSonSerializer;

type
  {$M+}
  TLrjRequest = class(TObject)
  private
    FJobId : Integer;
    FParentId : Integer;
    FResumeAfterStepId : Integer;
    FUsername : string;
    FSchema : string;
    FSegHash: string;
  published
    property JobId : Integer read FJobId write FJobId;
    property ParentId : Integer read FParentId write FParentId;
    property ResumeAfterStepId : Integer read FResumeAfterStepId write FResumeAfterStepId;
    property Username : string read FUsername write FUsername;
    property Schema : string read FSchema write FSchema;
    property SegHash : string read FSegHash write FSegHash;
  end;

  TLrjProgress = class(TObject)
  private
    FJobId : Integer;
    FReportedOn : TDateTime;
    FProgressStatus : string;
    FStepId : Integer;
  published
    property JobId : Integer read FJobId write FJobId;
    property ReportedOn : TDateTime read FReportedOn write FReportedOn;
    property ProgressStatus : string read FProgressStatus write FProgressStatus;
    property StepId : Integer read FStepId write FStepId;
  end;

  TLrjProgressArray = array of TLrjProgress;

  TBaseJob = class abstract (TObject)
  private
    FId : Integer;
    FParentId : Integer;
    FStepId : Integer;
    FCreatedOn : TDateTime;
    FModifiedOn : TDateTime;
    FExpiresOn : TDateTime;
    FCommand : string;
    FServiceName : string;
    FRequest : string;
    FResumeAfterStepId : Integer;
    FProgressList : TLrjProgressArray;
    FStatus : Integer;
  published
    property Id : Integer read FId write FId;
    property ParentId : Integer read FParentId write FParentId;
    property StepId : Integer read FStepId write FStepId;
    property CreatedOn : TDateTime read FCreatedOn write FCreatedOn;
    property ModifiedOn : TDateTime read FModifiedOn write FModifiedOn;
    property ExpiresOn : TDateTime read FExpiresOn write FExpiresOn;
    property Command : string read FCommand write FCommand;
    property ServiceName : string read FServiceName write FServiceName;
    property Request : string read FRequest write FRequest;
    property ResumAfterStepId : Integer read FResumeAfterStepId write FResumeAfterStepId;
    property ProgressList : TLrjProgressArray read FProgressList write FProgressList;
    property Status : Integer read FStatus write FStatus;
  end;

  TBaseJobArray = array of TBaseJob;

  TJob = class (TBaseJob)
  private
    FSteps : TBaseJobArray;
  published
    property Steps : TBaseJobArray read FSteps write FSteps;
  end;

  TChangeJobStatusDTO = class (TObject)
  private
    FJobId : Integer;
    FStatus : Integer;
  published
    property JobId : Integer read FJobId write FJobId;
    property Status : Integer read FStatus write FStatus;
  end;
  {$M-}

implementation

function BuildTLrjRequestObject(const AClassName: string; AContext: Pointer): TObject;
begin
  Result := TLrjRequest.Create;
end;

function BuildTLrjProgressObject(const AClassName: string; AContext: Pointer): TObject;
begin
  Result := TLrjProgress.Create;
end;

function BuildTJobObject(const AClassName: string; AContext: Pointer): TObject;
begin
  Result := TJob.Create;
end;

function BuildTChangeJobStatusObject(const AClassName: string; AContext: Pointer): TObject;
begin
  Result := TChangeJobStatusDTO.Create;
end;

{ TLrjRequest, TLrjProgress, TJob }

Initialization
  RegisterBuildableSerializableClass(TLrjRequest.ClassName, BuildTLrjRequestObject);
  RegisterBuildableSerializableClass(TLrjProgress.ClassName, BuildTLrjProgressObject);
  RegisterBuildableSerializableClass(TJob.ClassName, BuildTJobObject);
  RegisterBuildableSerializableClass(TChangeJobStatusDTO.ClassName, BuildTChangeJobStatusObject);
finalization
  UnregisterBuildableSerializableClass(TLrjRequest.ClassName);
  UnregisterBuildableSerializableClass(TLrjProgress.ClassName);
  UnregisterBuildableSerializableClass(TJob.ClassName);
  UnregisterBuildableSerializableClass(TChangeJobStatusDTO.ClassName);
end.
