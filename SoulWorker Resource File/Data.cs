namespace Leayal.SoulWorker.ResourceFile
{
    public class Data
    {
        internal static DataType DataTypeFromString(string rawdata)
        {
            switch (rawdata.ToLower())
            {
                case "len":
                    return DataType.Len;
                case "string":
                    return DataType.Len;
                case "1":
                    return DataType.Byte;
                case "2":
                    return DataType.Short;
                case "4":
                    return DataType.Integer;
                case "8":
                    return DataType.Long;
                default:
                    throw new System.Exception("Something freaking thing went wrong with your data format.");
            }
        }

        internal Data(string _datatype) : this(_datatype, DataNode.None) { }
        internal Data(string _datatype, DataNode dataNode) : this(DataTypeFromString(_datatype), dataNode) { }


        internal Data(int _datatype) : this(_datatype, DataNode.None) { }
        internal Data(int _datatype, DataNode dataNode) : this((DataType)_datatype, dataNode) { }
        internal Data(DataType _datatype) : this((DataType)_datatype, DataNode.None) { }
        internal Data(DataType _datatype, DataNode dataNode)
        {
            this._nodetype = dataNode;
            this.Type = _datatype;
        }
        
        internal void SetNodeType(DataNode type)
        {
            this._nodetype = type;
        }

        private DataNode _nodetype;
        public DataNode NodeType => this._nodetype;
        public DataType Type { get; }
    }
}
