using Avalonia.Collections;
using FluentAvalonia.UI.Controls;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using System.Threading.Tasks;
using TerraMours.Chat.Ava.Models;
using TerraMours.Chat.Ava.Models.Class;

namespace TerraMours.Chat.Ava.ViewModels {
    public class DataGridViewModel : ViewModelBase {

        #region 
        public long SelectedItemId => _selectedItem?.Id ?? default;

        private int _selectedItemIndex;
        public int SelectedItemIndex {
            get => _selectedItemIndex;
            set => this.RaiseAndSetIfChanged(ref _selectedItemIndex, value);
        }

        private bool _dataGridIsFocused;
        public bool DataGridIsFocused {
            get => _dataGridIsFocused;
            set => this.RaiseAndSetIfChanged(ref _dataGridIsFocused, value);
        }

        private DataGridCollectionView _dataGridCollection;
        public DataGridCollectionView DataGridCollection {
            get => _dataGridCollection;
            set => this.RaiseAndSetIfChanged(ref _dataGridCollection, value);
        }
        private ObservableCollection<ChatList> _chatList;
        public ObservableCollection<ChatList> ChatList {
            get => _chatList;
            set {
                if (_chatList != value) {
                    _chatList = value;
                    DataGridCollection = new DataGridCollectionView(ChatList);
                    DataGridCollection.GroupDescriptions.Add(new DataGridPathGroupDescription("Category"));
                    this.RaisePropertyChanged(nameof(ChatList));
                }
            }
        }


        private ChatList _selectedItem;
        public ChatList SelectedItem {
            get => _selectedItem;
            set {
                this.RaiseAndSetIfChanged(ref _selectedItem, value);

                if (SelectedItemId != -1 && _selectedItem != null && !VMLocator.ChatViewModel.ChatIsRunning && DataGridIsFocused) {
                    VMLocator.ChatViewModel.LastId = _selectedItem.Id;
                    ShowChatLogAsync(_selectedItem.Id);
                }
                else if (!string.IsNullOrWhiteSpace(VMLocator.MainViewModel.SearchLogKeyword)) {
                    SelectedItemIndex = -1;
                }
            }
        }
        #endregion
        private async void ShowChatLogAsync(long _selectedItem) {
            var _chatViewModel = VMLocator.ChatViewModel;

            if (!_chatViewModel.ChatIsRunning) {
                if (VMLocator.MainViewModel.SelectedLeftPane != "AI Chat") {
                    VMLocator.MainViewModel.SelectedLeftPane = "AI Chat";
                }
            }

            GptChatListByConversationId(_selectedItem);
        }

        private async Task GptChatListByConversationId(long conversationId)
        {
            TMHttpClient http = new TMHttpClient();
            var obj = new { ConversationId = conversationId, PageIndex = 1, PageSize = 20 };
            var res = await http.PostAsync<PagedRes<Models.ChatMessage>>("/api/v1/Chat/ChatRecordList", obj);
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
            ObservableCollection<Models.ChatMessage> observableCollection = new ObservableCollection<Models.ChatMessage>(res.Data.Items);
            VMLocator.ChatViewModel.ChatHistory= observableCollection;
        }
    }
}
