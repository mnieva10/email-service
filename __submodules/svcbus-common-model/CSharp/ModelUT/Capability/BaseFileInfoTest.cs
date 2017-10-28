using Sovos.SvcBus.Common.Model.Capability;
using NUnit.Framework;

namespace ModelUT.Capability
{
    [TestFixture]
    public class BaseFileInfoTest
    {
        [Test]
        public void FileName_NoFilePath([Values(null, "", " ")] string filePath)
        {
            var bfi = new BaseFileInfo {FilePath = filePath};
            Assert.IsEmpty(bfi.FileName);
        }

        [Test]
        public void FileName_NoFilePathFileName([Values(null, "", " ")] string filePath)
        {
            var bfi = new BaseFileInfo { FilePath = filePath, FileName = "file.txt" };
            Assert.IsNotNull(bfi.FileName);
            Assert.IsNotEmpty(bfi.FileName);
            Assert.AreEqual("file.txt", bfi.FileName);
        }


#if !NETCORE
        [Test]
        public void FileName_FilePath([Values(@"C:\a\b\file.txt", @"$:\s\file.txt", @"\t\file.txt", @".\file.txt", @"file.txt")] string filePath)
        {
            var bfi = new BaseFileInfo { FilePath = filePath };
            Assert.IsNotNull(bfi.FileName);
            Assert.IsNotEmpty(bfi.FileName);
            Assert.AreEqual("file.txt", bfi.FileName);
        }
#endif

        [Test]
        public void FileName_FilePathFileName([Values(@"C:\a\b\file.txt", @"$:\s\file.txt", @"\t\file.txt", @".\file.txt", @"file.txt")] string filePath)
        {
            var bfi = new BaseFileInfo { FilePath = filePath, FileName = "file1.txt" };
            Assert.IsNotNull(bfi.FileName);
            Assert.IsNotEmpty(bfi.FileName);
            Assert.AreEqual("file1.txt", bfi.FileName);
        }
    }
}
