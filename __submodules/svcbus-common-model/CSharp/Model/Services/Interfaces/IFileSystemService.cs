using System.IO;
using System.Security.AccessControl;
using System.Collections.Generic;

namespace Sovos.SvcBus.Common.Model.Services.Interfaces
{
    public interface IFileSystemService
    {
        void CreateFile(string path, int bufferSize = 4096, FileOptions fileOptions = FileOptions.None, FileSecurity fileSecurity = null);
        bool DirectoryExists(string path);
        void FileDelete(string file);
        bool FileExists(string file);
        string GetPathRoot(string path);
        void MoveFile(string source, string destination);
        long GetFileSize(string path);
        byte[] ReadAllBytes(string path);
        void WriteAllLines(string path, IEnumerable<string> contents);
    }
}