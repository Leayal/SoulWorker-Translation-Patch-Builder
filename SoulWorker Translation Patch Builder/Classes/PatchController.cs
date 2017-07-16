using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace SoulWorker_Translation_Patch_Builder.Classes
{
    class PatchController
    {
        private TranslationPatchBuilder patchbuilder;
        private TranslationResource translationresource;
        public PatchController()
        {
            this.patchbuilder = this.CreateTranslationPatchBuilder();
            this.translationresource = this.CreateTranslationResource();
        }

        #region "General"
        public void CancelAsync()
        {
            if (this.patchbuilder.IsBusy)
                this.patchbuilder.CancelAsync();
            if (this.translationresource.IsBusy)
                this.translationresource.CancelAsync();
        }

        public bool IsCurrentLanguageFileExists()
        {
            return this.translationresource.IsCurrentLanguageFileExists();
        }

        public bool IsSelectedLanguageFileOutDated()
        {
            if (this.LastKnownTranslationVersion == null)
                return false;
            var laihwglaiwhg = this.translationresource.GetCurrentLanguageFile();
            if (laihwglaiwhg.LastWriteTime >= this.LastKnownTranslationVersion.GetVersion(this.translationresource.SelectedClientRegion, this.translationresource.SelectedLanguage))
                return false;
            return true;
        }
        #endregion

        #region "TranslationResource"
        public string SelectedClientRegion { get { return this.translationresource.SelectedClientRegion; } set { this.translationresource.SelectedClientRegion = value; } }
        public string SelectedLanguage { get { return this.translationresource.SelectedLanguage; } set { this.translationresource.SelectedLanguage = value; } }
        // public string WorkingDirectory { get { return this.translationresource.WorkingDirectory; } set { this.translationresource.WorkingDirectory = value; } }

        public void DownloadTranslationAsync()
        {
            this.translationresource.DownloadTranslationAsync();
        }

        public event AsyncCompletedEventHandler DownloadTranslationCompleted;
        private void TranslationResource_DownloadTranslationCompleted(object sender, AsyncCompletedEventArgs e)
        {
            this.DownloadTranslationCompleted?.Invoke(this, e);
        }



        public TranslationVersions LastKnownTranslationVersion => this.translationresource.LastKnownTranslationVersion;
        private TranslationResource CreateTranslationResource()
        {
            TranslationResource TranslationResource = new TranslationResource();
            TranslationResource.CheckForTranslationVersionCompleted += this.TranslationResource_CheckForTranslationVersionCompleted;
            TranslationResource.DownloadTranslationProgressChanged += this.TranslationResource_DownloadTranslationProgressChanged;
            TranslationResource.DownloadTranslationCompleted += this.TranslationResource_DownloadTranslationCompleted;
            TranslationResource.DownloadBegin += this.TranslationResource_DownloadBegin;
            return TranslationResource;
        }

        public event EventHandler<StringEventArgs> DownloadBegin;
        private void TranslationResource_DownloadBegin(object sender, StringEventArgs e)
        {
            this.DownloadBegin?.Invoke(this, e);
        }

        public event EventHandler<DownloadTranslationProgressChangedEventArgs> DownloadTranslationProgressChanged;
        private void TranslationResource_DownloadTranslationProgressChanged(object sender, DownloadTranslationProgressChangedEventArgs e)
        {
            this.DownloadTranslationProgressChanged?.Invoke(this, e);
        }

        public void CheckForTranslationVersionsAsync()
        {
            this.translationresource.CheckForTranslationVersionsAsync();
        }

        public event EventHandler<CheckForTranslationVersionCompletedEventArgs> CheckForTranslationVersionCompleted;
        private void TranslationResource_CheckForTranslationVersionCompleted(object sender, CheckForTranslationVersionCompletedEventArgs e)
        {
            this.CheckForTranslationVersionCompleted?.Invoke(this, e);
        }
        #endregion

        #region "TranslationPatchBuilder"
        public string OutputPatchFilename { get { return this.patchbuilder.OutputPath; } set { this.patchbuilder.OutputPath = value; } }
        private TranslationPatchBuilder CreateTranslationPatchBuilder()
        {
            TranslationPatchBuilder result = new TranslationPatchBuilder();
            result.BuildStarted += this.TranslationPatchBuilder_BuildStarted;
            result.BuildProgressChanged += this.TranslationPatchBuilder_BuildProgressChanged;
            result.BuildCompleted += this.TranslationPatchBuilder_BuildCompleted;
            result.BuildStepChanged += this.TranslationPatchBuilder_BuildStepChanged;
            return result;
        }

        private void TranslationPatchBuilder_BuildStepChanged(object sender, StringEventArgs e)
        {
            this.OnBuildStepChanged(e);
        }

        public void BuildPatchAsync(string gameDirectory)
        {
            if (!this.patchbuilder.IsBusy)
                this.patchbuilder.BuildAsync(this.translationresource, gameDirectory);
        }

        private void TranslationPatchBuilder_BuildStarted(object sender, EventArgs e)
        {
            this.OnBuildStarted(e);
        }

        private void TranslationPatchBuilder_BuildCompleted(object sender, PatchBuildCompletedEventArgs e)
        {
            this.OnBuildCompleted(e);
        }
        
        private void TranslationPatchBuilder_BuildProgressChanged(object sender, ProgressBarValueEventArgs e)
        {
            this.OnBuildProgressChanged(e);
        }

        public event EventHandler<StringEventArgs> BuildStepChanged;
        protected virtual void OnBuildStepChanged(StringEventArgs e)
        {
            this.BuildStepChanged?.Invoke(this, e);
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

        public event EventHandler<PatchBuildCompletedEventArgs> BuildCompleted;
        protected virtual void OnBuildCompleted(PatchBuildCompletedEventArgs e)
        {
            this.BuildCompleted?.Invoke(this, e);
        }
        #endregion
    }
}
