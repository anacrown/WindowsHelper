using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
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
            SettingsLoad();

            Notify();
        }

        private void MainWindow_OnClosed(object sender, EventArgs e)
        {
            SettingsSave();
        }

        private void OpenSettingsFile()
        {
            try
            {
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
                var fs = new FileStream(Properties.Resources.SettingsFile, FileMode.OpenOrCreate);
                var formatter = new XmlSerializer(typeof(List<window>));
                formatter.Serialize(fs, Windows.Select(w => w.Window).ToList());

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
                if (File.Exists(Properties.Resources.SettingsFile))
                {
                    var fs = new FileStream(Properties.Resources.SettingsFile, FileMode.OpenOrCreate);
                    var formatter = new XmlSerializer(typeof(List<window>));
                    windows = (List<window>)formatter.Deserialize(fs);
                }
                else
                    windows = Tiler.DefaultData;

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
