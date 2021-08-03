using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Threading;

namespace WindowsHelper
{
    public class Unitor : DispatcherObject
    {
        private readonly window _window;
        private readonly DispatcherTimer timer;

        public Unitor(window window)
        {
            _window = window;
            timer = new DispatcherTimer(TimeSpan.FromMilliseconds(400), DispatcherPriority.Normal, Timer_Tick, Dispatcher);
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (!CheckCondition(_window.condition)) return;

            var handle = WinApi.FindWindow(_window.process, null);
            if (handle == IntPtr.Zero)
            {
                handle = GetProcessHandle(_window.process);

                if (handle == IntPtr.Zero)
                    return;
            }

            switch (_window.mode)
            {
                case windowMode.hold:

                    if (WinApi.GetWindowRect(handle, out var currentRect))
                    {
                        var requiredRect = new WinApi.RECT().FromPositionAndSize(
                            _window.position[_window.selectedposition],
                            _window.size[_window.selectedsize]);

                        if (!currentRect.Equals(requiredRect))
                        {
                            WinApi.SetWindowPos(handle,
                                (IntPtr)WinApi.SpecialWindowHandles.HWND_TOP,
                                requiredRect.Left,
                                requiredRect.Top,
                                requiredRect.Right - requiredRect.Left,
                                requiredRect.Bottom - requiredRect.Top,
                                WinApi.SetWindowPosFlags.SWP_SHOWWINDOW);
                        }
                    }

                    break;
                case windowMode.close:
                    break;
                case windowMode.remember:
                    break;
                case windowMode.topmost:
                    break;
                case windowMode.notopmost:

                    if (WinApi.GetWindowRect(handle, out var taskbarRect))
                    {
                        var taskbarX = taskbarRect.Left;
                        var taskbarY = taskbarRect.Top;
                        var taskbarCX = taskbarRect.Right - taskbarRect.Left;
                        var taskbarCY = taskbarRect.Bottom - taskbarRect.Top;

                        WinApi.SetWindowPos(handle, (IntPtr)WinApi.SpecialWindowHandles.HWND_NOTOPMOST, taskbarX, taskbarY, taskbarCX,
                            taskbarCY, WinApi.SetWindowPosFlags.SWP_SHOWWINDOW);
                    }

                    break;
            }
        }

        private IntPtr GetProcessHandle(string processName) => Process.GetProcessesByName(processName).FirstOrDefault()?.MainWindowHandle ?? IntPtr.Zero;

        private bool CheckCondition(IEnumerable<condition> conditions)
        {
            return true;
        }

        public void Run() => timer.Start();

        public void Stop() => timer.Stop();
    }
}