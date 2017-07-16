using System;

namespace SoulWorker_Translation_Patch_Builder.Classes
{
    class DownloadTranslationProgressChangedEventArgs : EventArgs
    {
        public int CurrentValue { get; }
        public int TotalValue { get; }
        public DownloadTranslationProgressChangedEventArgs(int current, int total) : base()
        {
            this.CurrentValue = current;
            this.TotalValue = total;
        }
    }
}
