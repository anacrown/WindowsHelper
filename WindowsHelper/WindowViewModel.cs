using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Input;
using WindowsHelper.Annotations;

namespace WindowsHelper
{
    public class WindowVm : INotifyPropertyChanged
    {
        public window Window { get; }

        public bool Enabled
        {
            get => Window.enabled;
            set
            {
                Window.enabled = value;
                OnPropertyChanged();
            }
        }

        public int SelectedSize
        {
            get => Window.selectedsize;
            set
            {
                Window.selectedsize = value; 
                OnPropertyChanged();
            }
        }

        public int SelectedPosition
        {
            get => Window.selectedposition;
            set
            {
                Window.selectedposition = value; 
                OnPropertyChanged();
            }
        }

        public WindowVm(window window)
        {
            Window = window;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class Unitor
    {
        private readonly window _window;
        private BackgroundWorker _worker = new BackgroundWorker();

        public Unitor(window window)
        {
            _window = window;
            _worker.DoWork += DoWork;
            _worker.WorkerSupportsCancellation = true;
        }

        private void DoWork(object sender, DoWorkEventArgs e)
        {
            while (!_worker.CancellationPending)
            {
                if (_window.enabled && CheckCondition(_window.condition))
                {
                    var handle = WinApi.FindWindow(_window.process);
                    if (handle != IntPtr.Zero)
                    {
                        var requiredRect = new WinApi.RECT().FromPositionAndSize(
                            _window.position[_window.selectedposition], 
                            _window.size[_window.selectedsize]);

                        switch (_window.mode)
                        {
                            case windowMode.hold:

                                if (WinApi.GetWindowRect(handle, out var currentRect))
                                {

                                }

                                break;
                            case windowMode.close:
                                break;
                            case windowMode.remember:
                                break;
                            case windowMode.topmost:
                                break;
                            case windowMode.notopmost:
                                break;
                        }
                    }

                    Thread.Sleep(400);
                }
            }
        }

        private bool CheckCondition(IEnumerable<condition> conditions)
        {
            return true;
        }

        public void Run()
        {

        }
    }
}