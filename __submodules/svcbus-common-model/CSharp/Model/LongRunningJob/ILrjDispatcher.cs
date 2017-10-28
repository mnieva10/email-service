namespace Sovos.SvcBus.Common.Model.LongRunningJob
{
    public interface ILrjDispatcher
    {
        object CancelLrj(Message msg);
        object PauseLrj(Message msg);
    }
}
