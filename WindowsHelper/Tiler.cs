using Serilog;
using Serilog.Context;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace WindowsHelper
{
    public class Tiler
    {
        public static List<window> DefaultData => new List<window>()
        {
            new window()
            {
                enabled = true,
                process = "AoMX",
                mode = windowMode.hold,
                selectedposition = 0,
                position = new List<position>()
                {
                    new position() {X = 0, Y = 0},
                    new position() {X = 960, Y = 0},
                    new position() {X = 1920, Y = 0}
                },
                selectedsize = 0,
                size = new List<size>()
                {
                    new size() {width = 1920, height = 1080}
                }
            },
            new window()
            {
                enabled = true,
                process = "ShooterGame",
                mode = windowMode.hold,
                selectedposition = 0,
                position = new List<position>()
                {
                    new position() {X = 0, Y = 0},
                    new position() {X = 960, Y = 0},
                    new position() {X = 1920, Y = 0}
                },
                selectedsize = 0,
                size = new List<size>()
                {
                    new size() {width = 1920, height = 1080}
                }
            },
            new window()
            {
                enabled = true,
                process = "Zona",
                mode = windowMode.close,
                condition = new List<condition>()
                {
                    new condition()
                    {
                        title = new List<title>()
                        {
                            new title() {isempty = true}
                        }
                    },
                    new condition()
                    {
                        width = new conditionWidth() {value = 162, accuracy = 10},
                        height = new conditionHeight() {value = 372, accuracy = 10}
                    }
                }
            },
            new window()
            {
                enabled = true,
                process = "Zona",
                mode = windowMode.remember,
                condition = new List<condition>()
                {
                    new condition()
                    {
                        title = new List<title>()
                        {
                            new title() {value = "Zona", mode = titleMode.notequals}
                        }
                    },
                    new condition()
                    {
                        title = new List<title>()
                        {
                            new title() {isempty = false}
                        }
                    }
                }
            },
            new window()
            {
                enabled = true,
                process = "dnplayer",
                mode = windowMode.hold,
                size = new List<size>()
                {
                    new size(){width = 974, height = 527}
                },
                position = new List<position>()
                {
                    new position(){X = -7, Y = 0},
                    new position(){X = 953, Y = 0},
                    new position(){X = 1913, Y = 0},
                    new position(){X = 2873, Y = 0},
                    new position(){X = -7, Y = 520},
                    new position(){X = 953, Y = 520},
                    new position(){X = 1913, Y = 520},
                    new position(){X = 2873, Y = 520},
                }
            },
            new window()
            {
                enabled = true,
                process = "Shell_TrayWnd",
                mode = windowMode.notopmost
            }
        };

        // static void Tile(params ArgData[] argData)
        // {
        //     while (true)
        //     {
        //         foreach (var d in argData)
        //         {
        //             var proc = GetProcess(d.processName);
        //             if (proc == null) continue;
        //
        //             d.MainWindowHandle = proc.MainWindowHandle;
        //             d.MainWindowTitle = proc.MainWindowTitle;
        //         }
        //
        //         if (argData.Any(d => d.MainWindowHandle != null))
        //         {
        //             var taskbar = WinApi.FindWindow("Shell_TrayWnd", null);
        //             var taskbarRect = WinApi.GetWindowRect(taskbar);
        //
        //             var taskbarX = taskbarRect.Left;
        //             var taskbarY = taskbarRect.Top;
        //             var taskbarCX = taskbarRect.Right - taskbarRect.Left;
        //             var taskbarCY = taskbarRect.Bottom - taskbarRect.Top;
        //
        //             WinApi.SetWindowPos(taskbar, (IntPtr)WinApi.SpecialWindowHandles.HWND_NOTOPMOST, taskbarX, taskbarY, taskbarCX,
        //                 taskbarCY, WinApi.SetWindowPosFlags.SWP_SHOWWINDOW);
        //         }
        //
        //         foreach (var d in argData.Where(d => d.MainWindowHandle != null))
        //         {
        //             if (d.processName == "Zona" && d.MainWindowTitle == "Zona" || string.IsNullOrEmpty(d.MainWindowTitle))
        //             {
        //                 if (string.IsNullOrEmpty(d.MainWindowTitle) &&
        //                     d.MainWindowHandle.HasValue && WinApi.GetWindowRect(d.MainWindowHandle.Value, out var rect))
        //                 {
        //                     //3668;658 162x372
        //                     if (rect.Right - rect.Left == 162 &&
        //                         rect.Bottom - rect.Top == 372)
        //                     {
        //                         Log.Debug("[{MainWindowTitle}] {ProcessName} {Rect} SendMessage SC_CLOSE",
        //                             d.MainWindowTitle,
        //                             d.processName,
        //                             $"{{{rect.Left};{rect.Top} {rect.Right - rect.Left}x{rect.Bottom - rect.Top}}}");
        //                         WinApi.SendMessage(d.MainWindowHandle.Value, WinApi.WM_SYSCOMMAND, (int)WinApi.SysCommands.SC_CLOSE, IntPtr.Zero);
        //                     }
        //                 }
        //
        //                 continue;
        //             }
        //
        //             // if (!d.MainWindowHandle.HasValue ||
        //             //     !Api.GetWindowRect(d.MainWindowHandle.Value, out var r) ||
        //             //     r.Left == d.x && r.Top == d.y && r.Right - r.Left == d.cx && r.Bottom - r.Top == d.cy) continue;
        //             //
        //             // Log.Debug("[{MainWindowTitle}] {ProcessName} {Rect} SetWindowPos",
        //             //     d.MainWindowTitle,
        //             //     d.processName);
        //
        //             if (!d.MainWindowHandle.HasValue) continue;
        //
        //             WinApi.SetWindowPos(d.MainWindowHandle.Value, (IntPtr)WinApi.SpecialWindowHandles.HWND_TOP, d.x, d.y, d.cx, d.cy,
        //                 WinApi.SetWindowPosFlags.SWP_SHOWWINDOW);
        //         }
        //
        //         if (argData.FirstOrDefault(d => d.processName == "Zona") == null)
        //         {
        //             var zonaProc = GetProcess("Zona");
        //             if (zonaProc != null && !string.IsNullOrEmpty(zonaProc.MainWindowTitle) && zonaProc.MainWindowTitle != "Zona")
        //             {
        //                 var r = WinApi.GetWindowRect(zonaProc.MainWindowHandle);
        //                 argData = argData.Append(new ArgData("Zona", r.Left, r.Top, r.Right - r.Left, r.Bottom - r.Top, zonaProc.MainWindowHandle)).ToArray();
        //             }
        //         }
        //
        //         Thread.Sleep(500);
        //     }
        // }

        public static void Diagnostic()
        {
            while (true)
            {
                foreach (var process in Process.GetProcesses())
                {
                    if (process.ProcessName != "Zona") continue;

                    using (LogContext.PushProperty("ProcessName", process.ProcessName))
                    using (LogContext.PushProperty("MainWindowTitle", process.MainWindowTitle))
                    {
                        var hwnd = process.MainWindowHandle;
                        if (hwnd != IntPtr.Zero)
                        {
                            if (WinApi.GetWindowRect(hwnd, out var rect))
                            {
                                Log.Debug("[{MainWindowTitle}] {ProcessName} {Rect}",
                                    process.MainWindowTitle,
                                    process.ProcessName,
                                    $"{{{rect.Left};{rect.Top} {rect.Right - rect.Left}x{rect.Bottom - rect.Top}}}");
                            }
                            // else
                            //     Log.Information("GetWindowRect return false");
                        }
                        // else
                        //     Log.Information("MainWindowHandle is IntPtr.Zero");
                    }
                }

                Thread.Sleep(500);
                Console.Clear();
            }
        }
    }
}