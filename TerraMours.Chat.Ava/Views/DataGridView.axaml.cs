using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using TerraMours.Chat.Ava.ViewModels;

namespace TerraMours.Chat.Ava.Views;

public partial class DataGridView : UserControl
{
    public DataGridViewModel DataGridViewModel { get; } = new DataGridViewModel();
    public DataGridView()
    {
        InitializeComponent();
        DataContext = DataGridViewModel;
        VMLocator.DataGridViewModel = DataGridViewModel;
        var dataGrid = this.Get<DataGrid>("ChatListDataGrid");
        dataGrid.GotFocus += DataGrid_GotFocus;
        dataGrid.LostFocus += DataGrid_LostFocus;
    }

    private void DataGrid_GotFocus(object sender,GotFocusEventArgs e) {
        VMLocator.DataGridViewModel.DataGridIsFocused = true;
    }
    private void DataGrid_LostFocus(object sender, RoutedEventArgs e) {
        VMLocator.DataGridViewModel.DataGridIsFocused = false;
    }

    private void InitializeComponent() {
        AvaloniaXamlLoader.Load(this);
    }
}