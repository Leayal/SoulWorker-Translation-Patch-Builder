using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace SoulWorker_Translation_Patch_Builder.Classes
{
    class TranslationResource
    {
        private TranslationDownloader downloader;

        public TranslationResource()
        {
            this.downloader = new TranslationDownloader();
            this.downloader.CheckForTranslationVersionCompleted += this.Downloader_CheckForTranslationVersionCompleted;
            this.downloader.DownloadTranslationProgressChanged += this.Downloader_DownloadTranslationProgressChanged;
            this.downloader.DownloadTranslationCompleted += this.Downloader_DownloadTranslationCompleted;
            this.downloader.DownloadBegin += this.Downloader_DownloadBegin;
        }

        public bool IsBusy => this.downloader.IsBusy;
        public string SelectedLanguage { get; set; }
        public string SelectedClientRegion { get; set; }

        public void CancelAsync()
        {
            if (this.IsBusy)
                this.downloader.CancelAsync();
        }

        public FileInfo GetCurrentLanguageFile()
        {
            return this.GetLanguageFile(this.SelectedClientRegion, this.SelectedLanguage);
        }

        public FileInfo GetLanguageFile(string clientregion, string translation)
        {
            return new FileInfo(System.IO.Path.Combine(Leayal.AppInfo.AssemblyInfo.DirectoryPath, "translation", clientregion, translation + ".zip"));
        }

        public bool IsCurrentLanguageFileExists()
        {
            return this.IsLanguageFileExists(this.SelectedClientRegion, this.SelectedLanguage);
        }

        public bool IsLanguageFileExists(string clientregion, string translation)
        {
            return File.Exists(System.IO.Path.Combine(Leayal.AppInfo.AssemblyInfo.DirectoryPath, "translation", clientregion, translation + ".zip"));
        }

        #region "Remote"
        public void DownloadTranslationAsync()
        {
            this.DownloadTranslationAsync(this.SelectedClientRegion, this.SelectedLanguage);
        }

        public void DownloadTranslationAsync(string region, string language)
        {
            DateTime somewhere = DateTime.Now;
            if (this.LastKnownTranslationVersion != null)
            {
                DateTime? getVersion = this.LastKnownTranslationVersion.GetVersion(region, language);
                if (getVersion.HasValue)
                    somewhere = getVersion.Value;
            }

            this.downloader.DownloadTranslationAsync(region, language, somewhere);
        }

        public event System.ComponentModel.AsyncCompletedEventHandler DownloadTranslationCompleted;
        private void Downloader_DownloadTranslationCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            this.DownloadTranslationCompleted?.Invoke(this, e);
        }

        public event EventHandler<DownloadTranslationProgressChangedEventArgs> DownloadTranslationProgressChanged;
        private void Downloader_DownloadTranslationProgressChanged(object sender, DownloadTranslationProgressChangedEventArgs e)
        {
            this.DownloadTranslationProgressChanged?.Invoke(this, e);
        }
        public event EventHandler<StringEventArgs> DownloadBegin;
        private void Downloader_DownloadBegin(object sender, StringEventArgs e)
        {
            this.DownloadBegin?.Invoke(this, e);
        }

        TranslationVersions lastknownTranslationVersionresult;
        public TranslationVersions LastKnownTranslationVersion => this.lastknownTranslationVersionresult;
        public void CheckForTranslationVersionsAsync()
        {
            this.downloader.CheckForTranslationVersionsAsync();
        }

        public event EventHandler<CheckForTranslationVersionCompletedEventArgs> CheckForTranslationVersionCompleted;
        private void Downloader_CheckForTranslationVersionCompleted(object sender, CheckForTranslationVersionCompletedEventArgs e)
        {
            if (e.Error == null && !e.Cancelled && e.Result != null)
                this.lastknownTranslationVersionresult = e.Result;
            this.CheckForTranslationVersionCompleted?.Invoke(this, e);
        }
        #endregion

        public class LanguageInfo
        {
            public string LanguageName { get; }
            public string LanguageVersion { get; }
            public LanguageInfo(string name, string version)
            {
                this.LanguageName = name;
                this.LanguageVersion = version;
            }
            public LanguageInfo(string name) : this(name, "unknown")
            { }
        }
    }
}
