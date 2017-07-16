using System;

namespace SoulWorker_Translation_Patch_Builder.Classes
{
    class CheckForTranslationVersionCompletedEventArgs : EventArgs
    {
        public Exception Error { get; }
        public bool Cancelled { get; }

        public TranslationVersions Result { get; }

        public CheckForTranslationVersionCompletedEventArgs(Exception ex, bool cancel, TranslationVersions _result) : base()
        {
            this.Error = ex;
            this.Cancelled = cancel;
            this.Result = _result;
        }

        public CheckForTranslationVersionCompletedEventArgs(Exception ex) : this(ex, false, null) { }
        public CheckForTranslationVersionCompletedEventArgs(bool cancel) : this(null, cancel, null) { }
        public CheckForTranslationVersionCompletedEventArgs(TranslationVersions _result) : this(null, false, _result) { }
    }
}
