using System;
using System.IO;
using System.Text;

namespace Leayal.SoulWorker.ResourceFile
{
    public class ResourceWriter : IDisposable
    {

        private BinaryWriter writer;
        private bool _leavestreamopen;
        public Stream BaseStream { get; }
        public Encoding Encoding { get; }
        private UInt64 dataSum;
        private DataFormat stucture;
        private UInt64 entryCount;
        private System.Security.Cryptography.MD5 hasher;

        public ResourceWriter(Stream resourceStream, DataFormat format) : this(resourceStream, format, false) { }

        public ResourceWriter(Stream resourceStream, DataFormat format, Encoding encoding) : this(resourceStream, format, encoding, false) { }

        public ResourceWriter(Stream resourceStream, DataFormat format, bool leaveStreamOpen) : this(resourceStream, format, Encoding.Unicode, leaveStreamOpen) { }

        public ResourceWriter(Stream resourceStream, DataFormat format, Encoding encoding, bool leaveStreamOpen)
        {
            if (!resourceStream.CanWrite)
                throw new InvalidCastException("Target stream must be writable");
            if (!resourceStream.CanRead)
                throw new InvalidCastException("Target stream must be readable");

            this.BaseStream = resourceStream;
            this.writer = new BinaryWriter(this.BaseStream, encoding);
            this.Encoding = encoding;
            this._leavestreamopen = leaveStreamOpen;
            this.dataSum = 0;
            this.entryCount = 0;
            this.stucture = format;
            this.hasher = System.Security.Cryptography.MD5.Create();
            long streamcountoffset = (long)this.stucture.GetCountData().Type;
            if (this.BaseStream.Length <= streamcountoffset)
                this.BaseStream.SetLength(streamcountoffset + 1);
            if (this.BaseStream.Position == 0)
            {
                switch (this.stucture.GetCountData().Type)
                {
                    case DataType.Byte:
                        this.writer.Write((byte)0);
                        break;
                    case DataType.Short:
                        this.writer.Write((ushort)0);
                        break;
                    case DataType.Integer:
                        this.writer.Write((uint)0);
                        break;
                    case DataType.Long:
                        this.writer.Write((ulong)0);
                        break;
                }
            }
            else if (this.BaseStream.Position < streamcountoffset)
                this.BaseStream.Position = streamcountoffset;
            this._disposed = false;
        }

        public void WriteEntry(ResourceData[] datas)
        {
            ResourceData currentData;
            byte[] somebytes;
            string currentString;

            for (int i = 0; i < datas.Length; i++)
            {
                currentData = datas[i];
                switch (currentData.DataType.Type)
                {
                    case DataType.Byte:
                        this.writer.Write((byte)currentData.Value);
                        this.dataSum += (byte)currentData.Value;
                        break;
                    case DataType.Short:
                        this.writer.Write((ushort)currentData.Value);
                        this.dataSum += (ushort)currentData.Value;
                        break;
                    case DataType.Integer:
                        this.writer.Write((uint)currentData.Value);
                        this.dataSum += (uint)currentData.Value;
                        break;
                    case DataType.Long:
                        this.writer.Write((ulong)currentData.Value);
                        this.dataSum += (ulong)currentData.Value;
                        break;
                    case DataType.Len:
                        currentString = (string)currentData.Value;
                        somebytes = this.Encoding.GetBytes(currentString);
                        switch (currentData.StringType.Type)
                        {
                            case DataType.Byte:
                                this.writer.Write((byte)currentString.Length);
                                this.dataSum += (byte)currentString.Length;
                                break;
                            case DataType.Short:
                                this.writer.Write((short)currentString.Length);
                                this.dataSum += (ushort)currentString.Length;
                                break;
                            case DataType.Integer:
                                this.writer.Write((int)currentString.Length);
                                this.dataSum += (uint)currentString.Length;
                                break;
                            case DataType.Long:
                                this.writer.Write((long)currentString.Length);
                                this.dataSum += (ulong)currentString.Length;
                                break;
                        }

                        for (int iStr = 0; iStr < somebytes.Length; iStr++)
                        {
                            this.writer.Write(somebytes[iStr]);
                            this.dataSum += somebytes[iStr];
                        }

                        break;
                }
            }
            this.entryCount++;
        }

        private void WriteFinaleFooter()
        {
            long currentPos = this.BaseStream.Position;
            this.BaseStream.Position = 0;
            switch (this.stucture.GetCountData().Type)
            {
                case DataType.Byte:
                    this.writer.Write((byte)this.entryCount);
                    break;
                case DataType.Short:
                    this.writer.Write((ushort)this.entryCount);
                    break;
                case DataType.Integer:
                    this.writer.Write((uint)this.entryCount);
                    break;
                case DataType.Long:
                    this.writer.Write((ulong)this.entryCount);
                    break;
            }
            
            string hash = Leayal.Security.Cryptography.MD5Wrapper.FromString(Convert.ToString(this.dataSum));
            hash = hash.ToLower();
            this.BaseStream.Position = currentPos;
            // Use 2-byte ushort to tell how long the hash is
            this.writer.Write(Convert.ToUInt16(hash.Length));
            // Write the hash in <System.Text.ASCIIEncoding.ASCII> encoding.
            for (int i = 0; i < hash.Length; i++)
                this.writer.Write(Convert.ToByte(hash[i]));
        }

        public void Close()
        {
            this.Dispose();
        }

        private bool _disposed;
        public void Dispose()
        {
            if (this._disposed) return;
            this._disposed = true;
            this.WriteFinaleFooter();
            this.hasher.Dispose();
            if (!this._leavestreamopen)
                this.BaseStream.Dispose();
        }
    }
}
