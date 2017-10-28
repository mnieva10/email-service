using System.IO;

namespace Sovos.SvcBus.Common.Model.Infrastructure.Pooling
{
    public class PooledStream<T> : Stream
    {
        private readonly T _item;
        private readonly Pool<T> _pool;
        public Stream Stream { get; set; }
        private bool _isDisposed;

        public PooledStream(Stream stream, T item, Pool<T> pool)
        {
            _item = item;
            _pool = pool;
            Stream = stream;
            _isDisposed = false;
        }

        public override void Flush()
        {
            Stream.Flush();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return Stream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            Stream.SetLength(value);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return Stream.Read(buffer, offset, count);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            Stream.Write(buffer, offset, count);
        }

        public override bool CanRead
        {
            get { return Stream.CanRead; }
        }

        public override bool CanSeek
        {
            get { return Stream.CanSeek; }
        }

        public override bool CanWrite
        {
            get { return Stream.CanWrite; }
        }

        public override long Length
        {
            get { return Stream.Length; }
        }

        public override long Position
        {
            get { return Stream.Position; } 
            set { Stream.Position = value; }
        }

        ~PooledStream()
        {
            Dispose(false);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (_isDisposed) return;
            Stream.Dispose();
            _pool.Release(_item);
            _isDisposed = true;
        }
    }
}
