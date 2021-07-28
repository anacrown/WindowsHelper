using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Xml.Serialization;
using Serilog;
using WPFTaskbarNotifier;

namespace WindowsHelper
{
    public partial class MainWindow : TaskbarNotifier
    {
        private List<window> _windows;
        private ObservableCollection<WindowViewModel> _windowsVm;

        public ObservableCollection<WindowViewModel> Windows
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
        }

        private void MainWindow_OnClosed(object sender, EventArgs e)
        {
            SettingsSave();
        }

        private async void OpenSettingsFile()
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
                formatter.Serialize(fs, _windows ?? Tiler.DefaultData);

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
                if (File.Exists(Properties.Resources.SettingsFile))
                {
                    var fs = new FileStream(Properties.Resources.SettingsFile, FileMode.OpenOrCreate);
                    var formatter = new XmlSerializer(typeof(List<window>));
                    _windows = (List<window>)formatter.Deserialize(fs);
                }
                else
                    _windows = Tiler.DefaultData;

                Windows = new ObservableCollection<WindowViewModel>(_windows.Select(w => w.CreateViewModel()));

                Log.Debug("Файл конфигурации загружен");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Ошибка при загрузке файла конфигурации");
            }
        }
    }
}
