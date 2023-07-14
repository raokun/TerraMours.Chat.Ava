using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using FluentAvalonia.UI.Controls;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using TerraMours.Chat.Ava.Models;

namespace TerraMours.Chat.Ava.ViewModels {
    public partial class MainViewModel :ViewModelBase{
        DatabaseProcess _dbProcess = new DatabaseProcess();
        public MainViewModel() {
            PostButtonText = "Post";

            LoadChatListCommand = ReactiveCommand.CreateFromTask<string>(async (keyword) => await LoadChatListAsync(keyword));
            PhrasePresetsItems = new ObservableCollection<string>();

            //会话
            ImportChatLogCommand = ReactiveCommand.CreateFromTask(ImportChatLogAsync);
            ExportChatLogCommand = ReactiveCommand.CreateFromTask(ExportChatLogAsync);
            DeleteChatLogCommand = ReactiveCommand.CreateFromTask(DeleteChatLogAsync);
            //配置
            SystemMessageCommand = ReactiveCommand.Create(InsertSystemMessage);
            HotKeyDisplayCommand = ReactiveCommand.CreateFromTask(HotKeyDisplayAsync);
            OpenApiSettingsCommand = ReactiveCommand.Create(OpenApiSettings);
            ShowDatabaseSettingsCommand = ReactiveCommand.CreateFromTask(ShowDatabaseSettingsAsync);
        }

        public async Task<ContentDialogResult> ContentDialogShowAsync(ContentDialog dialog) {
            //VMLocator.ChatViewModel.ChatViewIsVisible = false;
            //VMLocator.WebChatViewModel.WebChatViewIsVisible = false;
            //VMLocator.WebChatBardViewModel.WebChatBardViewIsVisible = false;
            var dialogResult = await dialog.ShowAsync();
            //VMLocator.ChatViewModel.ChatViewIsVisible = true;
            //VMLocator.WebChatViewModel.WebChatViewIsVisible = true;
            //VMLocator.WebChatBardViewModel.WebChatBardViewIsVisible = true;
            return dialogResult;
        }
        #region 事件
        public ICommand SavePhrasesCommand { get; }
        public ICommand RenamePhrasesCommand { get; }
        public ICommand DeletePhrasesCommand { get; }
        public ICommand ImportPhrasesCommand { get; }
        public ICommand ExportPhrasesCommand { get; }
        public ICommand ClearPhrasesCommand { get; }
        public ICommand ImportChatLogCommand { get; }
        public ICommand ExportChatLogCommand { get; }
        public ICommand DeleteChatLogCommand { get; }
        public ICommand PostCommand { get; }
        public ICommand LoadChatListCommand { get; }

        public ICommand SystemMessageCommand { get; }
        public ICommand OpenApiSettingsCommand { get; }
        public ICommand ShowDatabaseSettingsCommand { get; }
        public ICommand HotKeyDisplayCommand { get; }
        #endregion
        #region 属性
        private string _searchLogKeyword;
        public string SearchLogKeyword {
            get => _searchLogKeyword;
            set {
                this.RaiseAndSetIfChanged(ref _searchLogKeyword, value);
                    LoadChatListCommand.Execute(value);
                    if (string.IsNullOrWhiteSpace(value)) {
                        SearchKeyword = string.Empty;
                        VMLocator.DataGridViewModel.SelectedItemIndex = -1;
                    }
            }
        }
        private string _searchKeyword;
        public string SearchKeyword {
            get => _searchKeyword;
            set => this.RaiseAndSetIfChanged(ref _searchKeyword, value);
        }

        private bool _autoSaveIsChecked;
        public bool AutoSaveIsChecked {
            get => _autoSaveIsChecked;
            set => this.RaiseAndSetIfChanged(ref _autoSaveIsChecked, value);
        }

        private bool _logPainIsOpened;
        public bool LogPainIsOpened {
            get => _logPainIsOpened;
            set => this.RaiseAndSetIfChanged(ref _logPainIsOpened, value);
        }

        private bool _logPainButtonIsVisible;
        public bool LogPainButtonIsVisible {
            get => _logPainButtonIsVisible;
            set => this.RaiseAndSetIfChanged(ref _logPainButtonIsVisible, value);
        }

        private UserControl _selectedLeftView;
        public UserControl SelectedLeftView {
            get => _selectedLeftView;
            set => this.RaiseAndSetIfChanged(ref _selectedLeftView, value);
        }

        private string _selectedLeftPane;
        public string SelectedLeftPane {
            get => _selectedLeftPane;
            set => this.RaiseAndSetIfChanged(ref _selectedLeftPane, value);
        }

        public List<string> LeftPanes { get; } = new List<string>
        {
            "API Chat",
            "Web Chat",
            "Bard"
        };


        private UserControl _selectedRightView;
        public UserControl SelectedRightView {
            get => _selectedRightView;
            set => this.RaiseAndSetIfChanged(ref _selectedRightView, value);
        }

        private string _selectedRightPane;
        public string SelectedRightPane {
            get => _selectedRightPane;
            set => this.RaiseAndSetIfChanged(ref _selectedRightPane, value);
        }

        public List<string> RightPanes { get; } = new List<string>
        {
            "Prompt Editor",
            "Preview"
        };

        public List<string> LogPanes { get; } = new List<string>
        {
            "Chat List"
        };

        private string _selectedLogPain;
        public string SelectedLogPain {
            get => _selectedLogPain;
            set => this.RaiseAndSetIfChanged(ref _selectedLogPain, value);
        }

        private ObservableCollection<string> _phrasePresetsItems;
        public ObservableCollection<string> PhrasePresetsItems {
            get => _phrasePresetsItems;
            set => this.RaiseAndSetIfChanged(ref _phrasePresetsItems, value);
        }

        private string _selectedPhraseItem;
        public string SelectedPhraseItem {
            get => _selectedPhraseItem;
            set => this.RaiseAndSetIfChanged(ref _selectedPhraseItem, value);
        }

        private bool _phraseExpanderIsOpened;
        public bool PhraseExpanderIsOpened {
            get => _phraseExpanderIsOpened;
            set => this.RaiseAndSetIfChanged(ref _phraseExpanderIsOpened, value);
        }

        private bool _isCopyButtonClicked;
        public bool IsCopyButtonClicked {
            get => _isCopyButtonClicked;
            set => this.RaiseAndSetIfChanged(ref _isCopyButtonClicked, value);
        }

        private string _postButtonText;
        public string PostButtonText {
            get => _postButtonText;
            set => this.RaiseAndSetIfChanged(ref _postButtonText, value);
        }

        private string _inputTokens;
        public string InputTokens {
            get => _inputTokens;
            set => this.RaiseAndSetIfChanged(ref _inputTokens, value);
        }

        // CancellationTokenSourceを作成
        private CancellationTokenSource cts = new CancellationTokenSource();
        #endregion

        #region 方法
        public async Task LoadPhraseItemsAsync() {
            var phrases = await _dbProcess.GetPhrasesAsync();
            PhrasePresetsItems.Clear();

            foreach (var phrase in phrases) {
                PhrasePresetsItems.Add(phrase);
            }
        }
        private async Task LoadChatListAsync(string keyword) {
            VMLocator.DataGridViewModel.ChatList = await _dbProcess.SearchChatDatabaseAsync(keyword);
        }

        private async Task ImportChatLogAsync() {
            var dialog = new FilePickerOpenOptions {
                AllowMultiple = false,
                Title = "Select CSV file",
                FileTypeFilter = new List<FilePickerFileType>
                    {new("CSV files (*.csv)") { Patterns = new[] { "*.csv" } },
                    new("All files (*.*)") { Patterns = new[] { "*" } }}
            };

            var result = await (Application.Current!.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)!.MainWindow!.StorageProvider.OpenFilePickerAsync(dialog);

            if (result.Count > 0) {
                var selectedFilePath = result[0].Path.LocalPath;
                try {
                    var msg = await _dbProcess.ImportCsvToTableAsync(selectedFilePath);
                    var cdialog = new ContentDialog() { Title = msg, PrimaryButtonText = "OK" };
                    await ContentDialogShowAsync(cdialog);
                }
                catch (Exception ex) {
                    var cdialog = new ContentDialog() { Title = "Failed to import log." + Environment.NewLine + "Error: " + ex.Message, PrimaryButtonText = "OK" };
                    await ContentDialogShowAsync(cdialog);
                }
                VMLocator.DataGridViewModel.ChatList = await _dbProcess.SearchChatDatabaseAsync();
            }
        }

        private async Task ExportChatLogAsync() {
            var dialog = new FilePickerSaveOptions {
                Title = "Export CSV file",
                FileTypeChoices = new List<FilePickerFileType>
                    {new("CSV files (*.csv)") { Patterns = new[] { "*.csv" } },
                    new("All files (*.*)") { Patterns = new[] { "*" } }}
            };

            var result = await (Application.Current!.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)!.MainWindow!.StorageProvider.SaveFilePickerAsync(dialog);

            if (result != null) {
                var selectedFilePath = result.Path.LocalPath;
                string extension = Path.GetExtension(selectedFilePath);

                if (string.IsNullOrEmpty(extension)) {
                    selectedFilePath += ".csv";
                }

                try {
                    var msg = await _dbProcess.ExportTableToCsvAsync(selectedFilePath);
                    var cdialog = new ContentDialog() { Title = msg, PrimaryButtonText = "OK" };
                    await ContentDialogShowAsync(cdialog);
                }
                catch (Exception ex) {
                    var cdialog = new ContentDialog() { Title = "Failed to export log." + Environment.NewLine + "Error: " + ex.Message, PrimaryButtonText = "OK" };
                    await ContentDialogShowAsync(cdialog);
                }
            }
        }

        private async Task DeleteChatLogAsync() {
            if (VMLocator.DataGridViewModel.SelectedItem == null) {
                return;
            }

            var dialog = new ContentDialog() {
                Title = $"Delete this chat log?{Environment.NewLine}{Environment.NewLine}'{VMLocator.DataGridViewModel.SelectedItem.Title}'",
                PrimaryButtonText = "OK",
                CloseButtonText = "Cancel"
            };

            var contentDialogResult = await ContentDialogShowAsync(dialog);
            if (contentDialogResult != ContentDialogResult.Primary) {
                return;
            }

            try {
                await _dbProcess.DeleteChatLogDatabaseAsync(VMLocator.DataGridViewModel.SelectedItem.Id);
                //await VMLocator.ChatViewModel.InitializeChatAsync();
                VMLocator.DataGridViewModel.ChatList = await _dbProcess.SearchChatDatabaseAsync();
            }
            catch (Exception ex) {
                dialog = new ContentDialog() { Title = "Failed to delete log." + Environment.NewLine + "Error: " + ex.Message, PrimaryButtonText = "OK" };
                await ContentDialogShowAsync(dialog);
            }
        }

        #region 配置
        private void InsertSystemMessage() {
            Application.Current!.TryFindResource("My.Strings.SystemMessage", out object resource1);
           // VMLocator.EditorViewModel.Editor1Text = $"#System{Environment.NewLine}{Environment.NewLine}{resource1}";
        }

        public void OpenApiSettings() {
            VMLocator.ChatViewModel.ChatViewIsVisible = false;
            //VMLocator.WebChatViewModel.WebChatViewIsVisible = false;
            //VMLocator.WebChatBardViewModel.WebChatBardViewIsVisible = false;
            VMLocator.MainWindowViewModel.ApiSettingIsOpened = true;
        }

        private async Task ShowDatabaseSettingsAsync() {
            Application.Current!.TryFindResource("My.Strings.DatabaseInfo", out object resource1);
            var dialog = new ContentDialog() {
                Title = resource1,
                PrimaryButtonText = "OK"
            };

            //dialog.Content = new DatabaseSettingsView();
            await ContentDialogShowAsync(dialog);
        }

        private async Task HotKeyDisplayAsync() {
            var dialog = new ContentDialog() {
                Title = $"Keyboard shortcuts",
                PrimaryButtonText = "OK"
            };

            //dialog.Content = new HotKeyDisplayView();
            await ContentDialogShowAsync(dialog);
        }
        #endregion
        #endregion
    }
}
