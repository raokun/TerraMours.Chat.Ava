using Avalonia.Collections;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.Json;
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
                this.RaiseAndSetIfChanged(ref _chatList, value);
                DataGridCollection = new DataGridCollectionView(ChatList);
                DataGridCollection.GroupDescriptions.Add(new DataGridPathGroupDescription("Category"));
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
        DatabaseProcess _dbProcess = new DatabaseProcess();
        private async void ShowChatLogAsync(long _selectedItem) {
            var _chatViewModel = VMLocator.ChatViewModel;

            if (!_chatViewModel.ChatIsRunning) {
                var result = await _dbProcess.GetChatLogDatabaseAsync(_selectedItem);

                _chatViewModel.ReEditIsOn = false;
                _chatViewModel.ChatTitle = result[0];
                if (!string.IsNullOrWhiteSpace(result[1])) {
                    _chatViewModel.ConversationHistory = JsonSerializer.Deserialize<List<Dictionary<string, object>>>(result[1]);
                }
                _chatViewModel.ChatCategory = result[3];
                _chatViewModel.LastPrompt = result[4];
                if (!string.IsNullOrWhiteSpace(result[5])) {
                    _chatViewModel.LastConversationHistory = JsonSerializer.Deserialize<List<Dictionary<string, object>>>(result[5]);
                }

                if (VMLocator.MainViewModel.SelectedLeftPane != "API Chat") {
                    VMLocator.MainViewModel.SelectedLeftPane = "API Chat";
                }
            }
        }
    }
}
