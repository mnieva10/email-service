using System.Collections.Generic;
using System.IO;
using Sovos.SvcBus.Common.Model.Capability;

namespace Sovos.SvcBus.Common.Model.Repositories
{
    public interface IRfmRepository
    {
        List<string> FindFileByRegEx(string schema, string regex);

        void Rename(BaseFileInfo currentFileInfo, BaseFileInfo newFileInfo);
        bool Delete(BaseFileInfo fileInfo);
        List<string> ReadFile(BaseFileInfo fileInfo);
        Stream CreateSovosStream(BaseFileInfo fileInfo, SovosStreamMode mode);
        void SaveFile(BaseFileInfo fileInfo);
        void SaveBytes(BaseFileInfo fileInfo);
        string FindHashFromMinHash(string minHash);
    }
}
