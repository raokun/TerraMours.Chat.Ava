using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using Splat;
using System;
using TerraMours.Chat.Ava.Models;
using TerraMours.Chat.Ava.ViewModels;
using TerraMours.Chat.Ava.Views;

namespace TerraMours.Chat.Ava {
    public partial class App : Application {
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