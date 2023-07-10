using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using TerraMours.Chat.Ava.ViewModels;
using TerraMours.Chat.Ava.Views;

namespace TerraMours.Chat.Ava {
    public partial class App : Application {
        public override void Initialize() {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted() {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
                var load= new LoadView {
                    DataContext = new LoadViewModel(),
                };
                desktop.MainWindow = load;
                

            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}