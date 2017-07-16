using System;
using System.Collections.ObjectModel;
using Microsoft.VisualBasic.ApplicationServices;

namespace SoulWorker_Translation_Patch_Builder
{
    /// <summary>
    /// Sorry for using this
    /// </summary>
    class Program
    {
        [STAThread]
        static void Main()
        {
            ResolveEventHandler anResolveEvent = new ResolveEventHandler(Misc.AssemblyLoader.AssemblyResolve);
            AppDomain.CurrentDomain.AssemblyResolve += anResolveEvent;
            
            var something = new SingleInstanceApplication();
            something.Run(Environment.GetCommandLineArgs());
            AppDomain.CurrentDomain.AssemblyResolve -= anResolveEvent;
        }
    }

    class SingleInstanceApplication : WindowsFormsApplicationBase
    {
        private App myApp;
        public SingleInstanceApplication() : base(AuthenticationMode.Windows)
        {
            this.IsSingleInstance = true;
        }

        protected override bool OnInitialize(ReadOnlyCollection<string> commandLineArgs)
        {
            return base.OnInitialize(commandLineArgs);
        }

        protected override bool OnStartup(StartupEventArgs eventArgs)
        {
            this.myApp = new App();
            // if (this.myApp.Resources == null)
            // this.myApp.Resources = new System.Windows.ResourceDictionary();
            // this.myApp.Resources.MergedDictionaries.Add(new System.Windows.ResourceDictionary() { Source = new Uri("") });
            this.myApp.Run();
            return false;
            // return base.OnStartup(eventArgs);
        }

        protected override void OnStartupNextInstance(StartupNextInstanceEventArgs eventArgs)
        {
            if (this.myApp != null)
                this.myApp.HighlightMainForm();
            // base.OnStartupNextInstance(eventArgs);
        }
    }
}
