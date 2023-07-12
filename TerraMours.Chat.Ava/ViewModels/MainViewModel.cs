using Avalonia.Controls;
using FluentAvalonia.UI.Controls;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        public ICommand LoadChatListCommand { get; }
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
        #endregion
    }
}
