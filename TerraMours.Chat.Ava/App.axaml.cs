using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using Splat;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using TerraMours.Chat.Ava.Models;
using TerraMours.Chat.Ava.ViewModels;
using TerraMours.Chat.Ava.Views;

namespace TerraMours.Chat.Ava {
    public partial class App : Application {
        public App() {
            CheckAsync();
        }
        /// <summary>
        /// 启动时的数据检查
        /// </summary>
        public void CheckAsync() {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
                var info = new ProcessStartInfo("chmod", "+x TerraMours.Chat.Ava") {
                    RedirectStandardOutput = true
                };
                using var process = Process.Start(info);
                process.WaitForExit();

                if (process.ExitCode == 0) {
                    Console.WriteLine("执行 chmod 成功");
                }
                else {
                    Console.WriteLine("执行 chmod 命令时发生错误");
                }
            }
        }
        public override void Initialize() {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted() {
            //添加共享资源
            var VMLocator = new VMLocator();
            Resources.Add("VMLocator", VMLocator);

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
                var load = new LoginView {
                    DataContext = new LoginViewModel(),
                };
                desktop.MainWindow = load;
                VMLocator.LoginViewModel.LoginToMainAction = () =>
                {
                    desktop.MainWindow = new MainWindow();
                    desktop.MainWindow.Show();
                    load.Close();
                };

            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}