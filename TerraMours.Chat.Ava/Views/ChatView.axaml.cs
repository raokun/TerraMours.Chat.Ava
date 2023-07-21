using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using DynamicData;
using FluentAvalonia.UI.Controls;
using System;
using System.Linq;
using TerraMours.Chat.Ava.ViewModels;

namespace TerraMours.Chat.Ava.Views;

public partial class ChatView : UserControl
{
    public ChatViewModel ChatViewModel { get; } = new ChatViewModel();
    public ChatView()
    {
        InitializeComponent();
        DataContext = ChatViewModel;
        VMLocator.ChatViewModel = ChatViewModel;
    }

    private async void CopyClick(object? sender, RoutedEventArgs e) {
        if (sender is not MenuItem item) return;
        if (item.Tag is not string text) return;

        // 设置粘贴板
        var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
        var dataObject = new DataObject();
        dataObject.Set(DataFormats.Text, text);
        await clipboard.SetDataObjectAsync(dataObject);
        var dialog = new ContentDialog() { Title = "复制成功", PrimaryButtonText = "OK" };
        await VMLocator.MainViewModel.ContentDialogShowAsync(dialog);
    }

    private void DeleteClick(object? sender, RoutedEventArgs e) {
        if (sender is not MenuItem item) return;
        if (item.Tag is not long id) return;

        VMLocator.ChatViewModel.ChatHistory.Remove(VMLocator.ChatViewModel.ChatHistory.First(m => m.ChatRecordId == id));
        VMLocator.ChatDbcontext.ChatMessages.Remove(VMLocator.ChatDbcontext.ChatMessages.First(m => m.ChatRecordId == id));
    }
}