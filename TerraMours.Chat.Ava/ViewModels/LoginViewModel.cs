using FluentAvalonia.UI.Controls;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Input;

namespace TerraMours.Chat.Ava.ViewModels {
    public partial class LoginViewModel : ViewModelBase {
        public LoginViewModel() {
            LoginCommand = ReactiveCommand.CreateFromTask(Login);
            ExitCommand = ReactiveCommand.Create(Exit);
        }

        #region 字段
        private string _userAccount;
        public string UserAccount {
            get => _userAccount;
            set => this.RaiseAndSetIfChanged(ref _userAccount, value);
        }

        private string _userPassword;
        public string UserPassword {
            get => _userPassword;
            set => this.RaiseAndSetIfChanged(ref _userPassword, value);
        }
        #endregion
        public Action LoginToMainAction { get; set; }
        #region 事件
        public ICommand LoginCommand { get; }
        public ICommand ExitCommand { get; set; }
        #endregion

        #region 方法
        private async Task Login() {
            if (string.IsNullOrEmpty(UserAccount) || string.IsNullOrEmpty(UserAccount)) {
                var dialog = new ContentDialog() {
                    Title = "用户名或密码为空。",
                    PrimaryButtonText = "Ok"
                };
                await VMLocator.MainViewModel.ContentDialogShowAsync(dialog);
                return;
            }
            VMLocator.LoginViewModel.LoginToMainAction?.Invoke();
        }

        private void Exit() {
            Environment.Exit(0);
        }
        #endregion
    }
}
