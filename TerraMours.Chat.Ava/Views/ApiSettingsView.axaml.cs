using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace TerraMours.Chat.Ava.Views;

public partial class ApiSettingsView : UserControl
{
    public ApiSettingsView()
    {
        InitializeComponent();
    }
    private void InitializeComponent() {
        AvaloniaXamlLoader.Load(this);
    }
}