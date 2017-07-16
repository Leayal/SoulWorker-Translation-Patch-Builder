using System;
using System.IO;
using System.Text;

namespace Leayal.SoulWorker.ResourceFile
{
    public class ResourceReader : IDisposable
    {
        private BinaryReader reader;
        private bool _leavestreamopen;
        public Stream BaseStream { get; }
        private DataFormat readformat;
        public Encoding Encoding { get; }
        public long? EntryCount { get; }

        public bool EndOfStream
        {
            get
            {
                if (this.Entryposition >= this.EntryCount)
                    return true;
                return false;
                /*if (this.reader.PeekChar() > -1)
                    return false;
                return true;//*/
            }
        }

        public int Entryposition { get; private set; }

        public ResourceReader(Stream resourceStream, DataFormat format) : this(resourceStream, format, false) { }

        public ResourceReader(Stream resourceStream, DataFormat format, Encoding encoding) : this(resourceStream, format, encoding, false) { }

        public ResourceReader(Stream resourceStream, DataFormat format, bool leaveStreamOpen) : this(resourceStream, format, Encoding.Unicode, false) { }

        public ResourceReader(Stream resourceStream, DataFormat format, Encoding encoding, bool leaveStreamOpen)
        {
            if (!resourceStream.CanRead)
                throw new InvalidCastException("Target stream should be writable");
            this.EntryCount = null;
            this.BaseStream = resourceStream;
            if (encoding == null)
                this.reader = new BinaryReader(this.BaseStream);
            else
                this.reader = new BinaryReader(this.BaseStream, encoding);
            this.Encoding = encoding;
            this.readformat = format;
            this.Entryposition = 0;
            this._leavestreamopen = leaveStreamOpen;

            // Pre-read the stream to get the actual entry count
            Data laiwhg = this.readformat.GetCountData();
            if (laiwhg != null)
            {
                switch (laiwhg.Type)
                {
                    case DataType.Byte:
                        this.EntryCount = this.reader.ReadByte();
                        break;
                    case DataType.Short:
                        this.EntryCount = this.reader.ReadInt16();
                        break;
                    case DataType.Integer:
                        this.EntryCount = this.reader.ReadInt32();
                        break;
                    case DataType.Long:
                        this.EntryCount = this.reader.ReadInt64();
                        break;
                }
            }
            else
                throw new InvalidDataException("Format data seem to be wrong.");

        }

        public System.Collections.Generic.List<ResourceData[]> ReadToEnd()
        {
            System.Collections.Generic.List<ResourceData[]> result = new System.Collections.Generic.List<ResourceData[]>();
            for (int i = 0; i < this.EntryCount; i++)
            {
                result.Add(this.ReadEntry());
            }
            return result;
        }

        public ResourceData[] ReadEntry()
        {
            Data currentData, lendata;
            Int32 currentLenLength = 0;
            System.Collections.Generic.List<ResourceData> result = new System.Collections.Generic.List<ResourceData>(this.readformat.Format.Length);
            for (int i = 0; i < this.readformat.Format.Length; i++)
            {
                currentData = this.readformat.Format[i];
                if (currentData.NodeType != DataNode.Count)
                {
                    switch (currentData.Type)
                    {
                        case DataType.Byte:
                            result.Add(new ResourceData(this.reader.ReadByte(), currentData));
                            break;
                        case DataType.Short:
                            result.Add(new ResourceData(this.reader.ReadUInt16(), currentData));
                            break;
                        case DataType.Integer:
                            result.Add(new ResourceData(this.reader.ReadUInt32(), currentData));
                            break;
                        case DataType.Long:
                            result.Add(new ResourceData(this.reader.ReadUInt64(), currentData));
                            break;
                        case DataType.Len:
                            lendata = this.readformat.Format[++i];
                            // how long the string is ????
                            switch (lendata.Type)
                            {
                                case DataType.Byte:
                                    currentLenLength = this.reader.ReadByte();
                                    break;
                                case DataType.Short:
                                    currentLenLength = this.reader.ReadUInt16();
                                    break;
                                case DataType.Integer:
                                    currentLenLength = Convert.ToInt32(this.reader.ReadUInt32());
                                    break;
                                case DataType.Long:
                                    currentLenLength = Convert.ToInt32(this.reader.ReadUInt64());
                                    break;
                            }
                            result.Add(new ResourceData(this.Encoding.GetString(this.reader.ReadBytes(currentLenLength * 2)), currentData, lendata));
                            break;
                    }
                }
            }
            this.Entryposition++;
            return result.ToArray();
        }

        public void Dispose()
        {
            if (!this._leavestreamopen)
                this.reader.Dispose();
        }
    }
}
