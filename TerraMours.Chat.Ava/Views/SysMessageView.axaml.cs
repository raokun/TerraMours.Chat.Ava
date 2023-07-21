using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace TerraMours.Chat.Ava.Views;

public partial class SysMessageView : UserControl
{
    public SysMessageView()
    {
        InitializeComponent();
        DataContext = VMLocator.MainViewModel;
    }
    private void InitializeComponent() {
        AvaloniaXamlLoader.Load(this);
    }
}