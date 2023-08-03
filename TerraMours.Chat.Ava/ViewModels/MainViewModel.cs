using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using AvaloniaEdit.Utils;
using FluentAvalonia.UI.Controls;
using OpenAI.Managers;
using OpenAI.ObjectModels.RequestModels;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using TerraMours.Chat.Ava.Models;
using TerraMours.Chat.Ava.Models.Class;
using TerraMours.Chat.Ava.Views;
using Tmds.DBus.Protocol;

namespace TerraMours.Chat.Ava.ViewModels {
    public partial class MainViewModel :ViewModelBase{
        ChatProcess _chatProcess = new ChatProcess();
        public MainViewModel() {
            PostButtonText = "Post";

            LoadChatListCommand = ReactiveCommand.CreateFromTask<string>(async (keyword) => await LoadChatListAsync(keyword));
            PhrasePresetsItems = new ObservableCollection<string>();

            //会话
            ImportChatLogCommand = ReactiveCommand.CreateFromTask(ImportChatLogAsync);
            ExportChatLogCommand = ReactiveCommand.CreateFromTask(ExportChatLogAsync);
            DeleteChatLogCommand = ReactiveCommand.CreateFromTask(DeleteChatLogAsync);
            GptChatConversationListCommand= ReactiveCommand.CreateFromTask(GptChatConversationList);
            //配置
            SystemMessageCommand = ReactiveCommand.CreateFromTask(InsertSystemMessageAsync);
            HotKeyDisplayCommand = ReactiveCommand.CreateFromTask(HotKeyDisplayAsync);
            OpenApiSettingsCommand = ReactiveCommand.Create(OpenApiSettings);
            ShowDatabaseSettingsCommand = ReactiveCommand.CreateFromTask(ShowDatabaseSettingsAsync);
            //聊天
            PostCommand = ReactiveCommand.CreateFromTask(PostGptChatAsync);
        }

        public async Task<ContentDialogResult> ContentDialogShowAsync(ContentDialog dialog) {
            var dialogResult = await dialog.ShowAsync();
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

        public ICommand GptChatConversationListCommand { get; }
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
            "AI Chat"
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


        public List<string> LogPanes { get; } = new List<string>
        {
            "会话列表"
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

        private string _postMessage;
        public string PostMessage
        {
            get => _postMessage;
            set => this.RaiseAndSetIfChanged(ref _postMessage, value);
        }

        private string _systemMessage;
        public string SystemMessage {
            get => _systemMessage;
            set => this.RaiseAndSetIfChanged(ref _systemMessage, value);
        }

        #endregion

        #region 方法
        private async Task LoadChatListAsync(string keyword) {
            VMLocator.DataGridViewModel.ChatList = VMLocator.ChatDbcontext.ChatLists.ToObservableCollection();
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
                    var msg = await _chatProcess.ImportCsvToTableAsync(selectedFilePath);
                    var cdialog = new ContentDialog() { Title = msg, PrimaryButtonText = "OK" };
                    await ContentDialogShowAsync(cdialog);
                }
                catch (Exception ex) {
                    var cdialog = new ContentDialog() { Title = "Failed to import log." + Environment.NewLine + "Error: " + ex.Message, PrimaryButtonText = "OK" };
                    await ContentDialogShowAsync(cdialog);
                }
                VMLocator.DataGridViewModel.ChatList = VMLocator.ChatDbcontext.ChatLists.ToObservableCollection();
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
                    var msg = await _chatProcess.ExportTableToCsvAsync(selectedFilePath);
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
                var deleRecord = VMLocator.ChatDbcontext.ChatMessages.Where(m => m.ConversationId == VMLocator.DataGridViewModel.SelectedItem.Id);
                VMLocator.ChatDbcontext.ChatMessages.RemoveRange(deleRecord);
                var delListItem= VMLocator.ChatDbcontext.ChatLists.FirstOrDefault(m => m.Id == VMLocator.DataGridViewModel.SelectedItem.Id);
                VMLocator.ChatDbcontext.ChatLists.Remove(delListItem);
                VMLocator.ChatDbcontext.SaveChanges();
                VMLocator.ChatViewModel.ChatHistory.Clear();
                VMLocator.DataGridViewModel.ChatList.Remove(VMLocator.DataGridViewModel.SelectedItem);
                //VMLocator.DataGridViewModel.ChatList = VMLocator.ChatDbcontext.ChatLists.ToObservableCollection();
            }
            catch (Exception ex) {
                dialog = new ContentDialog() { Title = "Failed to delete log." + Environment.NewLine + "Error: " + ex.Message, PrimaryButtonText = "OK" };
                await ContentDialogShowAsync(dialog);
            }
        }

        #region 配置
        private async Task InsertSystemMessageAsync() {
            var systemMessage = SystemMessage;
            if (string.IsNullOrEmpty(systemMessage)) {
                Application.Current!.TryFindResource("My.Strings.SystemMessage", out object resource);
                SystemMessage = (string)resource;
            }

            Application.Current!.TryFindResource("My.Strings.SystemMessageTitle", out object resource1);

            var dialog = new ContentDialog() {
                Title = resource1,
                PrimaryButtonText = "OK"
            };

            dialog.Content = new SysMessageView();
            await ContentDialogShowAsync(dialog);
        }

        public void OpenApiSettings() {
            VMLocator.ChatViewModel.ChatViewIsVisible = false;
            VMLocator.MainWindowViewModel.ApiSettingIsOpened = true;
        }

        private async Task ShowDatabaseSettingsAsync() {
            Application.Current!.TryFindResource("My.Strings.DatabaseInfo", out object resource1);
            var dialog = new ContentDialog() {
                Title = resource1,
                PrimaryButtonText = "OK"
            };

            dialog.Content = new DatabaseSettingsView();
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

        #region 服务端接口
        /// <summary>
        /// OpenAI 调用方法
        /// </summary>
        /// <returns></returns>
        private async Task PostChatAsync()
        {
            try
            {
                string message = PostMessage;
                int conversationId = 1;
                //创建会话
                if(VMLocator.DataGridViewModel.ChatList == null)
                {
                    var newChat = new ChatList() {Title = (message.Length < 5 ? message : $"{message.Substring(0, 5)}..."), Category = (message.Length < 5 ? message : $"{message.Substring(0, 5)}..."), Date = DateTime.Now };
                    VMLocator.DataGridViewModel.ChatList = new ObservableCollection<ChatList> {
                        newChat
                    };
                    await VMLocator.ChatDbcontext.ChatLists.AddAsync(newChat);
                    await VMLocator.ChatDbcontext.SaveChangesAsync();
                }
                if (VMLocator.ChatViewModel.ChatHistory == null)
                    VMLocator.ChatViewModel.ChatHistory = new ObservableCollection<Models.ChatMessage>();
                var userMessge = new Models.ChatMessage() { ConversationId = conversationId, Message = message, Role = "User", CreateDate = DateTime.Now };
                VMLocator.ChatViewModel.ChatHistory.Add(userMessge);
                await VMLocator.ChatDbcontext.ChatMessages.AddAsync(userMessge);
                await VMLocator.ChatDbcontext.SaveChangesAsync();
                //根据配置中的CONTEXT_COUNT 查询上下文
                var messages = new List<OpenAI.ObjectModels.RequestModels.ChatMessage>();
                messages.Add(OpenAI.ObjectModels.RequestModels.ChatMessage.FromUser(message));
                var openAiOpetions = new OpenAI.OpenAiOptions()
                {
                    ApiKey = AppSettings.Instance.ApiKey,
                    BaseDomain = AppSettings.Instance.ApiUrl
                };
                var openAiService = new OpenAIService(openAiOpetions);
                //调用SDK
                var response = await openAiService.ChatCompletion.CreateCompletion(new ChatCompletionCreateRequest
                {
                    Messages = messages,
                    Model = AppSettings.Instance.ApiModel,
                    MaxTokens = AppSettings.Instance.ApiMaxTokensIsEnable ? AppSettings.Instance.ApiMaxTokens:null,
                    TopP = AppSettings.Instance.ApiTopPIsEnable ? (float?)AppSettings.Instance.ApiTopP:null,
                    N = AppSettings.Instance.ApiNIsEnable ? AppSettings.Instance.ApiN:null,
                    PresencePenalty = AppSettings.Instance.ApiPresencePenaltyIsEnable ? (float?)AppSettings.Instance.ApiPresencePenalty:null,
                    FrequencyPenalty= AppSettings.Instance.ApiFrequencyPenaltyIsEnable ? (float?)AppSettings.Instance.ApiFrequencyPenalty:null,
                    Stop = AppSettings.Instance.ApiStopIsEnable? AppSettings.Instance.ApiStop:null,
                    Temperature=AppSettings.Instance.ApiTemperatureIsEnable?(float?) AppSettings.Instance.ApiTemperature:null,
                    LogitBias= AppSettings.Instance.ApiLogitBiasIsEnable? AppSettings.Instance.ApiLogitBias:null,
                });
                if (response == null)
                {
                    var dialog = new ContentDialog()
                    {
                        Title = "接口调用失败",
                        PrimaryButtonText = "Ok"
                    };
                    await VMLocator.MainViewModel.ContentDialogShowAsync(dialog);
                }
                if (!response.Successful)
                {
                    var dialog = new ContentDialog()
                    {
                        Title = $"接口调用失败，报错内容: {response.Error.Message}",
                        PrimaryButtonText = "Ok",
                    };
                    await VMLocator.MainViewModel.ContentDialogShowAsync(dialog);
                }
                var assistant = new Models.ChatMessage() {  ConversationId = conversationId, Message = response.Choices.FirstOrDefault().Message.Content, Role = "Assistant", CreateDate = DateTime.Now };
                VMLocator.ChatViewModel.ChatHistory.Add(assistant);
                await VMLocator.ChatDbcontext.ChatMessages.AddAsync(assistant);
                await VMLocator.ChatDbcontext.SaveChangesAsync();
                VMLocator.MainViewModel.PostMessage = "";
            }
            catch (Exception e)
            {
                var dialog = new ContentDialog() {
                    Title = $"接口调用失败，报错内容: {e.ToString()}",
                    PrimaryButtonText = "Ok"
                };
                await VMLocator.MainViewModel.ContentDialogShowAsync(dialog);
            }
        }
        /// <summary>
        /// 平台聊天
        /// </summary>
        /// <returns></returns>
        private async Task PostGptChatAsync()
        {
            if (string.IsNullOrEmpty(VMLocator.AppToken))
            {
                var dialog = new ContentDialog() {
                    Title = $"登陆信息为空/已过期",
                    PrimaryButtonText = "Ok"
                };
                await VMLocator.MainViewModel.ContentDialogShowAsync(dialog);
                return;
            }
            var userMessge = new Models.ChatMessage() { ConversationId = VMLocator.DataGridViewModel.SelectedItemId, Message = PostMessage, Role = "User", CreateDate = DateTime.Now };
            VMLocator.ChatViewModel.ChatHistory.Add(userMessge);
            await VMLocator.ChatDbcontext.ChatMessages.AddAsync(userMessge);
            await VMLocator.ChatDbcontext.SaveChangesAsync();
            TMHttpClient http = new TMHttpClient();
            var obj = new { Prompt =PostMessage, SystemMessage =SystemMessage, ConversationId =VMLocator.DataGridViewModel.SelectedItemId, Model =AppSettings.Instance.ApiModel, ModelType =1};
            var res =await http.PostAsync<Models.ChatMessage>("/api/v1/Chat/ChatCompletion", obj);
            if (res.StatusCode != 200) {
                var dialog = new ContentDialog() {
                    Title = $"接口报错：code：{res.StatusCode}.Msg:{JsonSerializer.Serialize(res.Message+res.Errors, new JsonSerializerOptions() {
                        Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
                    })}",
                    PrimaryButtonText = "Ok"
                };
                await VMLocator.MainViewModel.ContentDialogShowAsync(dialog);
                return;
            }
            var assistant = new Models.ChatMessage() { ConversationId = res.Data.ConversationId, Message = res.Data.Message, Role = "Assistant", CreateDate = DateTime.Now };
            VMLocator.ChatViewModel.ChatHistory.Add(assistant);
            await VMLocator.ChatDbcontext.ChatMessages.AddAsync(assistant);
            await VMLocator.ChatDbcontext.SaveChangesAsync();
            VMLocator.MainViewModel.PostMessage = "";
        }

        private async Task GptChatConversationList()
        {
            TMHttpClient http = new TMHttpClient();
            var obj = new { QueryString = SearchLogKeyword, PageIndex = 1, PageSize = 10};
            var res = await http.PostAsync<PagedRes<ChatConversationRes>>("/api/v1/Chat/ChatConversationList", obj);
            if (res.StatusCode != 200) {
                var dialog = new ContentDialog() {
                    Title = $"接口报错：code：{res.StatusCode}.Msg:{JsonSerializer.Serialize(res.Message + res.Errors, new JsonSerializerOptions() {
                        Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
                    })}",
                    PrimaryButtonText = "Ok"
                };
                await VMLocator.MainViewModel.ContentDialogShowAsync(dialog);
                return;
            }
            ObservableCollection<ChatList> resList =
                new ObservableCollection<ChatList>();
            foreach (var chatConversationRes in res.Data.Items)
            {
                ChatList i=new ChatList();
                i.Id = chatConversationRes.ConversationId;
                i.Category = chatConversationRes.ConversationName;
                i.Title= chatConversationRes.ConversationName;
                resList.Add(i);
            }
            VMLocator.DataGridViewModel.ChatList = resList;
        }
        #endregion
        #endregion
    }
}
