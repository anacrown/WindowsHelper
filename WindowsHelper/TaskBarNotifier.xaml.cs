using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Windows;
using System.Xml;
using System.Xml.Serialization;
using Serilog;
using WPFTaskbarNotifier;

namespace WindowsHelper
{
    public partial class MainWindow : TaskbarNotifier
    {
        private ObservableCollection<WindowVm> _windowsVm;

        public ObservableCollection<WindowVm> Windows
        {
            get => _windowsVm;
            set
            {
                _windowsVm = value;
                OnPropertyChanged(nameof(Windows));
            }
        }

        public MainWindow() => InitializeComponent();

        private void OpenMenuItem_OnClick(object sender, RoutedEventArgs e) => Notify();

        private void ExitMenuItem_OnClick(object sender, RoutedEventArgs e) => Close();

        private void ConfigureMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            OpenSettingsFile();

            SettingsLoad();
        }

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (!IsRunAsAdministrator())
            {
                var processInfo = new ProcessStartInfo(Assembly.GetExecutingAssembly().CodeBase);

                processInfo.UseShellExecute = true;
                processInfo.Verb = "runas";

                try
                {
                    Process.Start(processInfo);
                }
                catch (Exception)
                {
                    MessageBox.Show("Sorry, this application must be run as Administrator.");
                }

                Application.Current.Shutdown();
            }

            SettingsLoad();

            Notify();
        }

        private bool IsRunAsAdministrator()
        {
            var wi = WindowsIdentity.GetCurrent();
            var wp = new WindowsPrincipal(wi);

            return wp.IsInRole(WindowsBuiltInRole.Administrator);
        }

        private void MainWindow_OnClosed(object sender, EventArgs e)
        {
            SettingsSave();
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.Property == VisibilityProperty && _windowsVm != null)
            {
                Log.Debug("OnPropertyChanged {property} value {NewValue}", e.Property.Name, e.NewValue);

                switch ((Visibility)e.NewValue)
                {
                    case Visibility.Visible:

                        foreach (var vm in _windowsVm) vm.Pause();

                        break;
                    case Visibility.Hidden:

                        foreach (var vm in _windowsVm) vm.Resume();

                        break;
                    case Visibility.Collapsed:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            base.OnPropertyChanged(e);
        }

        private void OpenSettingsFile()
        {
            try
            {
                SettingsSave();
                
                var fs = new FileStream(Properties.Resources.SettingsFile, FileMode.Create);
                var formatter = new XmlSerializer(typeof(List<window>));
                formatter.Serialize(fs, Windows.Select(vm => vm.Window).ToList());
                fs.Close();

                var proc = new Process
                {
                    StartInfo =
                    {
                        FileName = Properties.Resources.SettingsFile,
                        UseShellExecute = true
                    }
                };

                proc.Start();
                proc.WaitForExit();

                fs = new FileStream(Properties.Resources.SettingsFile, FileMode.Open);
                var windows = (List<window>)formatter.Deserialize(fs);

                if (Windows != null && Windows.Any())
                    foreach (var windowVm in Windows)
                    {
                        windowVm.Stop();
                        windowVm.Dispose();
                    }

                Windows = new ObservableCollection<WindowVm>(windows.Select(w => new WindowVm(w)));

                SettingsSave();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Ошибка при открытии файла конфигурации");
            }
        }

        private void SettingsSave()
        {
            try
            {
                var ms = new MemoryStream();
                var formatter = new XmlSerializer(typeof(List<window>));
                formatter.Serialize(ms, Windows.Select(w => w.Window).ToList());
                ms.Position = 0;

                if (Properties.Settings.Default.UserSettings == null)
                    Properties.Settings.Default.UserSettings = new XmlDocument();

                Properties.Settings.Default.UserSettings.Load(ms);
                Properties.Settings.Default.Save();

                Log.Debug("Файл конфигурации успешно сохранен");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Ошибка при сохранении файла конфигурации");
            }
        }

        private void SettingsLoad()
        {
            try
            {
                List<window> windows;
                if (Properties.Settings.Default.UserSettings != null)
                {
                    var ms = new MemoryStream();
                    Properties.Settings.Default.UserSettings.Save(ms);
                    ms.Position = 0;
                    var formatter = new XmlSerializer(typeof(List<window>));
                    windows = (List<window>)formatter.Deserialize(ms);
                }
                else
                    windows = Tiler.DefaultData;

                if (Windows != null && Windows.Any())
                    foreach (var windowVm in Windows)
                    {
                        windowVm.Stop();
                        windowVm.Dispose();
                    }

                Windows = new ObservableCollection<WindowVm>(windows.Select(w => new WindowVm(w)));

                Log.Debug("Файл конфигурации загружен");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Ошибка при загрузке файла конфигурации");
            }
        }
    }
}
