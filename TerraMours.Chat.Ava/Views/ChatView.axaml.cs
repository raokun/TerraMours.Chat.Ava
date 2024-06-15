using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using DynamicData;
using FluentAvalonia.UI.Controls;
using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Threading;
using ReactiveUI;
using TerraMours.Chat.Ava.ViewModels;

namespace TerraMours.Chat.Ava.Views;

public partial class ChatView : UserControl
{
    private ChatViewModel _chatViewModel { get; } = new ChatViewModel();
    private ScrollViewer? _scrollViewer;

    public ChatView()
    {
        InitializeComponent();
        DataContext = _chatViewModel;
        VMLocator.ChatViewModel = _chatViewModel;

        //注册新增消息往下拉
        _scrollViewer =
            this.FindControl<ScrollViewer>(
                "Scroll"); //this.FindControl<Markdown.Avalonia.MarkdownScrollViewer>("MarkdownScrollViewer");
        _chatViewModel.WhenAnyValue(vm => vm.CurAsk)
            .Subscribe(_ => ScrollToBottom());

    }

    private void ScrollToBottom()
    {
        // 确保滚动容器存在
        if (_scrollViewer != null)
        {
            // 设置滚动位置到容器的最大垂直偏移量
            Task.Factory.StartNew(() =>
            { 
                Task.Delay(2000);//DelayTime 是秒
                Dispatcher.UIThread.InvokeAsync(() =>
                {
                    _scrollViewer.ScrollToEnd();
                }).Wait();

            });
        }
    }

    private async void CopyClick(object? sender, RoutedEventArgs e) {
        if (sender is not MenuItem item) return;
        if (item.Tag is not string text) return;

        
        var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
        var dataObject = new DataObject();
        dataObject.Set(DataFormats.Text, text);
        await clipboard.SetDataObjectAsync(dataObject);
        var dialog = new ContentDialog() { Title = "已成功复制", PrimaryButtonText = "OK" };
        await VMLocator.MainViewModel.ContentDialogShowAsync(dialog);
    }

    private void DeleteClick(object? sender, RoutedEventArgs e) {
        if (sender is not MenuItem item) return;
        if (item.Tag is not long id) return;

        VMLocator.ChatViewModel.ChatHistory.Remove(VMLocator.ChatViewModel.ChatHistory.First(m => m.ChatRecordId == id));
        VMLocator.ChatDbcontext.ChatMessages.Remove(VMLocator.ChatDbcontext.ChatMessages.First(m => m.ChatRecordId == id));
    }
}