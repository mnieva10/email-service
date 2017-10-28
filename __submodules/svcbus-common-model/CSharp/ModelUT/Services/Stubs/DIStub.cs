using System.Text;
using Sovos.SvcBus;
using Sovos.SvcBus.Common.Model.Capability;

namespace ModelUT.Services.Stubs
{
    public class DiStub : DispatchInterface
    {
        public DiStub(object userData) : base(userData)
        {
        }

        public object Help(Message msg)
        {
            var bldr = new StringBuilder();
            bldr.AppendLine("Help");
            return bldr.ToString();
        }

        public object TestMethod(Message msg)
        {
            return new DataDto{DataString = "Pass"};
        }
    }

    public abstract class DiAbstractStub: DispatchInterface
    {
        protected DiAbstractStub(object userData) : base(userData)
        {
        }
    }
}
