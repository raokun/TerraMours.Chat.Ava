using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerraMours.Chat.Ava.Models;
using TerraMours.Chat.Ava.ViewModels;

namespace TerraMours.Chat.Ava {
    internal class VMLocator {
        private static MainWindowViewModel _mainWindowViewModel;
        public static MainWindowViewModel MainWindowViewModel {
            get =>_mainWindowViewModel ??= new MainWindowViewModel();
            set => _mainWindowViewModel = value;
        }
        private static LoadViewModel _loadViewModel;
        public static LoadViewModel LoadViewModel {
            get=>_loadViewModel ??= new LoadViewModel();
            set => _loadViewModel = value;
        }
        private static MainViewModel _mainViewModel;
        public static MainViewModel MainViewModel {
            get=> _mainViewModel ??= new MainViewModel();
            set => _mainViewModel = value;
        }
        private static ChatViewModel _chatViewModel;
        public static ChatViewModel ChatViewModel {
            get=>_chatViewModel ??= new ChatViewModel();
            set => _chatViewModel = value;
        }
        private static DataGridViewModel _dataGridViewModel;
        public static DataGridViewModel DataGridViewModel {
            get=> (_dataGridViewModel ??= new DataGridViewModel());
            set => _dataGridViewModel = value;
        }
        private static DatabaseSettingsViewModel _databaseSettingsViewModel;
        public static DatabaseSettingsViewModel DatabaseSettingsViewModel {
            get => _databaseSettingsViewModel ??= new DatabaseSettingsViewModel();
            set => _databaseSettingsViewModel = value;
        }
        private static LoginViewModel _loginViewModel;
        public static LoginViewModel LoginViewModel {
            get => _loginViewModel ??= new LoginViewModel();
            set => _loginViewModel = value;
        }
        private static ChatDbcontext _chatDbcontext;
        public static ChatDbcontext ChatDbcontext {
            get => (_chatDbcontext ??= new ChatDbcontext());
            set => _chatDbcontext = value;
        }

        private static string _appToken;
        public static string AppToken {
            get => (_appToken);
            set => _appToken = value;
        }
    }
}
