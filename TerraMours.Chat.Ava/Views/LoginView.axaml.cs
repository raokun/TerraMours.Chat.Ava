using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using FluentAvalonia.UI.Controls;
using System.Text.Json;
using System.Threading.Tasks;
using System;
using System.IO;

namespace TerraMours.Chat.Ava.Views;

public partial class LoginView : Window
{
    public LoginView()
    {
        InitializeComponent();
        this.Loaded += LoginView_Loaded;

    }
    private async void LoginView_Loaded(object sender, RoutedEventArgs e) {
        var settings= await LoadAppSettingsAsync();
        VMLocator.LoginViewModel.UserAccount = settings.CurrentUserName;
    }

    private async Task<AppSettings> LoadAppSettingsAsync() {
        var settings = AppSettings.Instance;

        settings = new AppSettings();

        if (File.Exists(Path.Combine(settings.AppDataPath, "settings.json"))) {
            try {
                var options = new JsonSerializerOptions();
                options.Converters.Add(new GridLengthConverter());

                var jsonString = File.ReadAllText(Path.Combine(settings.AppDataPath, "settings.json"));
                settings = JsonSerializer.Deserialize<AppSettings>(jsonString, options);
            }
            catch (Exception) {
                var dialog = new ContentDialog() { Title = $"Invalid setting file. Reset to default values.", PrimaryButtonText = "OK" };
                await VMLocator.MainViewModel.ContentDialogShowAsync(dialog);
                File.Delete(Path.Combine(settings.AppDataPath, "settings.json"));
            }
        }

        return settings;
    }
}