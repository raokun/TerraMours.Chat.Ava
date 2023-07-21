using DynamicData;
using FluentAvalonia.UI.Controls;
using OpenAI.Managers;
using OpenAI.ObjectModels.RequestModels;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using TerraMours.Chat.Ava.Models;
using Tmds.DBus.Protocol;

namespace TerraMours.Chat.Ava.ViewModels {
    public class ChatViewModel :ViewModelBase{
        public ChatViewModel() {
            ChatViewIsVisible = true;
            DeleteMessageCommand = ReactiveCommand.CreateFromTask(DeleteMessage);
        }
        #region 字段
        private bool _chatViewIsVisible;
        public bool ChatViewIsVisible //界面是否显示
        {
            get => _chatViewIsVisible;
            set => this.RaiseAndSetIfChanged(ref _chatViewIsVisible, value);
        }

        private bool _chatIsRunning;
        public bool ChatIsRunning //聊天接口正在输出
        {
            get => _chatIsRunning;
            set => this.RaiseAndSetIfChanged(ref _chatIsRunning, value);
        }
        private long _lastId;
        public long LastId //上一次的对话ID
        {
            get => _lastId;
            set => this.RaiseAndSetIfChanged(ref _lastId, value);
        }

        private bool _reEditIsOn;
        public bool ReEditIsOn //Post
        {
            get => _reEditIsOn;
            set {
                this.RaiseAndSetIfChanged(ref _reEditIsOn, value);
                    if (value) {
                        VMLocator.MainViewModel.PostButtonText = "Edit";
                    }
                    else {
                        VMLocator.MainViewModel.PostButtonText = "Post";
                    }
            }
        }

        private string _chatTitle;
        public string ChatTitle {
            get => _chatTitle;
            set => this.RaiseAndSetIfChanged(ref _chatTitle, value);
        }

        private string _chatCategory;
        public string ChatCategory {
            get => _chatCategory;
            set => this.RaiseAndSetIfChanged(ref _chatCategory, value);
        }

        private string _lastPrompt;
        public string LastPrompt {
            get => _lastPrompt;
            set => this.RaiseAndSetIfChanged(ref _lastPrompt, value);
        }

        private List<Dictionary<string, object>> _conversationHistory;
        public List<Dictionary<string, object>> ConversationHistory {
            get => _conversationHistory;
            set => this.RaiseAndSetIfChanged(ref _conversationHistory, value);
        }

        private List<Dictionary<string, object>> _lastConversationHistory;
        public List<Dictionary<string, object>> LastConversationHistory {
            get => _lastConversationHistory;
            set => this.RaiseAndSetIfChanged(ref _lastConversationHistory, value);
        }

        private double _chatViewFontSize;
        public double ChatViewFontSize {
            get => _chatViewFontSize;
            set => this.RaiseAndSetIfChanged(ref _chatViewFontSize, value);
        }

        #region 消息
        private ObservableCollection<Models.ChatMessage> _chatHistory;
        public ObservableCollection<Models.ChatMessage> ChatHistory { 
            get => _chatHistory;
            set => this.RaiseAndSetIfChanged(ref _chatHistory, value);
        }

        private Models.ChatMessage _chatMessage;
        public Models.ChatMessage ChatMessage {
            get => _chatMessage;
            set => this.RaiseAndSetIfChanged(ref _chatMessage, value);
        }
        #endregion
        #endregion
        #region 事件
        public ICommand DeleteMessageCommand { get; }
        #endregion
        #region 方法
        public void OpenApiSettings() {
            //VMLocator.ChatViewModel.ChatViewIsVisible = false;
            VMLocator.MainWindowViewModel.ApiSettingIsOpened = true;
            var i = VMLocator.MainWindowViewModel.ApiMaxTokens;
            VMLocator.MainWindowViewModel.ApiMaxTokens=100;
        }
        private async Task DeleteMessage() {
            return;
        }

        #endregion
    }
}
