using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Leayal.SoulWorker.ResourceFile
{
    public class DataFormat
    {
        internal DataFormat(IEnumerable<Data> list)
        {
            this.myFormat = list.Where((x) => { return x != null; }).ToArray();
        }

        private int knownindexid = -1;
        public int GetIDIndex()
        {
            if (this.knownindexid > -1)
                return this.knownindexid;
            for (int i = 0; i < this.Format.Length; i++)
                if (this.Format[i].NodeType == DataNode.ID)
                    this.knownindexid = i;
            return this.knownindexid;
        }

        public Data GetIDData()
        {
            int something = this.GetIDIndex();
            if (something > -1)
                return this.Format[something];
            else
                return null;
        }

        private Data _countData;
        internal void SetCountData(Data countdata)
        {
            this._countData = countdata;
        }
        public Data GetCountData()
        {
            return this._countData;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder((this.myFormat.Length * 2) + 2);
            sb.Append(this.GetIDIndex().ToString());
            for (int i = 0; i < this.myFormat.Length; i++)
                sb.Append(" " + DataFormatBuilder.ThrowToString(this.myFormat[i].Type));
            return sb.ToString();
        }

        private Data[] myFormat;
        public Data[] Format => this.myFormat;

        private int _knownfixedlength = -1;
        private int GetFixedLength()
        {
            if (this._knownfixedlength > -1)
                return this._knownfixedlength;
            this._knownfixedlength = 0;
            for (int i = 0; i < this.Format.Length; i++)
                if (this.Format[i].Type != DataType.Len && this.Format[i].NodeType != DataNode.Count)
                    this._knownfixedlength++;
            return this._knownfixedlength;
        }

        internal ResourceData[] AllocResourceData()
        {
            return new ResourceData[this.GetFixedLength()];
        }
    }
}
