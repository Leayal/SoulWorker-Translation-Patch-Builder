using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;

namespace SoulWorker_Translation_Patch_Builder
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App() :base()
        {
            this.InitializeComponent();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            // e.Args.Contains("", StringComparer.OrdinalIgnoreCase);

            base.OnStartup(e);
        }

        public void HighlightMainForm()
        {
            if (this.MainWindow != null)
            {
                this.MainWindow.Activate();
                /*if (!this.MainWindow.Topmost)
                {
                    this.MainWindow.Topmost = true;
                    this.MainWindow.Topmost = false;
                }
                else
                {
                    this.MainWindow.Activate();
                }//*/
            }
        }
    }
}
