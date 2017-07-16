using System;
using System.Collections.Generic;
using System.IO;

namespace SoulWorker_Translation_Patch_Builder.Classes
{
    class TranslationDatabase
    {
        private int idindex = -1;
        private Leayal.SoulWorker.ResourceFile.DataFormat readformat;

        public static TranslationDatabase From(TextReader textReader, Leayal.SoulWorker.ResourceFile.DataFormat format)
        {
            return new TranslationDatabase(textReader, format);
        }

        public static TranslationDatabase From(string content, Leayal.SoulWorker.ResourceFile.DataFormat format)
        {
            using (StreamReader sr = new StreamReader(content))
                return TranslationDatabase.From(sr, format);
        }

        public int Capacity { get; set; }
        private Dictionary<UInt64, string[]> dict;

        public TranslationDatabase(System.IO.TextReader textReader, Leayal.SoulWorker.ResourceFile.DataFormat format)
        {
            this.Capacity = format.Format.Length;
            this.readformat = format;
            this.ReadContent(textReader);
        }

        private int SearchForIDIndex()
        {
            if (this.idindex > -1)
                return this.idindex;
            for (int i = 0; i < this.readformat.Format.Length; i++)
                if (this.readformat.Format[i].NodeType == Leayal.SoulWorker.ResourceFile.DataNode.ID)
                    this.idindex = i;
            return this.idindex;
        }

        public Leayal.SoulWorker.ResourceFile.ResourceData[] ApplyTranslation(Leayal.SoulWorker.ResourceFile.ResourceData[] originalData)
        {
            int IDindex = this.SearchForIDIndex();

            if (IDindex == -1)
                for (int i = 0; i < originalData.Length; i++)
                    if (originalData[i].DataType.NodeType == Leayal.SoulWorker.ResourceFile.DataNode.ID)
                        IDindex = i;

            if (IDindex == -1)
                return originalData;

            UInt64 targetID = Convert.ToUInt64(originalData[IDindex].Value);
            if (!this.dict.ContainsKey(targetID))
                return originalData;
            string lawhgliawg;

            string[] translation = this.GetTranslationFromID(targetID);

            int lencount = 0;

            for (int i = 0; i < originalData.Length; i++)
            {
                if (originalData[i].DataType.Type == Leayal.SoulWorker.ResourceFile.DataType.Len || originalData[i].DataType.Type == Leayal.SoulWorker.ResourceFile.DataType.String)
                {
                    lawhgliawg = translation[lencount];
                    // SW's res file has string limit which only contains 512 chars in the array.
                    if (!string.IsNullOrEmpty(lawhgliawg) && lawhgliawg.Length < 512)
                        originalData[i].Value = lawhgliawg;

                    lencount++;
                }
            }

            return originalData;
        }

        public string[] GetTranslationFromID(UInt64 id)
        {
            return this.dict[id];
        }

        public void Clear()
        {
            this.dict.Clear();
        }

        private void ReadContent(System.IO.TextReader textReader)
        {
            this.dict = new Dictionary<UInt64, string[]>();
            
            string currentString = string.Empty;
            string[] splitted = null;
            string[] currentitem = null;

            string currentline;
            int linecount = 0;

            while (textReader.Peek() > -1)
            {
                currentline = textReader.ReadLine();
                if (!string.IsNullOrWhiteSpace(currentline))
                {
                    if (currentline.StartsWith("id=", StringComparison.OrdinalIgnoreCase))
                    {
                        splitted = currentline.Split('=');
                        if (splitted.Length == 2)
                        {
                            currentitem = new string[this.Capacity];
                            linecount = 0;
                            this.dict.Add(UInt64.Parse(splitted[1]), currentitem);
                        }
                    }
                    else if (currentitem != null)
                    {
                        if (currentline == "0")
                            currentitem[linecount] = string.Empty;
                        else
                            currentitem[linecount] = currentline;
                        linecount++;
                    }
                }
            }
        }


    }
}
