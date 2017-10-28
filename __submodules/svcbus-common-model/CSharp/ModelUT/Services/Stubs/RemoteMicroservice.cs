using System;
using System.Collections.Generic;
using Sovos.SvcBus;
using Sovos.SvcBus.Common.Model.Capability;
using Sovos.SvcBus.Common.Model.Services.Interfaces;

namespace ModelUT.Services.Stubs
{
    class RemoteMicroservice : IRemoteMicroservice
    {
        public List<Tuple<string, object>> RequestsSent { get; set; }

        public RemoteMicroservice()
        {
            RequestsSent = new List<Tuple<string, object>>();
        }

        public object SendBroadcast(string command, object args)
        {
            throw new NotImplementedException();
        }

        public object SendCommand(string command, object args, bool wait = true)
        {
            RequestsSent.Add(new Tuple<string, object>(command, args));

            return new DataDto { DataBool = true };
        }

        public void SendCommandAsync(string command, object args, Message sourceMessage, IReplier replier, Consumer.OnAsyncWait callback,
            object callbackArgs = null)
        {
            throw new NotImplementedException();
        }
    }
}