using System;
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
            if (!GetHandle(_vm.Process, out var handle, out var title))
                return;
                
            if (!WinApi.GetWindowRect(handle, out var currentRect))
                return;

            if (!CheckConditions(handle, title, currentRect, _vm.Condition)) return;

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

        private bool GetHandle(string processName, out IntPtr handle, out string title)
        {
            title = null;
            handle = WinApi.FindWindow(_vm.Process, null);
            if (handle == IntPtr.Zero)
            {
                var process = Process.GetProcessesByName(processName).FirstOrDefault();
                handle = process?.MainWindowHandle ?? IntPtr.Zero;
                title = process?.MainWindowTitle;

                return handle != IntPtr.Zero;
            }

            return handle != IntPtr.Zero;
        }

        public static bool CheckConditions(IntPtr handle, string windowTitle, WinApi.RECT rect, List<condition> conditions)
        {
            if (!conditions.Any())
                return true;

            var conditionResults = conditions.Select(condition => CheckCondition(handle, windowTitle, rect, condition)).ToArray();

            var result = conditionResults.Aggregate(conditionResults.First(), (current, t) => current & t);

            return result;
        }

        public static bool CheckCondition(IntPtr handle, string windowTitle, WinApi.RECT rect, condition condition)
        {
            var titleResults = condition.title.Select(title => CheckConditionTitle(windowTitle, title)).ToArray();

            var result = titleResults.Any() && titleResults.Aggregate(titleResults.First(), (current, t) => current | t);

            var widthResult = CheckConditionWidth(rect, condition.width);
            if (widthResult != null)
                result |= widthResult.Value;

            var heightResult = CheckConditionHeight(rect, condition.height);
            if (heightResult != null)
                result |= heightResult.Value;

            return result;
        }

        public static bool CheckConditionTitle(string windowTitle, title title)
        {
            return !string.IsNullOrEmpty(title.value)
                ? title.mode == titleMode.@equals 
                    ? title.value == windowTitle 
                    : title.value != windowTitle
                : title.isempty
                    ? string.IsNullOrEmpty(windowTitle)
                    : !string.IsNullOrEmpty(windowTitle);
        }

        public static bool? CheckConditionWidth(WinApi.RECT rect, conditionWidth width)
        {
            return width != null && !width.IsEmpty()
                ? (bool?) (Math.Abs(rect.Right - rect.Left - width.value) < width.accuracy)
                : null;
        }

        public static bool? CheckConditionHeight(WinApi.RECT rect, conditionHeight height)
        {
            return height != null && !height.IsEmpty()
                ? (bool?) (Math.Abs(rect.Bottom - rect.Top - height.value) < height.accuracy)
                : null;
        }

        public void Run()
        {
            if (!GetHandle(_vm.Process, out var handle, out var title))
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
            if (!GetHandle(_vm.Process, out var handle, out var title))
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

    public static class ConditionExtension
    {
        public static bool IsEmpty(this conditionWidth width) => width.value == 0 && width.accuracy == 0;

        public static bool IsEmpty(this conditionHeight height) => height.value == 0 && height.accuracy == 0;
    }
}