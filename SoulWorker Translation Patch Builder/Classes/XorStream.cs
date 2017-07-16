using System;
using System.IO;

namespace SoulWorker_Translation_Patch_Builder.Classes
{
    class XorStream : Stream
    {
        private bool leaveStreamOpen;
        public XorStream(Stream _baseStream, byte xorbyte) : this(_baseStream, xorbyte, false) { }
        public XorStream(Stream _baseStream, byte xorbyte, bool _leaveStreamOpen)
        {
            this.BaseStream = _baseStream;
            this.leaveStreamOpen = _leaveStreamOpen;
            this.XorByte = xorbyte;
        }

        public Stream BaseStream { get; }
        public byte XorByte { get; }
        public override long Position { get => this.BaseStream.Position; set => this.BaseStream.Position = value; }
        public override long Length => this.BaseStream.Length;
        public override bool CanRead => this.BaseStream.CanRead;
        public override bool CanWrite => this.BaseStream.CanWrite;
        public override bool CanTimeout => this.BaseStream.CanTimeout;
        public override bool CanSeek => this.BaseStream.CanSeek;
        public override int WriteTimeout { get => this.BaseStream.WriteTimeout; set => this.BaseStream.WriteTimeout = value; }
        public override int ReadTimeout { get => this.BaseStream.ReadTimeout; set => this.BaseStream.ReadTimeout = value; }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int result = this.BaseStream.Read(buffer, offset, count);
            this.InternalXorArray(buffer, offset, count);
            return result;
        }

        public override int ReadByte()
        {
            return (base.ReadByte() ^ this.XorByte);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            this.InternalXorArray(buffer, offset, count);
            this.BaseStream.Write(buffer, offset, count);
        }

        public override void WriteByte(byte value)
        {
            value ^= this.XorByte;
            base.WriteByte(value);
        }

        public override void SetLength(long value)
        {
            this.BaseStream.SetLength(value);
        }

        public override void Flush()
        {
            this.BaseStream.Flush();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return this.BaseStream.Seek(offset, origin);
        }

        public override void Close()
        {
            if (!this.leaveStreamOpen)
                this.BaseStream.Close();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                if (!this.leaveStreamOpen)
                    this.BaseStream.Dispose();
            base.Dispose(disposing);
        }

        private void InternalXorArray(byte[] buffer)
        {
            for (int i = 0; i < buffer.Length; i++)
                buffer[i] ^= this.XorByte;
        }
        private void InternalXorArray(byte[] buffer, int offset, int count)
        {
            for (int i = offset; i < (offset + count); i++)
                buffer[i] ^= this.XorByte;
        }
    }
}
