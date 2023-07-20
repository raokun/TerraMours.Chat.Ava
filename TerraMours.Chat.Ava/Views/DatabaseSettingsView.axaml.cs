using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using TerraMours.Chat.Ava.ViewModels;

namespace TerraMours.Chat.Ava.Views;

public partial class DatabaseSettingsView : UserControl
{
    public DatabaseSettingsViewModel DatabaseSettingsViewModel { get; } = new DatabaseSettingsViewModel();
    public DatabaseSettingsView()
    {
        InitializeComponent();
        DataContext = DatabaseSettingsViewModel;
        VMLocator.DatabaseSettingsViewModel = DatabaseSettingsViewModel;
    }
}