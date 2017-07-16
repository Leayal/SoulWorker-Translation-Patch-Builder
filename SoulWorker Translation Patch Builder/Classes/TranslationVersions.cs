using System;
using System.IO;
using System.Xml.Linq;

namespace SoulWorker_Translation_Patch_Builder.Classes
{
    class TranslationVersions : IDisposable
    {
        private Leayal.Ini.IniFile memoryIni;
        public TranslationVersions(string xmldata)
        {
            this.memoryIni = new Leayal.Ini.IniFile(string.Empty);
            this.ConvertXMLtoINI(xmldata);
        }

        private void ConvertXMLtoINI(string xmldata)
        {
            XDocument document = XDocument.Parse(xmldata);
            if (document == null)
                throw new InvalidDataException("Can not parse the xml from the server.");
            XElement element = document.Element("soulworkerPatcher"), languageInRegion, valueInLanguage;
            XAttribute langName;
            if (element == null)
                throw new InvalidDataException("Can not read the xml from the server.");

            element = element.Element("regions");
            if (element == null)
                throw new InvalidDataException("Can not read the xml from the server.");


            foreach (XElement region in element.Elements())
            {
                languageInRegion = region.Element("languages");
                if (languageInRegion == null)
                    // throw new InvalidDataException($"Can not read the region {region} from the XML.");
                    continue;
                foreach (XElement lang in languageInRegion.Elements())
                {
                    langName = lang.Attribute("name");
                    if (langName != null)
                    {
                        valueInLanguage = lang.Element("value");
                        this.memoryIni.SetValue(region.Name.LocalName, langName.Value, valueInLanguage != null ? valueInLanguage.Value : string.Empty);
                    }
                }
            }

            // Debugging purpose
            // this.memoryIni.SaveAs("laiwghlahg.ini");
        }

        public DateTime? GetVersion(string clientRegion, string language)
        {
            string datet = this.memoryIni.GetValue(clientRegion, language, string.Empty);
            if (string.IsNullOrEmpty(datet))
                return null;

            try
            { return DateTime.ParseExact(datet, "d/MMM/yyyy h:mm tt", System.Globalization.CultureInfo.InvariantCulture); }
            catch
            { return null; }
        }

        public System.Collections.ObjectModel.ReadOnlyCollection<string> GetClientRegions() => this.memoryIni.Sections;

        public string[] GetRegionLanguages(string clientregion)
        {
            return System.Linq.Enumerable.ToArray(this.memoryIni.GetAllValues(clientregion).Keys);
        }

        public void Dispose()
        {
            this.memoryIni.Close();
        }
    }
}
