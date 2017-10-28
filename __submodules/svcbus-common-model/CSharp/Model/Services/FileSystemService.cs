using System.IO;
using System.Security.AccessControl;
using System.Collections.Generic;
using Sovos.SvcBus.Common.Model.Exceptions;
using Sovos.SvcBus.Common.Model.Services.Interfaces;

namespace Sovos.SvcBus.Common.Model.Services
{
    public class FileSystemService : IFileSystemService
    {
        public void CreateFile(string path, int bufferSize = 4096, FileOptions fileOptions = FileOptions.None, FileSecurity fileSecurity = null)
        {
#if !NETCORE
            using (File.Create(path, bufferSize, fileOptions, fileSecurity)) { }
#else
            using (File.Create(path, bufferSize, fileOptions)) { }
#endif
        }

        public bool DirectoryExists(string path)
        {
            return Directory.Exists(path);
        }

        public void FileDelete(string file)
        {
            if (!File.Exists(file))
                throw new FileNotFoundException(file);

            File.Delete(file);
        }

        public bool FileExists(string file)
        {
            return File.Exists(file);
        }

        public string GetPathRoot(string path)
        {
            return Path.GetPathRoot(path);
        }

        public void MoveFile(string source, string destination)
        {
            if (!File.Exists(source))
                throw new FileNotFoundException(source);
            if (string.IsNullOrEmpty(destination) || !Directory.Exists(Path.GetDirectoryName(destination)))
                throw new DirectoryNotFoundException(destination);
            if (File.Exists(destination))
                throw new FileAlreadyExistsException(destination);

            File.Move(source, destination);
        }

        public long GetFileSize(string path)
        {
            return (new FileInfo(path).Length);
        }

        public byte[] ReadAllBytes(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException(path);

            return File.ReadAllBytes(path);
        }

        public void WriteAllLines(string path, IEnumerable<string> contents)
        {
            File.WriteAllLines(path, contents);
        }
    }
}
