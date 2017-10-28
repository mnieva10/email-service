using Sovos.SvcBus.Common.Model.Operation;

namespace Sovos.Template.Model.Services
{
    public interface ITemplateService
    {
        SecurityAnswer FindUserAnswer(SecurityAnswer answer);
    }
}

