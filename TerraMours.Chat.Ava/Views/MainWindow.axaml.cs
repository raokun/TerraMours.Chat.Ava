using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using System.Text.Json;
using System.Threading.Tasks;
using System;
using System.IO;
using FluentAvalonia.UI.Controls;
using TerraMours.Chat.Ava.ViewModels;
using System.Globalization;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Markup.Xaml;
using System.Linq;
using Avalonia.Threading;
using Avalonia;
using TerraMours.Chat.Ava.Models;
using ReactiveUI;

namespace TerraMours.Chat.Ava.Views {
    public partial class MainWindow : Window {
        public MainWindowViewModel MainWindowViewModel { get; } = new MainWindowViewModel();
        DatabaseProcess _dbProcess = new DatabaseProcess();
        public MainWindow() {
            InitializeComponent();
            this.Closing += (sender, e) => SaveWindowSizeAndPosition();

            this.Loaded += MainWindow_Loaded;
            MainWindowViewModel = new MainWindowViewModel();
            VMLocator.MainWindowViewModel = MainWindowViewModel;
            DataContext = MainWindowViewModel;
            var cultureInfo = CultureInfo.CurrentCulture;
            if (cultureInfo.Name == "zh-CN") {
                Translate("zh-CN");
            }

            this.KeyDown += MainWindow_KeyDown;

        }
        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }
        #region 键盘监听
        private async void MainWindow_KeyDown(object sender,KeyEventArgs e) {
            if(e.Key==Key.LeftAlt || e.Key == Key.RightAlt) {
                //todo: 添加逻辑
            }
        }
        #endregion
        /// <summary>
        /// 隐藏宽度
        /// </summary>
        private double _previousWidth;

        private async void MainWindow_Loaded(object sender,RoutedEventArgs e) {
            var settings = await LoadAppSettingsAsync();

            if (File.Exists(Path.Combine(settings.AppDataPath, "settings.json"))) {
                this.Width = settings.Width - 1;
                this.Position = new PixelPoint(settings.X, settings.Y);
                this.Height = settings.Height;
                this.Width = settings.Width;
                this.WindowState = settings.IsMaximized ? WindowState.Maximized : WindowState.Normal;
            }
            else {
                var screen = Screens.Primary;
                var workingArea = screen.WorkingArea;

                double dpiScaling = screen.PixelDensity;
                this.Width = 1300 * dpiScaling;
                this.Height = 840 * dpiScaling;

                this.Position = new PixelPoint(5, 0);
            }


            if (!File.Exists(settings.DbPath)) {
                _dbProcess.CreateDatabase();
            }

            await _dbProcess.DbLoadToMemoryAsync();
            await VMLocator.MainViewModel.LoadPhraseItemsAsync();

            VMLocator.MainViewModel.SelectedPhraseItem = settings.PhrasePreset;

            VMLocator.MainViewModel.SelectedLogPain = "Chat List";

            VMLocator.MainViewModel.PhraseExpanderIsOpened = settings.PhraseExpanderMode;


            VMLocator.MainViewModel.SelectedPhraseItem = settings.PhrasePreset;

            VMLocator.MainWindowViewModel.ApiMaxTokens = settings.ApiMaxTokens;
            VMLocator.MainWindowViewModel.ApiTemperature = settings.ApiTemperature;
            VMLocator.MainWindowViewModel.ApiTopP = settings.ApiTopP;
            VMLocator.MainWindowViewModel.ApiN = settings.ApiN;
            VMLocator.MainWindowViewModel.ApiLogprobs = settings.ApiLogprobs;
            VMLocator.MainWindowViewModel.ApiPresencePenalty = settings.ApiPresencePenalty;
            VMLocator.MainWindowViewModel.ApiFrequencyPenalty = settings.ApiFrequencyPenalty;
            VMLocator.MainWindowViewModel.ApiBestOf = settings.ApiBestOf;
            VMLocator.MainWindowViewModel.ApiStop = settings.ApiStop;
            VMLocator.MainWindowViewModel.ApiLogitBias = settings.ApiLogitBias;
            VMLocator.MainWindowViewModel.ApiModel = settings.ApiModel;
            VMLocator.MainWindowViewModel.ApiUrl = settings.ApiUrl;
            VMLocator.MainWindowViewModel.ApiKey = settings.ApiKey;
            VMLocator.MainWindowViewModel.MaxContentLength = settings.MaxContentLength;

            VMLocator.MainWindowViewModel.ApiMaxTokensIsEnable = settings.ApiMaxTokensIsEnable;
            VMLocator.MainWindowViewModel.ApiTemperatureIsEnable = settings.ApiTemperatureIsEnable;
            VMLocator.MainWindowViewModel.ApiTopPIsEnable = settings.ApiTopPIsEnable;
            VMLocator.MainWindowViewModel.ApiNIsEnable = settings.ApiNIsEnable;
            VMLocator.MainWindowViewModel.ApiLogprobIsEnable = settings.ApiLogprobIsEnable;
            VMLocator.MainWindowViewModel.ApiPresencePenaltyIsEnable = settings.ApiPresencePenaltyIsEnable;
            VMLocator.MainWindowViewModel.ApiFrequencyPenaltyIsEnable = settings.ApiFrequencyPenaltyIsEnable;
            VMLocator.MainWindowViewModel.ApiBestOfIsEnable = settings.ApiBestOfIsEnable;
            VMLocator.MainWindowViewModel.ApiStopIsEnable = settings.ApiStopIsEnable;
            VMLocator.MainWindowViewModel.ApiLogitBiasIsEnable = settings.ApiLogitBiasIsEnable;
            VMLocator.MainWindowViewModel.MaxContentLengthIsEnable = settings.MaxContentLengthIsEnable;


            await Dispatcher.UIThread.InvokeAsync(() => { VMLocator.MainViewModel.LogPainIsOpened = false; });
            if (this.Width > 1295) {
                //await Task.Delay(1000);
                await Dispatcher.UIThread.InvokeAsync(() => { VMLocator.MainViewModel.LogPainIsOpened = true; });
            }

            this.GetObservable(ClientSizeProperty).Subscribe(size => OnSizeChanged(size));
            _previousWidth = ClientSize.Width;

            await _dbProcess.UpdateChatLogDatabaseAsync();


            await _dbProcess.CleanUpEditorLogDatabaseAsync();

            if (string.IsNullOrWhiteSpace(VMLocator.MainWindowViewModel.ApiKey)) {
                var dialog = new ContentDialog() { Title = $"Please enter your API key.", PrimaryButtonText = "OK" };
                await VMLocator.MainViewModel.ContentDialogShowAsync(dialog);
                VMLocator.ChatViewModel.OpenApiSettings();
            }
        }

        #region 系统配置
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

        public void SaveWindowSizeAndPosition() {
            var settings = AppSettings.Instance;
            settings.IsMaximized = this.WindowState == WindowState.Maximized;
            this.WindowState = WindowState.Normal;
            settings.Width = this.Width;
            settings.Height = this.Height;
            settings.X = this.Position.X;
            settings.Y = this.Position.Y;

            SaveAppSettings(settings);
        }

        private void SaveAppSettings(AppSettings settings) {
            var jsonString = JsonSerializer.Serialize(settings);
            File.WriteAllText(Path.Combine(settings.AppDataPath, "settings.json"), jsonString);
        }
        #endregion

        #region 国际化
        public void Translate(string targetLanguage) {
            var translations = App.Current.Resources.MergedDictionaries.OfType<ResourceInclude>().FirstOrDefault(x => x.Source?.OriginalString?.Contains("/Lang/") ?? false);

            if (translations != null)
                App.Current.Resources.MergedDictionaries.Remove(translations);

            App.Current.Resources.MergedDictionaries.Add(
                (ResourceDictionary)AvaloniaXamlLoader.Load(
                    new Uri($"avares://TerraMours.Chat.Ava/Assets/lang/{targetLanguage}.axaml")
                    )
                );
        }
        #endregion

        #region 方法
        private void OnSizeChanged(Size newSize) {
            if (_previousWidth != newSize.Width) {
                if (newSize.Width <= 1295) {
                    VMLocator.MainViewModel.LogPainIsOpened = false;
                    VMLocator.MainViewModel.LogPainButtonIsVisible = false;
                }
                else {
                    if (VMLocator.MainViewModel.LogPainButtonIsVisible == false) {
                        VMLocator.MainViewModel.LogPainButtonIsVisible = true;
                    }
                    if (newSize.Width > _previousWidth) {
                        VMLocator.MainViewModel.LogPainIsOpened = true;
                    }
                }
                _previousWidth = newSize.Width;
            }
        }
        #endregion
    }
}