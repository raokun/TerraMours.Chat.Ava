using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
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
}