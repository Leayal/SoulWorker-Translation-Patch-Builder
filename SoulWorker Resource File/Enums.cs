namespace Leayal.SoulWorker.ResourceFile
{
    public enum DataType : byte
    {
        // These two are one
        String = 0,
        Len = 0,
        Byte = 1,
        Short = 2,
        Integer = 4,
        Long = 8
    }

    public enum DataNode : byte
    {
        None = 0,
        ID,
        Count
    }
}
