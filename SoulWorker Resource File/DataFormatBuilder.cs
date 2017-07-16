using System;
using System.Collections.Generic;
using System.Text;

namespace Leayal.SoulWorker.ResourceFile
{
    public class DataFormatBuilder
    {
        private DataType?[] innerData;
        private int currentIndex;
        private int _count;
        public int Count => this._count;

        public DataFormatBuilder()
        {
            this.IDIndex = 0;
            this._count = 0;
        }

        public void Append(string datatype)
        {
            this.Append(Data.DataTypeFromString(datatype));
        }

        public void Append(DataType type)
        {
            if (this.innerData == null)
                this.innerData = new DataType?[5];

            this._count++;
            if (this._count >= this.innerData.Length)
            {
                var newarray = new DataType?[this.innerData.Length * 2];
                Array.Copy(this.innerData, newarray, this.innerData.Length);
                this.innerData = newarray;
            }

            this.innerData[this.currentIndex] = type;
            this.currentIndex++;
        }

        public void Remove(int index)
        {
            this.innerData[index] = null;
        }

        public void Clear()
        {
            if (this._count == 0) return;
            this._count = 0;
            this.currentIndex = 0;
            Array.Clear(this.innerData, 0, this.innerData.Length);
        }

        public int IDIndex { get; set; }
        public DataType CountDataType { get; set; }

        public DataFormat ToDataFormat()
        {
            Data[] output = new Data[this.Count + 1];
            for (int i = 0; i < this.Count; i++)
                if (this.innerData[i].HasValue)
                {
                    if (i == this.IDIndex)
                        output[i] = new Data(this.innerData[i].Value, DataNode.ID);
                    else
                        output[i] = new Data(this.innerData[i].Value);
                }
            DataFormat result = new DataFormat(output);
            result.SetCountData(new Data(this.CountDataType));
            return result;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder((this.Count * 2) + 2);
            sb.Append(this.IDIndex.ToString());
            sb.Append(" " + ((int)this.CountDataType).ToString());
            for (int i = 0; i < this.Count; i++)
                if (this.innerData[i].HasValue)
                    sb.Append(" " + ThrowToString(this.innerData[i].Value));
            return sb.ToString();
        }

        internal static string ThrowToString(DataType dt)
        {
            switch(dt)
            {
                case DataType.Len:
                    return "len";
                case DataType.Byte:
                    return "1";
                case DataType.Short:
                    return "2";
                case DataType.Integer:
                    return "4";
                case DataType.Long:
                    return "8";
                default:
                    throw new Exception();
            }
        }
    }
}
