using System;

namespace SoulWorker_Translation_Patch_Builder.Classes
{
    class StringEventArgs : EventArgs
    {
        public string Value { get; }
        public StringEventArgs(string val) : base()
        {
            this.Value = val;
        }
    }
}
