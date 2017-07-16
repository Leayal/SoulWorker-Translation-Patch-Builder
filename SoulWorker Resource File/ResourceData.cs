namespace Leayal.SoulWorker.ResourceFile
{
    public class ResourceData
    {
        public static ResourceData[] Create(DataFormat format)
        {
            ResourceData[] result = format.AllocResourceData();
            for (int i = 0; i < format.Format.Length; i++)
                if (format.Format[i].Type == ResourceFile.DataType.Len || format.Format[i].Type == ResourceFile.DataType.String)
                {
                    result[i] = new ResourceData(null, format.Format[i], format.Format[i+1]);
                    i++;
                }
                else
                    result[i] = new ResourceData(null, format.Format[i]);
            return result;
        }

        internal ResourceData(object value, Data type)
        {
            this.Value = value;
            this.DataType = type;
        }

        internal ResourceData(string value, Data type, Data stringtype)
             : this(value, type)
        {
            this.StringType = stringtype;
        }

        public object Value { get; set; }
        public Data DataType { get; }
        public Data StringType { get; }
    }
}
