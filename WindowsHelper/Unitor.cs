﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Threading;
using Serilog;

namespace WindowsHelper
{
    public class Unitor : DispatcherObject
    {
        private bool _started;
        private readonly WindowVm _vm;
        private readonly DispatcherTimer _timer;

        public Unitor(WindowVm vm)
        {
            _vm = vm;
            _timer = new DispatcherTimer(TimeSpan.FromMilliseconds(400), DispatcherPriority.Normal, Timer_Tick, Dispatcher);
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (!CheckCondition(_vm.Condition)) return;

            var handle = GetHandle(_vm.Process);
            if (handle == IntPtr.Zero)
                return;

            if (!WinApi.GetWindowRect(handle, out var currentRect))
                return;

            switch (_vm.Mode)
            {
                case windowMode.hold:
                case windowMode.remember:

                    WinApi.RECT requiredRect;

                    if (_vm.Position.Count <= _vm.SelectedPosition ||
                        _vm.Size.Count <= _vm.SelectedSize)
                    {
                        if (_vm.Mode == windowMode.remember)
                        {
                            currentRect.GetPositionAndSize(out var position, out var size);
                            _vm.Position.Add(position);
                            _vm.SelectedPosition = _vm.Position.IndexOf(position);
                            _vm.Size.Add(size);
                            _vm.SelectedSize = _vm.Size.IndexOf(size);
                        }

                        break;
                    }
                    else
                    {
                        requiredRect = new WinApi.RECT().FromPositionAndSize(
                            _vm.Position[_vm.SelectedPosition],
                            _vm.Size[_vm.SelectedSize]);
                    }

                    if (currentRect.Equals(requiredRect)) break;

                    Log.Debug("SetWindowPos {process}", _vm.Process);
                    WinApi.SetWindowPos(handle,
                        (IntPtr)WinApi.SpecialWindowHandles.HWND_TOP,
                        requiredRect.Left,
                        requiredRect.Top,
                        requiredRect.Right - requiredRect.Left,
                        requiredRect.Bottom - requiredRect.Top,
                        WinApi.SetWindowPosFlags.SWP_SHOWWINDOW);

                    break;
                case windowMode.close:

                    Log.Debug("SendMessage CLOSE {process}", _vm.Process);
                    WinApi.SendMessage(handle, WinApi.WM_SYSCOMMAND, (int)WinApi.SysCommands.SC_CLOSE, IntPtr.Zero);

                    break;
                case windowMode.topmost:
                case windowMode.notopmost:

                    var hWndInsertAfter = _vm.Mode == windowMode.topmost
                        ? WinApi.SpecialWindowHandles.HWND_TOPMOST
                        : WinApi.SpecialWindowHandles.HWND_NOTOPMOST;

                    Log.Debug("SetWindowPos {process}", _vm.Process);
                    WinApi.SetWindowPos(handle,
                        (IntPtr)hWndInsertAfter,
                        currentRect.Left,
                        currentRect.Top,
                        currentRect.Right - currentRect.Left,
                        currentRect.Bottom - currentRect.Top,
                        WinApi.SetWindowPosFlags.SWP_SHOWWINDOW);

                    break;
            }
        }

        private IntPtr GetHandle(string process)
        {
            var handle = WinApi.FindWindow(_vm.Process, null);
            if (handle == IntPtr.Zero) handle = Process.GetProcessesByName(process).FirstOrDefault()?.MainWindowHandle ?? IntPtr.Zero;

            return handle;
        }

        public static bool CheckCondition(IEnumerable<condition> conditions)
        {
            return true;
        }

        public void Run()
        {
            var handle = GetHandle(_vm.Process);
            if (handle == IntPtr.Zero)
                return;

            if ((_vm.Position.Count <= _vm.SelectedPosition ||
                 _vm.Size.Count <= _vm.SelectedSize) && _vm.Mode == windowMode.remember)
            {
                if (!WinApi.GetWindowRect(handle, out var currentRect))
                    return;

                currentRect.GetPositionAndSize(out var position, out var size);
                _vm.Position.Add(position);
                _vm.SelectedPosition = _vm.Position.IndexOf(position);
                _vm.Size.Add(size);
                _vm.SelectedSize = _vm.Size.IndexOf(size);
            }

            TimerStart();
        }

        public void Stop()
        {
            var handle = GetHandle(_vm.Process);
            if (handle == IntPtr.Zero)
                return;

            if (_vm.Mode == windowMode.remember)
            {
                _vm.Position.Clear();
                _vm.SelectedPosition = 0;
                _vm.Size.Clear();
                _vm.SelectedSize = 0;
            }

            TimerStop();
        }

        public void Pause()
        {
            if (_vm.Process == "Shell_TrayWnd")
                TimerStop();
        }

        public void Resume()
        {
            if (_vm.Process == "Shell_TrayWnd")
                TimerStart();
        }

        private void TimerStart()
        {
            if (_started) return;
            _started = true;
            _timer.Start();
        }

        private void TimerStop()
        {
            if (!_started) return;
            _started = false;
            _timer.Stop();
        }
    }
}