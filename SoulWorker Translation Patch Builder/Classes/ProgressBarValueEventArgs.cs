using System;

namespace SoulWorker_Translation_Patch_Builder.Classes
{
    class ProgressBarValueEventArgs : EventArgs
    {
        public int CurrentValue { get; }
        public int TotalValue { get; }

        public ProgressBarValueEventArgs(int current, int total) : base()
        {
            this.TotalValue = total;
            this.CurrentValue = current;
        }
    }
}
