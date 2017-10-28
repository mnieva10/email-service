using Sovos.SvcBus.Common.Model.Operation;

namespace Sovos.<%= namespace %>.Model.Services
{
    public interface I<%= namespace %>Service
    {
        SecurityAnswer FindUserAnswer(SecurityAnswer answer);
    }
}

