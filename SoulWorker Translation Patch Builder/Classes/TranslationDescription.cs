using System;
using System.Collections.Generic;

namespace SoulWorker_Translation_Patch_Builder.Classes
{
    class TranslationDescription
    {
        private List<Description> descripList;
        private Leayal.Ini.IniFile inifile;

        internal TranslationDescription(Leayal.Ini.IniFile rawdata)
        {
            this.inifile = rawdata;
            this.descripList = new List<Description>();
            var sections = rawdata.Sections;
            if (sections.Count > 0)
            {
                foreach (string section in sections)
                {
                    this.descripList.Add(new Description(section,
                        rawdata.GetValue(section, "path", string.Empty),
                        rawdata.GetValue(section, "path_a", string.Empty),
                        rawdata.GetValue(section, "path_d", string.Empty),
                        rawdata.GetValue(section, "format", string.Empty)
                        ));
                }
            }
        }

        public override string ToString()
        {
            return this.inifile.ToString();
        }

        private Description[] _descriptions;
        public Description[] Descriptions
        {
            get
            {
                if (this._descriptions == null)
                    this._descriptions = this.descripList.ToArray();
                return this._descriptions;
            }
        }

        public class Description
        {
            //IniSection
            public string Name { get; }
            //path
            public string TargetData { get; }
            //path_a
            public string PathInsideArchieve { get; }
            //path_d
            public string PathTXT { get; }
            //format
            public string ResourceFormat { get; }

            internal Description(string _name, string path, string path_a, string path_d, string format)
            {
                this.Name = _name;
                this.TargetData = path;
                this.PathInsideArchieve = path_a;
                this.PathTXT = path_d;
                this.ResourceFormat = format;
            }
        }
    }
}
