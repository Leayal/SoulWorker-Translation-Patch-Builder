using System;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Animation;

namespace SoulWorker_Translation_Patch_Builder.Windows
{
    /// <summary>
    /// Interaction logic for MyMainMenu.xaml
    /// </summary>
    public partial class MyMainMenu
    {
        private Classes.PatchController _patchcontroller;
        private Leayal.Ini.IniFile config;
        private System.Threading.SynchronizationContext syncContext;
        private Storyboard buildPatchStoryBoard, updateTranslationStoryBoard;

        protected override void OnInitialized(EventArgs e)
        {
            this.config = new Leayal.Ini.IniFile(System.IO.Path.Combine(Leayal.AppInfo.AssemblyInfo.DirectoryPath, "config.ini"));
            this._patchcontroller = this.CreatePatchController();
            base.OnInitialized(e);

            var myIcon = Properties.Resources.haru_sd_WM1UAm_256px;
            this.Icon = Imaging.CreateBitmapSourceFromHIcon(myIcon.Handle, Int32Rect.Empty, System.Windows.Media.Imaging.BitmapSizeOptions.FromWidthAndHeight(myIcon.Width, myIcon.Height));
        }

        public MyMainMenu()
        {
            InitializeComponent();
            
            this.syncContext = System.Threading.SynchronizationContext.Current;
            this.Loaded += this.MyMainMenu_Loaded;

            string predefineGameLocation = this.config.GetValue("Settings", "GameLocation", string.Empty);
            if (string.IsNullOrWhiteSpace(predefineGameLocation))
            {
                if (Environment.Is64BitProcess)
                    predefineGameLocation = Microsoft.Win32.Registry.GetValue(System.IO.Path.Combine(Microsoft.Win32.Registry.LocalMachine.Name, "SOFTWARE", "WOW6432Node", "HanPurple", "J_SW"), "folder", string.Empty) as string;
                else
                    predefineGameLocation = Microsoft.Win32.Registry.GetValue(System.IO.Path.Combine(Microsoft.Win32.Registry.LocalMachine.Name, "SOFTWARE", "HanPurple", "J_SW"), "folder", string.Empty) as string;
                if (!string.IsNullOrWhiteSpace(predefineGameLocation))
                    this.config.SetValue("Settings", "GameLocation", predefineGameLocation);
            }
            this.textBox_gamelocation.Text = predefineGameLocation;

            predefineGameLocation = this.config.GetValue("Settings", "WorkingFolder", string.Empty);
            if (string.IsNullOrWhiteSpace(predefineGameLocation))
                predefineGameLocation = System.IO.Path.Combine(Leayal.AppInfo.AssemblyInfo.DirectoryPath, "WorkingSpace");

            predefineGameLocation = this.config.GetValue("Settings", "OutputFile", string.Empty);
            if (string.IsNullOrWhiteSpace(predefineGameLocation))
                predefineGameLocation = System.IO.Path.Combine(Leayal.AppInfo.AssemblyInfo.DirectoryPath, "Output");
            this.textBox_outputfile.Text = predefineGameLocation;

            this.updateTranslation.Click += this.UpdateTranslation_Click;
            this.buildPatch.Click += this.BuildPatch_Click;
            this.cancelProgress.Click += this.CancelProgress_Click;

            this.browse_gamelocation.Click += this.Browse_gamelocation_Click;
            this.browse_outputfile.Click += this.Browse_outputfile_Click;

            this.buildPatchStoryBoard = this.CreateFadingStoryBoard(this.buildPatch, 1, 0, new TimeSpan(0, 0, 0, 0, 500));
            this.updateTranslationStoryBoard = this.CreateFadingStoryBoard(this.updateTranslation, 1, 0, new TimeSpan(0, 0, 0, 0, 500));
        }

        private void MyMainMenu_Loaded(object sender, RoutedEventArgs e)
        {
            // updateTranslation_fadi
            
            this.building_currentitem.Content = "Connecting to SwHQ Translation Server...";
            this.Working.IsSelected = true;
            this._patchcontroller.CheckForTranslationVersionsAsync();
        }

        private void Browse_outputfile_Click(object sender, RoutedEventArgs e)
        {
            using (Leayal.Forms.FolderBrowseDialogEx fbd = new Leayal.Forms.FolderBrowseDialogEx())
            {
                fbd.OKButtonText = "Select";
                fbd.ShowTextBox = true;
                fbd.ShowNewFolderButton = true;
                fbd.UseNewStyle = true;
                fbd.ValidateTextBox = true;
                fbd.Description = "Select output folder";
                if (!string.IsNullOrWhiteSpace(this.textBox_outputfile.Text) && System.IO.Directory.Exists(this.textBox_outputfile.Text))
                    fbd.SelectedDirectory = this.textBox_outputfile.Text;
                if (fbd.ShowDialog(this) == MessageBoxResult.OK)
                {
                    this.textBox_outputfile.Text = fbd.SelectedDirectory;
                }
            }
        }

        private void Browse_gamelocation_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog();
            ofd.RestoreDirectory = true;
            ofd.CheckFileExists = true;
            ofd.CheckPathExists = true;
            if (!string.IsNullOrWhiteSpace(this.textBox_gamelocation.Text))
                ofd.FileName = Leayal.IO.PathHelper.Combine(this.textBox_gamelocation.Text, "soulworker100.exe");
            ofd.Filter = "SoulWorker Client|soulworker100.exe";
            ofd.Title = "Locate soulworker client game";
            ofd.DefaultExt = "exe";
            if (ofd.ShowDialog(this) == true)
            {
                this.textBox_gamelocation.Text = Microsoft.VisualBasic.FileIO.FileSystem.GetParentPath(ofd.FileName);
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            this.config.Save(System.Text.Encoding.UTF8);
            base.OnClosed(e);
        }

        private void CancelProgress_Click(object sender, RoutedEventArgs e)
        {
            this._patchcontroller.CancelAsync();
        }

        private void UpdateTranslation_Click(object sender, RoutedEventArgs e)
        {
            this.building_currentitem.Content = "Getting file list...";
            this.Working.IsSelected = true;
            this._patchcontroller.DownloadTranslationAsync();
        }

        private void PatchController_DownloadTranslationCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            this.syncContext?.Post(new System.Threading.SendOrPostCallback(delegate 
            {
                if (this._patchcontroller.IsCurrentLanguageFileExists())
                {
                    this.updateTranslation.Content = "Update Translation";
                    this.updateTranslationStoryBoard.Stop();
                    this.buildPatchStoryBoard.Begin();
                }
                else
                {
                    this.updateTranslation.Content = "Download Translation";
                    this.buildPatchStoryBoard.Stop();
                    this.updateTranslationStoryBoard.Begin();
                }
                this.MainMenu.IsSelected = true;
            }), null);
        }

        private void PatchController_DownloadBegin(object sender, Classes.StringEventArgs e)
        {
            this.syncContext?.Post(new System.Threading.SendOrPostCallback(delegate
            {
                // this.Working.IsSelected = true;
                if (this.progressPercent.IsSelected != true)
                    this.progressPercent.IsSelected = true;
                this.building_currentitem.Content = $"Downloading {e.Value}";
            }), null);
        }

        private void PatchController_DownloadTranslationProgressChanged(object sender, Classes.DownloadTranslationProgressChangedEventArgs e)
        {
            this.syncContext?.Post(new System.Threading.SendOrPostCallback(delegate 
            {
                // this.building_currentitem.Content = "Downloading files (" + e.CurrentValue.ToString() + "/" + e.TotalValue.ToString() + ")";
                this.building_progressbar.Maximum = e.TotalValue;
                this.building_progressbar.Value = e.CurrentValue;
            }), null);
        }

        private void PatchController_CheckForTranslationVersionCompleted(object sender, Classes.CheckForTranslationVersionCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                MessageBox.Show(this, e.Error.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close();
            }
            else if (e.Cancelled)
                this.Close();
            else
            {
                if (this.comboBox_ClientRegion.HasItems)
                    this.comboBox_ClientRegion.Items.Clear();
                foreach (string clientRegion in e.Result.GetClientRegions())
                {
                    // Push JP Client to first
                    if (Leayal.StringHelper.IsEqual(clientRegion, "jp", true))
                        this.comboBox_ClientRegion.Items.Insert(0, clientRegion);
                    else
                        this.comboBox_ClientRegion.Items.Add(clientRegion);
                }
                string something = this.config.GetValue("Settings", "SelectedClientRegion", string.Empty);

                if (this.comboBox_ClientRegion.Items.Contains(something))
                    this.comboBox_ClientRegion.SelectedItem = something;
                else
                    this.comboBox_ClientRegion.SelectedIndex = 0;

                this.MainMenu.IsSelected = true;
                this.cancelProgress.Visibility = Visibility.Visible;
                // This as start point

            }
        }

        private void ComboBox_ClientRegion_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                string currentVal = (string)e.AddedItems[0];
                this.config.SetValue("Settings", "SelectedClientRegion", currentVal);
                this._patchcontroller.SelectedClientRegion = currentVal;

                string oldval = this.config.GetValue("Settings", "SelectedLanguage", string.Empty);
                if (this.comboBox_TranslationLanguage.HasItems)
                    this.comboBox_TranslationLanguage.Items.Clear();
                foreach (string regionlangs in this._patchcontroller.LastKnownTranslationVersion.GetRegionLanguages(currentVal))
                {
                    // Put English above everything else
                    if (Leayal.StringHelper.IsEqual(regionlangs, "english", true))
                        this.comboBox_TranslationLanguage.Items.Insert(0, regionlangs);
                    else
                        this.comboBox_TranslationLanguage.Items.Add(regionlangs);
                }

                if (this.comboBox_TranslationLanguage.Items.Contains(oldval))
                    this.comboBox_TranslationLanguage.SelectedItem = oldval;
                else
                    this.comboBox_TranslationLanguage.SelectedIndex = 0;
            }
        }

        private void comboBox_TranslationLanguage_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                string currentselectedLang = (string)e.AddedItems[0];
                this.config.SetValue("Settings", "SelectedLanguage", currentselectedLang);
                this._patchcontroller.SelectedLanguage = currentselectedLang;

                if (this._patchcontroller.IsCurrentLanguageFileExists())
                {
                    this.updateTranslation.Content = "Update Translation";
                    if (this._patchcontroller.IsSelectedLanguageFileOutDated())
                    {
                        this.buildPatchStoryBoard.Stop();
                        this.updateTranslationStoryBoard.Begin();
                    }
                    else
                    {
                        this.updateTranslationStoryBoard.Stop();
                        this.buildPatchStoryBoard.Begin();
                    }
                }
                else
                {
                    this.updateTranslation.Content = "Download Translation";
                    this.buildPatchStoryBoard.Stop();
                    this.updateTranslationStoryBoard.Begin();
                }
            }
        }

        private void BuildPatch_Click(object sender, RoutedEventArgs e)
        {
            string targetlocation = this.textBox_gamelocation.Text, verlocation = Leayal.IO.PathHelper.Combine(targetlocation, "Ver.ini");
            if (!string.IsNullOrWhiteSpace(targetlocation) && System.IO.File.Exists(verlocation))
            {
                Leayal.Ini.IniFile verIni = new Leayal.Ini.IniFile(verlocation);
                if (MessageBox.Show(this, $"Do you want to build Translation Patch for SoulWorker client v{verIni.GetValue("Client", "ver", "Unknown")}?\nIf the game client has any new updates, it is RECOMMENDED for you to build a new patch after the updates.", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    this.config.Save(System.Text.Encoding.UTF8);
                    this._patchcontroller.OutputPatchFilename = this.textBox_outputfile.Text;
                    this._patchcontroller.BuildPatchAsync(targetlocation);
                }
                verIni.Close();
            }
            else
                MessageBox.Show(this, $"Can not find the file 'Ver.ini' in '{targetlocation}'.", "FileNotFoundException", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private Classes.PatchController CreatePatchController()
        {
            Classes.PatchController result = new Classes.PatchController();

            result.BuildStarted += this.PatchController_BuildStarted;
            result.BuildProgressChanged += this.PatchController_BuildProgressChanged;
            result.BuildStepChanged += this.PatchController_BuildStepChanged;
            result.BuildCompleted += this.PatchController_BuildCompleted;

            result.CheckForTranslationVersionCompleted += this.PatchController_CheckForTranslationVersionCompleted;
            result.DownloadTranslationCompleted += this.PatchController_DownloadTranslationCompleted;
            result.DownloadTranslationProgressChanged += this.PatchController_DownloadTranslationProgressChanged;
            result.DownloadBegin += this.PatchController_DownloadBegin;

            return result;
        }

        private void PatchController_BuildStepChanged(object sender, Classes.StringEventArgs e)
        {
            this.syncContext?.Post(new System.Threading.SendOrPostCallback(delegate
            {
                this.building_currentitem.Content = $"Applying translation file {e.Value}";
            }), null);
        }

        private void PatchController_BuildProgressChanged(object sender,Classes.ProgressBarValueEventArgs e)
        {
            this.syncContext?.Post(new System.Threading.SendOrPostCallback(delegate
            {
                // this.building_currentitem.Content = "Downloading files (" + e.CurrentValue.ToString() + "/" + e.TotalValue.ToString() + ")";
                this.building_progressbar.Maximum = e.TotalValue;
                this.building_progressbar.Value = e.CurrentValue;
            }), null);
        }

        private void PatchController_BuildStarted(object sender, EventArgs e)
        {
            // this.progressInfinite.IsSelected = false;
            this.building_currentitem.Content = "Working...";
            this.Working.IsSelected = true;
            this.progressPercent.IsSelected = true;
        }

        private void PatchController_BuildCompleted(object sender, Classes.PatchBuildCompletedEventArgs e)
        {
            this.MainMenu.IsSelected = true;
            if (e.Error != null)
            {
                if (e.Error is Classes.TranslationFileNotFoundException)
                {
                    if (this._patchcontroller.IsCurrentLanguageFileExists())
                    {
                        this.updateTranslation.Content = "Update Translation";
                        this.updateTranslationStoryBoard.Stop();
                        this.buildPatchStoryBoard.Begin();
                    }
                    else
                    {
                        this.updateTranslation.Content = "Download Translation";
                        this.buildPatchStoryBoard.Stop();
                        this.updateTranslationStoryBoard.Begin();
                    }
                    MessageBox.Show(this, "Please Download the translation before building the translation patch.", e.Error.GetType().Namespace, MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                    MessageBox.Show(this, e.Error.ToString(), e.Error.GetType().Namespace, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if (e.Cancelled) { }
            else
            {
                if (MessageBox.Show(this, $"{e.Result.SelectedLanguage} Translation Patch has been built successfully.\nDo you want to open output folder?", "Task finished", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    Leayal.Shell.Explorer.ShowAndHighlightItem(e.Result.OutputPath);
            }
        }

        private void textBox_outputfile_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            this.config.SetValue("Settings", "OutputFile", this.textBox_outputfile.Text);
        }

        private void textBox_gamelocation_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            this.config.SetValue("Settings", "GameLocation", this.textBox_gamelocation.Text);
        }

        private Storyboard CreateFadingStoryBoard(System.Windows.Controls.Control target, double fromValue, double toValue, TimeSpan timespan)
        {
            return CreateFadingStoryBoard(target, fromValue, toValue, timespan, RepeatBehavior.Forever);
        }
        private Storyboard CreateFadingStoryBoard(System.Windows.Controls.Control target, double fromValue, double toValue, TimeSpan timespan, RepeatBehavior behavior)
        {
            // new TimeSpan(0, 0, 0, 0, 500);
            // fromValue = 1
            // toValue = 0

            DoubleAnimation fading = new DoubleAnimation(fromValue, toValue, timespan.Duration());

            Storyboard.SetTarget(fading, target);
            Storyboard.SetTargetProperty(fading, new PropertyPath($"({target.GetType().Name}.Opacity)"));

            var result = new Storyboard()
            {
                AutoReverse = true
            };

            if (behavior != null)
                result.RepeatBehavior = behavior;


            result.Children.Add(fading);
            return result;
        }
    }
    
}
