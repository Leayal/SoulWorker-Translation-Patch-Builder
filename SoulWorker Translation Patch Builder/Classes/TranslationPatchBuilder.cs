using System;
using Ionic.Zip;
using Leayal.SoulWorker.ResourceFile;
using System.ComponentModel;
using System.IO;

namespace SoulWorker_Translation_Patch_Builder.Classes
{
    class TranslationPatchBuilder
    {
        private static byte LionGameSecretByte = 0x55; //It's not really secret, right ????
        // private bool _isbusy;
        private BackgroundWorker bWorker;

        public TranslationPatchBuilder()
        {
            this.OutputPath = string.Empty;
            this.bWorker = new BackgroundWorker();
            this.bWorker.WorkerReportsProgress = false;
            this.bWorker.WorkerSupportsCancellation = true;
            this.bWorker.RunWorkerCompleted += this.BWorker_RunWorkerCompleted;
            this.bWorker.DoWork += this.BWorker_DoWork;
        }

        #region "Properties"
        public bool IsBusy => this.bWorker.IsBusy;

        public string OutputPath { get; set; }
        #endregion

        #region "Build"
        public void CancelAsync()
        {
            if (this.IsBusy)
                this.bWorker.CancelAsync();
        }

        public void BuildAsync(TranslationResource translationResource, string gameDirectory)
        {
            if (!this.IsBusy)
            {
                if (string.IsNullOrWhiteSpace(this.OutputPath))
                    throw new ArgumentNullException("OutputPath property shouldn't be null or empty.");

                if (translationResource == null)
                    throw new ArgumentNullException("TranslationResource property shouldn't be null.");

                this.OnBuildStarted(EventArgs.Empty);
                this.bWorker.RunWorkerAsync(new WorkerArguments(this.OutputPath, translationResource, gameDirectory));
            }
        }

        private void BWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            WorkerArguments args = e.Argument as WorkerArguments;
            if (args == null)
                throw new InvalidOperationException("Something went wrong, this is out of design.");

            var fileinfo = args.TranslationResource.GetCurrentLanguageFile();
            string currentLanguage = args.TranslationResource.SelectedLanguage, 
                currentRegion = args.TranslationResource.SelectedClientRegion,
                gamedata12 = Path.Combine(args.GameDirectory, "datas", "data12.v"),
                outputpath = args.OutputPath;

            // Directly read the game resource and make a copy of the .res file in the memory ???
            if (!File.Exists(gamedata12))
                throw new FileNotFoundException("Can not find original data file.", gamedata12);

            if (!fileinfo.Exists)
                throw new TranslationFileNotFoundException("Can not find translation files.", fileinfo.FullName);

            // Get the file password and make it as a pre-list
            TranslationDescription transDesc;
            System.Collections.Concurrent.ConcurrentDictionary<string, string> encryptions = new System.Collections.Concurrent.ConcurrentDictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            // Read our derped translation files
            using (ZipFile translationzf = ZipFile.Read(fileinfo.FullName))
            {
                // Password file
                using (Stream entryStream = translationzf["datas.ini"].OpenReader())
                using (StreamReader sr = new StreamReader(entryStream))
                {
                    Leayal.Ini.IniFile afile = new Leayal.Ini.IniFile(sr);
                    foreach (var keyValue in afile.GetAllValues("Zip Passwords"))
                        if (!keyValue.Value.IsComment)
                            encryptions.TryAdd(keyValue.Key, keyValue.Value.Value);
                    afile.Close();
                }

                // Translation Description file
                using (Stream entryStream = translationzf["TranslationPackData.ini"].OpenReader())
                using (StreamReader sr = new StreamReader(entryStream))
                {
                    Leayal.Ini.IniFile afile = new Leayal.Ini.IniFile(sr);
                    transDesc = new TranslationDescription(afile);
                    afile.Close();
                }

                using (FileStream fs = File.OpenRead(gamedata12))
                // Wrap the FileStream in XorStream so we can use ZipFile class directly.
                using (XorStream xoredStream = new XorStream(fs, LionGameSecretByte))
                using (ZipFile zf = ZipFile.Read(xoredStream))
                {
                    string[] splittedStr;
                    DataFormatBuilder formatBuilder = new DataFormatBuilder();
                    zf.CompressionLevel = Ionic.Zlib.CompressionLevel.BestSpeed;

                    if (encryptions.ContainsKey("data12"))
                        zf.Password = encryptions["data12"];

                    System.Collections.Generic.Dictionary<string, Leayal.IO.RecyclableMemoryStream> somethingliawhglaihwg = new System.Collections.Generic.Dictionary<string, Leayal.IO.RecyclableMemoryStream>(StringComparer.OrdinalIgnoreCase);

                    for (int i = 0; i < transDesc.Descriptions.Length; i++)
                        if (transDesc.Descriptions[i].TargetData.EndsWith("data12.v", StringComparison.OrdinalIgnoreCase)) // Build only for data12.v
                        {
                            formatBuilder.Clear();
                            
                            if (this.bWorker.CancellationPending)
                                break;

                            this.OnBuildProgressChanged(new ProgressBarValueEventArgs(i + 1, transDesc.Descriptions.Length));
                            TranslationDescription.Description desc = transDesc.Descriptions[i];
                            this.OnBuildStepChanged(new StringEventArgs(Path.GetFileName(desc.PathTXT)));

                            splittedStr = desc.ResourceFormat.Split(' ');

                            formatBuilder.CountDataType = (DataType)Leayal.NumberHelper.Parse(splittedStr[1]);
                            formatBuilder.IDIndex = Leayal.NumberHelper.Parse(splittedStr[0]);

                            for (int splittedCount = 2; splittedCount < splittedStr.Length; splittedCount++)
                                if (!string.IsNullOrEmpty(splittedStr[splittedCount]))
                                    formatBuilder.Append(splittedStr[splittedCount]);

                            DataFormat readformat = formatBuilder.ToDataFormat();

                            TranslationDatabase translatedb;
                            using (Stream transText = translationzf[desc.PathTXT].OpenReader())
                            using (StreamReader sr = new StreamReader(transText))
                            {
                                translatedb = TranslationDatabase.From(sr, readformat);
                            }

                            // Read the source resource here.
                            Ionic.Crc.CrcCalculatorStream dataStream;
                            if (encryptions.ContainsKey("data12"))
                                dataStream = zf[desc.PathInsideArchieve].OpenReader(encryptions["data12"]);
                            else
                                dataStream = zf[desc.PathInsideArchieve].OpenReader();

                            System.Collections.Generic.List<ResourceData[]> databuffer;
                            using (dataStream)
                            using (ResourceReader rr = new ResourceReader(dataStream, readformat, true))
                                databuffer = rr.ReadToEnd();

                            Leayal.IO.RecyclableMemoryStream memoryEntry = new Leayal.IO.RecyclableMemoryStream(desc.PathInsideArchieve);
                            somethingliawhglaihwg.Add(Path.GetFileName(desc.PathInsideArchieve), memoryEntry);

                            ResourceData[] currentnode;
                            using (ResourceWriter sw = new ResourceWriter(memoryEntry, readformat, true))
                                for (int writeEntryCount = 0; writeEntryCount < databuffer.Count; writeEntryCount++)
                                {
                                    currentnode = databuffer[writeEntryCount];

                                    // Apply translation here
                                    currentnode = translatedb.ApplyTranslation(currentnode);

                                    sw.WriteEntry(currentnode);
                                }

                            zf.UpdateEntry(desc.PathInsideArchieve, new WriteDelegate((filename, writeStream) =>
                            {
                                var memStream = somethingliawhglaihwg[Path.GetFileName(filename)];
                                writeStream.Write(memStream.GetBuffer(), 0, (int)memStream.Length);
                            }));


                        }

                    if (!this.bWorker.CancellationPending)
                    {
                        // Get client version (again)
                        Leayal.Ini.IniFile inifile = new Leayal.Ini.IniFile(Path.Combine(args.GameDirectory, "ver.ini"));
                        string clientVer = inifile.GetValue("Client", "ver", string.Empty);
                        if (string.IsNullOrWhiteSpace(clientVer))
                            clientVer = "Unknown";
                        outputpath = Path.Combine(args.OutputPath, $"SoulWorker{args.TranslationResource.SelectedClientRegion.ToUpper()} {args.TranslationResource.SelectedLanguage} Patch for client v{clientVer}.zip");
                        inifile.Close();

                        Microsoft.VisualBasic.FileIO.FileSystem.CreateDirectory(args.OutputPath);
                        File.Delete(outputpath);
                        using (ZipFile outputzf = new ZipFile(outputpath, System.Text.Encoding.UTF8))
                        {
                            outputzf.AddEntry($"{args.TranslationResource.SelectedLanguage}\\data12.v", new WriteDelegate((_entryName, _entryStream) =>
                            {
                                using (XorStream writeout = new XorStream(_entryStream, LionGameSecretByte))
                                {
                                    zf.Save(writeout);
                                }
                            }));
                            outputzf.AddFile(gamedata12, "original");
                            outputzf.AddEntry("version.txt", $"100% compatible with SoulWorker{args.TranslationResource.SelectedClientRegion.ToUpper()} client v{clientVer}", System.Text.Encoding.UTF8);
                            outputzf.Save();
                        }
                    }

                    foreach (Leayal.IO.RecyclableMemoryStream memStream in somethingliawhglaihwg.Values)
                        memStream.Dispose();
                    somethingliawhglaihwg.Clear();
                }

            }

            if (this.bWorker.CancellationPending)
                e.Cancel = true;
            else
                // Return the fullpath of output
                e.Result = new BuildResult(currentRegion, currentLanguage, "PatchVersion", outputpath);
        }

        private void BWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.OnBuildCompleted(new PatchBuildCompletedEventArgs(e.Error, e.Cancelled, e.Result as BuildResult));
        }

        public event EventHandler BuildStarted;
        protected virtual void OnBuildStarted(EventArgs e)
        {
            this.BuildStarted?.Invoke(this, e);
        }

        public event EventHandler<ProgressBarValueEventArgs> BuildProgressChanged;
        protected virtual void OnBuildProgressChanged(ProgressBarValueEventArgs e)
        {
            this.BuildProgressChanged?.Invoke(this, e);
        }

        public event EventHandler<StringEventArgs> BuildStepChanged;
        protected virtual void OnBuildStepChanged(StringEventArgs e)
        {
            this.BuildStepChanged?.Invoke(this, e);
        }

        public event EventHandler<PatchBuildCompletedEventArgs> BuildCompleted;
        protected virtual void OnBuildCompleted(PatchBuildCompletedEventArgs e)
        {
            this.BuildCompleted?.Invoke(this, e);
        }
        #endregion

        #region "Private classes"
        private class WorkerArguments
        {
            public string OutputPath { get; }
            public string GameDirectory { get; }
            public TranslationResource TranslationResource { get; }

            public WorkerArguments(string _outputpath, TranslationResource _translationresource, string gameDirectory)
            {
                this.GameDirectory = gameDirectory;
                this.OutputPath = _outputpath;
                this.TranslationResource = _translationresource;
            }
        }

        internal class BuildResult
        {
            public string SelectedLanguage { get; }
            public string SelectedClientRegion { get; }
            public string LanguageVersion { get; }
            public string OutputPath { get; }
            internal BuildResult(string clientregion, string lang, string langver, string _outputpath)
            {
                this.SelectedLanguage = lang;
                this.SelectedClientRegion = clientregion;
                this.LanguageVersion = langver;
                this.OutputPath = _outputpath;
            }
        }
        #endregion
    }
}
