using FluentAvalonia.UI.Controls;
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
using System.Threading.Tasks;
using System.Windows.Input;
using TerraMours.Chat.Ava.Models;
using TerraMours.Chat.Ava.Models.Class;

namespace TerraMours.Chat.Ava.ViewModels {
    public partial class LoginViewModel : ViewModelBase {
        public LoginViewModel() {
            LoginCommand = ReactiveCommand.CreateFromTask(Login);
            ExitCommand = ReactiveCommand.Create(Exit);
            getLastUser();
        }
        private AppSettings _appSettings => AppSettings.Instance;
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

        private  void getLastUser()
        {
            var settings = AppSettings.Instance;
            if (File.Exists(Path.Combine(settings.AppDataPath, "user.dat"))){
                UserAccount= File.ReadAllText(Path.Combine(settings.AppDataPath, "user.dat"));
            }
        }
        private void SaveUser(string userName) {
            var settings = AppSettings.Instance;
            if (!Directory.Exists(settings.AppDataPath)) {
                Directory.CreateDirectory(settings.AppDataPath);
            }
                File.WriteAllText(Path.Combine(settings.AppDataPath, "user.dat"),userName);
        }

        private async Task Login() {
            if (string.IsNullOrEmpty(UserAccount) || string.IsNullOrEmpty(UserPassword)) {
                var dialog = new ContentDialog() {
                    Title = "用户名或密码为空。",
                    PrimaryButtonText = "Ok"
                };
                await VMLocator.MainViewModel.ContentDialogShowAsync(dialog);
                return;
            }

            TMHttpClient http = new TMHttpClient();
            var obj = new { UserAccount = UserAccount, UserPassword = UserPassword };
            var res = await http.PostAsync<LoginRes>("/api/v1/Login/Login", obj);
            if (res.StatusCode != 200)
            {
                var dialog = new ContentDialog() {
                    Title = $"接口报错：code：{res.StatusCode}.Msg:{JsonSerializer.Serialize(res.Errors, new JsonSerializerOptions() {
                        Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
                    })}",
                    PrimaryButtonText = "Ok"
                };
                await VMLocator.MainViewModel.ContentDialogShowAsync(dialog);
                return;
            }
            SaveUser(UserAccount);
            VMLocator.AppToken = res.Data.Token;
            VMLocator.LoginViewModel.LoginToMainAction?.Invoke();
        }

        private void Exit() {
            Environment.Exit(0);
        }
        #endregion
    }
}
