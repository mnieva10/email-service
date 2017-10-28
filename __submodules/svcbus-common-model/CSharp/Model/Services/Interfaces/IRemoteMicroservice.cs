namespace Sovos.SvcBus.Common.Model.Services.Interfaces
{
    public interface IRemoteMicroservice
    {
        object SendBroadcast(string command, object args);

        object SendCommand(string command, object args, bool wait = true);

        void SendCommandAsync(string command, object args, Message sourceMessage, IReplier replier,
            Consumer.OnAsyncWait callback, object callbackArgs = null);
    }
}
