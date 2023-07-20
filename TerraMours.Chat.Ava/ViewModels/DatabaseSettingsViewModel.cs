using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using TerraMours.Chat.Ava.Models;
using TerraMours.Chat.Ava.Models.Class;
using TerraMours.Chat.Ava.Views;

namespace TerraMours.Chat.Ava.ViewModels {
    public class DatabaseSettingsViewModel : ViewModelBase {
        ChatProcess _chatPross=new ChatProcess();
        public DatabaseSettingsViewModel() {
            ProcessLog = "";

            MoveDatabaseCommand =  ReactiveCommand.CreateFromTask(MoveDatabaseAsync);
            LoadDatabaseCommand =  ReactiveCommand.CreateFromTask(LoadDatabaseAsync);

        }
        #region 事件
        public ICommand MoveDatabaseCommand { get; }
        public ICommand LoadDatabaseCommand { get; }
        #endregion

        #region 属性
        private AppSettings _appSettings => AppSettings.Instance;

        public string DatabasePath {
            get => _appSettings.DbPath;
            set {
                if (_appSettings.DbPath != value) {
                    _appSettings.DbPath = value;
                    this.RaisePropertyChanged(nameof(DatabasePath));
                }
            }
        }
        /// <summary>
        /// 提示
        /// </summary>
        private string _processLog;
        public string ProcessLog {
            get => _processLog;
            set => this.RaiseAndSetIfChanged(ref _processLog, value);
        }
        #endregion

        #region 方法
        private async Task MoveDatabaseAsync() {
            var dialog = new FilePickerSaveOptions {
                Title = "Move database file",
                SuggestedFileName = "log_database",
                FileTypeChoices = new List<FilePickerFileType>
            {new("Database files (*.db)") { Patterns = new[] { "*.db" } },
            new("All files (*.*)") { Patterns = new[] { "*" } }}
            };

            try {
                var result = await (Application.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime).MainWindow.StorageProvider.SaveFilePickerAsync(dialog);

                if (result != null) {
                    var selectedFilePath = result.Path.LocalPath;
                    string extension = Path.GetExtension(selectedFilePath);
                    if (string.IsNullOrEmpty(extension)) {
                        selectedFilePath += ".db";
                    }

                    if (selectedFilePath == DatabasePath) {
                        return;
                    }

                    File.Copy(DatabasePath, selectedFilePath, true);

                    if (File.Exists(selectedFilePath)) {
                        File.Delete(DatabasePath);

                        DatabasePath = selectedFilePath;
                        MainWindow mainWindow = (MainWindow)(Application.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime).MainWindow;
                        mainWindow.SaveWindowSizeAndPosition();
                        ProcessLog = "Database file moved successfully.";
                    }
                }
            }
            catch (Exception ex) {
                ProcessLog = "Error: " + ex.Message;
            }
        }

        private async Task LoadDatabaseAsync() {
            var dialog = new FilePickerOpenOptions {
                AllowMultiple = false,
                Title = "Select database file",
                FileTypeFilter = new List<FilePickerFileType>
                    {new("TXT files (*.db)") { Patterns = new[] { "*.db" } },
                    new("All files (*.*)") { Patterns = new[] { "*" } }}
            };
            var result = await (Application.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime).MainWindow.StorageProvider.OpenFilePickerAsync(dialog);

            if (result.Count > 0) {
                try {
                    var selectedFilePath = result[0].Path.LocalPath;
                    var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(selectedFilePath);

                    if (selectedFilePath == DatabasePath) {
                        return;
                    }

                    if (!await _chatPross.CheckTableExists(selectedFilePath)) {
                        ProcessLog = "Error: Invalid database file.";
                        return;
                    }

                    DatabasePath = selectedFilePath;

                    VMLocator.MainViewModel.SelectedPhraseItem = "";

                    VMLocator.DataGridViewModel.ChatList = VMLocator.ChatDbcontext.ChatLists.ToObservableCollection();

                    MainWindow mainWindow = (MainWindow)(Application.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime).MainWindow;
                    mainWindow.SaveWindowSizeAndPosition();

                    ProcessLog = "Database file loaded successfully.";
                }
                catch (Exception ex) {
                    ProcessLog = "Error: " + ex.Message;
                }
            }
        }
        #endregion
    }
}
