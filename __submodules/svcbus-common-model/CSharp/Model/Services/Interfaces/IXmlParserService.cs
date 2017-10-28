namespace Sovos.SvcBus.Common.Model.Services.Interfaces
{
    public interface IXmlParserService
    {
        T Parse<T>(string fileName);
    }
}
