using System;

namespace SoulWorker_Translation_Patch_Builder.Classes
{
    class PatchBuildCompletedEventArgs : EventArgs
    {
        public PatchBuildCompletedEventArgs(TranslationPatchBuilder.BuildResult _result) : this(null, false, _result) { }
        public PatchBuildCompletedEventArgs(bool cancel) : this(null, true, null) { }
        public PatchBuildCompletedEventArgs(Exception ex) : this(ex, false, null) { }
        public PatchBuildCompletedEventArgs(Exception ex, bool cancel, TranslationPatchBuilder.BuildResult _result) : base()
        {
            this.Error = ex;
            this.Cancelled = cancel;
            this.Result = _result;
        }

        public Exception Error { get; }
        public TranslationPatchBuilder.BuildResult Result { get; }
        public bool Cancelled { get; }
    }
}
