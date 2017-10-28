using System.Security.Permissions;
using System.Text;
using Sovos.SvcBus;
using Sovos.SvcBus.Common.Model.Capability;
using Sovos.SvcBus.Common.Model.Operation;
using Sovos.<%= namespace %>.Model.Capability;

namespace Sovos.<%= namespace %>.Model.Services
{
    public class <%= namespace %>DispatchInterface : DispatchInterface
    {
        private I<%= namespace %>Service <%= namespace %>Service { get; set; }
        private DIInputDto InputDto { get; set; }

        public <%= namespace %>DispatchInterface(object userData) : base(userData)
        {
            InputDto = (DIInputDto)userData;
            <%= namespace %>Service = new <%= namespace %>Service(InputDto.RepositoryFactory);
        }
        
        [PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
        public object Help(Message msg)
        {
            var bldr = new StringBuilder();
            bldr.AppendLine("\n  ** Dispatch Interface commands:\n");
            bldr.AppendLine("  FindUserAnswer /t:SecurityAnswer </Username:username> </QuestionId:question id> </Schema:schema> </TablePrefix:table prefix>");

            bldr.AppendLine("\n  * JSON notation alternative:\n");
            bldr.AppendLine("  FindUserAnswer /t:SecurityAnswer /j:{\"Username\":\"username\", \"QuestionId\":\"question id\", \"Schema\":\"schema\", \"TablePrefix\":\"table prefix\"}");

            return bldr.ToString();
        }

        [PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
        public new object Describe(Message msg)
        {
            return new DataDto { DataString = (string)Help(msg) };
        }

        public object FindUserAnswer(Message msg)
        {
            object answer = new SecurityAnswer();
            DeserializeRequest(msg, ref answer);

            return <%= namespace %>Service.FindUserAnswer((SecurityAnswer)answer);
        }
    }
}
