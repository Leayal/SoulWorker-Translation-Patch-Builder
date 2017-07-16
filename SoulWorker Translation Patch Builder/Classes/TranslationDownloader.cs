using System;
using Leayal;
using Leayal.Net;
using System.Text;
using System.IO;
using System.Collections.Concurrent;

namespace SoulWorker_Translation_Patch_Builder.Classes
{
    /// <summary>
    /// Should this class really be existed ???
    /// </summary>
    class TranslationDownloader
    {
        private static readonly Uri LanguagePacks = new Uri("https://raw.githubusercontent.com/Miyuyami/SoulWorkerHQTranslations/master/LanguagePacks.xml");
        private const string RootURL = "https://raw.githubusercontent.com/Miyuyami/SoulWorkerHQTranslations/master";

        private WebClient myWebClient;
        public TranslationDownloader()
        {
            this.myWebClient = new WebClient(5000);
            this.myWebClient.CacheStorage = new CacheStorage(System.IO.Path.Combine(AppInfo.AssemblyInfo.DirectoryPath, "cache"));
            this.myWebClient.DownloadStringCompleted += this.MyWebClient_DownloadStringCompleted;
            this.myWebClient.DownloadToMemoryCompleted += this.MyWebClient_DownloadToMemoryCompleted;
        }

        public bool IsBusy => this.myWebClient.IsBusy;
        public void CancelAsync()
        {
            if (this.IsBusy)
                this.myWebClient.CancelAsync();
        }

        public void CheckForTranslationVersionsAsync()
        {
            if (!this.IsBusy)
                this.myWebClient.DownloadStringAsync(LanguagePacks, DownloadStringStep.CheckForUpdate);
        }

        public event EventHandler<CheckForTranslationVersionCompletedEventArgs> CheckForTranslationVersionCompleted;
        private void MyWebClient_DownloadStringCompleted(object sender, System.Net.DownloadStringCompletedEventArgs e)
        {
            DownloadMeta meta = e.UserState as DownloadMeta;
            if (e.Error != null)
            {
                if (meta != null)
                {
                }
                else
                {
                    DownloadStringStep step = (DownloadStringStep)e.UserState;
                    switch (step)
                    {
                        case DownloadStringStep.CheckForUpdate:
                            this.CheckForTranslationVersionCompleted?.Invoke(this, new CheckForTranslationVersionCompletedEventArgs(e.Error, e.Cancelled, new TranslationVersions(e.Result)));
                            break;
                    }
                }
            }
            else if (e.Cancelled)
            {
                if (meta != null)
                {
                }
                else
                {
                    DownloadStringStep step = (DownloadStringStep)e.UserState;
                    switch (step)
                    {
                        case DownloadStringStep.CheckForUpdate:
                            this.CheckForTranslationVersionCompleted?.Invoke(this, new CheckForTranslationVersionCompletedEventArgs(e.Error, e.Cancelled, new TranslationVersions(e.Result)));
                            break;
                    }
                }
            }
            else
            {
                if (meta != null)
                {
                    switch (meta.Step)
                    {
                        case DownloadStringStep.GetLanguagesData:
                            meta.SetDataEncryption(e.Result);
                            meta.SetStep(DownloadStringStep.GetLanguageDescription);
                            this.myWebClient.DownloadStringAsync(new Uri(Leayal.UriHelper.URLConcat(RootURL, meta.SelectedRegion, meta.SelectedLanguage, "TranslationPackData.ini")), meta);
                            break;
                        case DownloadStringStep.GetLanguageDescription:
                            meta.SetTranslationDescription(e.Result);
                            meta.SetStep(DownloadStringStep.DownloadTXTs);
                            this.DownloadTXTs(meta);
                            break;
                    }
                }
                else
                {
                    DownloadStringStep step = (DownloadStringStep)e.UserState;
                    switch (step)
                    {
                        case DownloadStringStep.CheckForUpdate:
                            this.CheckForTranslationVersionCompleted?.Invoke(this, new CheckForTranslationVersionCompletedEventArgs(e.Error, e.Cancelled, new TranslationVersions(e.Result)));
                            break;
                    }
                }
            }
        }

        public void DownloadTranslationAsync(string language, DateTime? dateVersion)
        {
            this.DownloadTranslationAsync("jp", language, dateVersion);
        }

        public void DownloadTranslationAsync(string clientregion, string language, DateTime? dateVersion)
        {
            if (!this.IsBusy)
                this.myWebClient.DownloadStringAsync(new Uri(Leayal.UriHelper.URLConcat(RootURL, clientregion, "datas.ini")), new DownloadMeta(DownloadStringStep.GetLanguagesData, clientregion, language, dateVersion));
        }

        private void DownloadTXTs(DownloadMeta meta)
        {
            DownloadList dl = new DownloadList(meta, this.GetFileList(meta));
            var something = dl.GetNext();
            if (something.HasValue)
            {
                this.OnDownloadBegin(new StringEventArgs(Path.GetFileName(something.Value.Key)));
                this.OnDownloadTranslationProgressChanged(new DownloadTranslationProgressChangedEventArgs(dl.CurrentCount, dl.TotalCount));
                this.myWebClient.DownloadToMemoryAsync(something.Value.Value, something.Value.Key, dl);
            }
        }

        private System.Collections.Generic.Dictionary<string, Uri> GetFileList(DownloadMeta meta)
        {
            TranslationDescription.Description desc;
            System.Collections.Generic.Dictionary<string, Uri> result = new System.Collections.Generic.Dictionary<string, Uri>();
            for (int i = 0; i < meta.TranslationDescription.Descriptions.Length; i++)
            {
                desc = meta.TranslationDescription.Descriptions[i];
                if (desc.TargetData.EndsWith("data12.v", StringComparison.OrdinalIgnoreCase))
                    result.Add(desc.PathTXT, new Uri(Leayal.UriHelper.URLConcat(RootURL, meta.SelectedRegion, meta.SelectedLanguage, desc.PathTXT)));
            }
            return result;
        }

        private void MyWebClient_DownloadToMemoryCompleted(object sender, DownloadToMemoryCompletedEventArgs e)
        {
            // e.Tag
            DownloadList dl = e.UserState as DownloadList;
            if (e.Error != null)
                this.OnDownloadTranslationCompleted(new System.ComponentModel.AsyncCompletedEventArgs(e.Error, false, null));
            else if (e.Cancelled)
                this.OnDownloadTranslationCompleted(new System.ComponentModel.AsyncCompletedEventArgs(null, true, null));
            else
            {
                // Throw the memory to the archive then free it
                // e.Result
                System.Threading.ThreadPool.QueueUserWorkItem(new System.Threading.WaitCallback(delegate 
                {
                    dl.ZipFile.AddEntry(dl.CurrentItem.Value.Key, e.Result);
                    /*var entry = dl.ZipFile.AddEntry(dl.CurrentItem.Value.Key, new Ionic.Zip.WriteDelegate((_string, _stream) =>
                    {
                        _stream.Write(e.Result.GetBuffer(), 0, (int)e.Result.Length);
                    }));*/
                    dl.ZipFile.Save();
                    e.Result.Dispose();
                    var something = dl.GetNext();
                    if (something != null && something.HasValue)
                    {
                        this.OnDownloadBegin(new StringEventArgs(Path.GetFileName(something.Value.Key)));
                        this.OnDownloadTranslationProgressChanged(new DownloadTranslationProgressChangedEventArgs(dl.CurrentCount, dl.TotalCount));
                        this.myWebClient.DownloadToMemoryAsync(something.Value.Value, something.Value.Key, dl);
                    }
                    else
                    {
                        dl.ZipFile.AddEntry("TranslationPackData.ini", dl.Meta.TranslationDescription.ToString());
                        dl.ZipFile.AddEntry("datas.ini", dl.Meta.DataEncryption.ToString());
                        dl.ZipFile.Save();
                        dl.ZipFile.Dispose();
                        File.Delete(dl.OutputPath);
                        File.Move(dl.ZipPath, dl.OutputPath);
                        if (dl.Meta.VersionDate.HasValue)
                            File.SetLastWriteTime(dl.OutputPath, dl.Meta.VersionDate.Value);
                        
                        this.OnDownloadTranslationCompleted(new System.ComponentModel.AsyncCompletedEventArgs(null, false, dl.Meta));
                    }
                }), null);
            }
        }

        public event EventHandler<DownloadTranslationProgressChangedEventArgs> DownloadTranslationProgressChanged;
        protected virtual void OnDownloadTranslationProgressChanged(DownloadTranslationProgressChangedEventArgs e)
        {
            this.DownloadTranslationProgressChanged?.Invoke(this, e);
        }

        public event EventHandler<StringEventArgs> DownloadBegin;
        protected virtual void OnDownloadBegin(StringEventArgs e)
        {
            this.DownloadBegin?.Invoke(this, e);
        }

        public event System.ComponentModel.AsyncCompletedEventHandler DownloadTranslationCompleted;
        protected virtual void OnDownloadTranslationCompleted(System.ComponentModel.AsyncCompletedEventArgs e)
        {
            this.DownloadTranslationCompleted?.Invoke(this, e);
        }

        private enum DownloadStringStep : byte
        {
            CheckForUpdate = 0,
            GetLanguagesData,
            GetLanguageDescription,
            DownloadTXTs
        }

        private class DownloadMeta
        {
            private DownloadStringStep _step;
            public DownloadStringStep Step => this._step;
            public string SelectedLanguage { get; }
            public string SelectedRegion { get; }
            public DateTime? VersionDate { get; }

            public DownloadMeta(DownloadStringStep step, string selectedregion, string selectedlanguage, DateTime? version)
            {
                this.VersionDate = version;
                this._step = step;
                this.SelectedLanguage = selectedlanguage;
                this.SelectedRegion = selectedregion;
            }

            internal void SetStep(DownloadStringStep step)
            {
                this._step = step;
            }

            internal void SetDataEncryption(string rawdataencryption)
            {
                using (StringReader sr = new StringReader(rawdataencryption))
                    this._dataencryption = new Leayal.Ini.IniFile(sr);
            }

            internal void SetTranslationDescription(string rawtranslationdescription)
            {
                using (StringReader sr = new StringReader(rawtranslationdescription))
                    this._translationdescription = new TranslationDescription(new Leayal.Ini.IniFile(sr));
            }

            private Leayal.Ini.IniFile _dataencryption;
            public Leayal.Ini.IniFile DataEncryption => this._dataencryption;
            private TranslationDescription _translationdescription;
            public TranslationDescription TranslationDescription => this._translationdescription;
        }

        private class DownloadList
        {
            private ConcurrentBag<string> queue;
            private ConcurrentDictionary<string, Uri> dict;
            public DownloadMeta Meta { get; }
            public Ionic.Zip.ZipFile ZipFile { get; }
            internal string OutputPath { get; }
            internal string ZipPath { get; }

            public int CurrentCount { get; private set; }
            public int TotalCount { get; private set; }

            public DownloadList(DownloadMeta meta, System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, Uri>> list)
            {
                this._currentitem = null;
                this.Meta = meta;
                string zipfolder = System.IO.Path.Combine(Leayal.AppInfo.AssemblyInfo.DirectoryPath, "translation", meta.SelectedRegion);
                Microsoft.VisualBasic.FileIO.FileSystem.CreateDirectory(zipfolder);
                this.OutputPath = System.IO.Path.Combine(zipfolder, meta.SelectedLanguage + ".zip");
                this.ZipPath = Path.ChangeExtension(this.OutputPath, "downloading");
                File.Delete(this.ZipPath);
                this.ZipFile = new Ionic.Zip.ZipFile(this.ZipPath, Encoding.UTF8)
                {
                    CompressionMethod = Ionic.Zip.CompressionMethod.Deflate,
                    CompressionLevel = Ionic.Zlib.CompressionLevel.BestCompression
                };
                if (list != null)
                {
                    this.dict = new ConcurrentDictionary<string, Uri>(list);
                    this.queue = new ConcurrentBag<string>(this.dict.Keys);
                    this.TotalCount = this.dict.Count;
                }
                else
                {
                    this.dict = new ConcurrentDictionary<string, Uri>();
                    this.queue = new ConcurrentBag<string>();
                    this.TotalCount = 0;
                }
            }

            public bool AddQueue(string filename, Uri URL)
            {
                this.TotalCount++;
                return this.dict.TryAdd(filename, URL);
            }

            public void Update(string filename, Uri URL)
            {
                this.dict[filename] = URL;
            }

            internal System.Collections.Generic.KeyValuePair<string, Uri>? GetNext()
            {
                if (this.queue.TryTake(out string result))
                    if (this.dict.TryGetValue(result, out Uri value))
                    {
                        this.CurrentCount++;
                        this._currentitem = new System.Collections.Generic.KeyValuePair<string, Uri>(result, value);
                        return this._currentitem;
                    }
                return null;
            }

            private System.Collections.Generic.KeyValuePair<string, Uri>? _currentitem;
            public System.Collections.Generic.KeyValuePair<string, Uri>? CurrentItem => this._currentitem;
        }
    }
}
