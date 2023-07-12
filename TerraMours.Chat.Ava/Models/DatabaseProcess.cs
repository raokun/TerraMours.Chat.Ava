using Avalonia;
using Avalonia.Controls;
using CsvHelper;
using CsvHelper.Configuration;
using FluentAvalonia.UI.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SQLite;
using System.Formats.Asn1;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TerraMours.Chat.Ava.Models.Class;

namespace TerraMours.Chat.Ava.Models {
    /// <summary>
    /// 数据库
    /// </summary>
    public class DatabaseProcess {
        public static SQLiteConnection memoryConnection; // メモリ上のSQLコネクション

        // SQL db初期化--------------------------------------------------------------
        public void CreateDatabase() {
            using var connection = new SQLiteConnection($"Data Source={AppSettings.Instance.DbPath}");
            string sql = "CREATE TABLE phrase (id INTEGER PRIMARY KEY AUTOINCREMENT, name TEXT NOT NULL DEFAULT '', phrase TEXT NOT NULL DEFAULT '');";

            using var command = new SQLiteCommand(sql, connection);
            // phraseテーブル作成
            connection.Open();
            command.ExecuteNonQuery();

            // phraseインデックス作成
            sql = "CREATE INDEX idx_text ON phrase (phrase);";
            command.CommandText = sql;
            command.ExecuteNonQuery();

            // chatlogテーブル作成
            sql = "CREATE TABLE chatlog (id INTEGER PRIMARY KEY AUTOINCREMENT, date DATE, title TEXT NOT NULL DEFAULT '', json TEXT NOT NULL DEFAULT '', text TEXT NOT NULL DEFAULT '', category TEXT NOT NULL DEFAULT '', lastprompt TEXT NOT NULL DEFAULT '', jsonprev TEXT NOT NULL DEFAULT '');";
            command.CommandText = sql;
            command.ExecuteNonQuery();

            // chatlogインデックス作成
            sql = "CREATE INDEX idx_chat_text ON chatlog (text);";
            command.CommandText = sql;
            command.ExecuteNonQuery();

            // editorlogテーブル作成
            sql = "CREATE TABLE editorlog (id INTEGER PRIMARY KEY AUTOINCREMENT, date DATE, text TEXT NOT NULL DEFAULT '');";
            command.CommandText = sql;
            command.ExecuteNonQuery();

            // editorlogインデックス作成
            sql = "CREATE INDEX idx_editor_text ON editorlog (text);";
            command.CommandText = sql;
            command.ExecuteNonQuery();

            // templateテーブル作成
            sql = "CREATE TABLE template (id INTEGER PRIMARY KEY AUTOINCREMENT, title TEXT NOT NULL DEFAULT '', text TEXT NOT NULL DEFAULT '');";
            command.CommandText = sql;
            command.ExecuteNonQuery();

            // templateインデックス作成
            sql = "CREATE INDEX idx_template_text ON editorlog (text);";
            command.CommandText = sql;
            command.ExecuteNonQuery();
        }

        // データベースのチャットログをバージョンアップ--------------------------------------------------------------
        public async Task UpdateChatLogDatabaseAsync() {
            try {
                // SQLiteデータベースに接続
                using SQLiteConnection connection = new SQLiteConnection($"Data Source={AppSettings.Instance.DbPath}");
                await connection.OpenAsync();

                bool categoryExists = false;
                bool lastPromptExists = false;
                bool jsonPrevExists = false;

                using (var command = new SQLiteCommand(connection)) {
                    // Check 'category' column
                    command.CommandText = "PRAGMA table_info(chatlog)";
                    using (var reader = await command.ExecuteReaderAsync()) {
                        while (reader.Read()) {
                            var columnName = reader["name"].ToString();
                            if (columnName == "category") {
                                categoryExists = true;
                            }
                            else if (columnName == "lastprompt") {
                                lastPromptExists = true;
                            }
                            else if (columnName == "jsonprev") {
                                jsonPrevExists = true;
                            }
                        }
                    }

                    // Backup database
                    if (!categoryExists || !lastPromptExists || !jsonPrevExists) {
                        string sourceFile = AppSettings.Instance.DbPath;
                        string backupFile = AppSettings.Instance.DbPath + ".backup";

                        // Ensure the target does not exist.
                        if (File.Exists(backupFile)) {
                            File.Delete(backupFile);
                        }

                        // Copy the file.
                        File.Copy(sourceFile, backupFile);
                    }

                    // Add 'category' column
                    if (!categoryExists) {
                        command.CommandText = "ALTER TABLE chatlog ADD COLUMN category TEXT NOT NULL DEFAULT ''";
                        await command.ExecuteNonQueryAsync();
                    }

                    // Add 'lastprompt' column
                    if (!lastPromptExists) {
                        command.CommandText = "ALTER TABLE chatlog ADD COLUMN lastprompt TEXT NOT NULL DEFAULT ''";
                        await command.ExecuteNonQueryAsync();
                    }

                    // Add 'jsonprev' column
                    if (!jsonPrevExists) {
                        command.CommandText = "ALTER TABLE chatlog ADD COLUMN jsonprev TEXT NOT NULL DEFAULT ''";
                        await command.ExecuteNonQueryAsync();
                    }
                }

                if (!categoryExists || !lastPromptExists) {
                    Application.Current!.TryFindResource("My.Strings.DatabaseUpdate", out object resource1);
                    var dialog = new ContentDialog() {
                        Title = $"{resource1}{Environment.NewLine}{Environment.NewLine}{AppSettings.Instance.DbPath}.backup",
                        PrimaryButtonText = "OK"
                    };
                    await VMLocator.MainViewModel.ContentDialogShowAsync(dialog);
                }
            }
            catch (Exception ex) {
                var dialog = new ContentDialog() {
                    Title = $"Error: {ex}",
                    PrimaryButtonText = "OK"
                };
                await VMLocator.MainViewModel.ContentDialogShowAsync(dialog);
                throw;
            }
            // インメモリをいったん閉じてまた開く
            await memoryConnection.CloseAsync();
            await DbLoadToMemoryAsync();
        }


        // SQL dbファイルをメモリにロード--------------------------------------------------------------
        public async Task DbLoadToMemoryAsync() {
            var fileConnection = new SQLiteConnection($"Data Source={AppSettings.Instance.DbPath}");
            fileConnection.Open();
            // メモリ上のDBファイルを作成
            memoryConnection = new SQLiteConnection("Data Source=:memory:");
            memoryConnection.Open();
            try {
                // SQL dbをメモリにコピー
                fileConnection.BackupDatabase(memoryConnection, "main", "main", -1, null, 0);
            }
            catch (Exception ex) {
                var dialog = new ContentDialog() {
                    Title = $"Error: {ex}",
                    PrimaryButtonText = "Ok"
                };
                await VMLocator.MainViewModel.ContentDialogShowAsync(dialog);
            }
            fileConnection.Close();
        }

        // 定型句プリセットSave--------------------------------------------------------------
        public async Task SavePhrasesAsync(string name, string phrasesText) {
            using var connection = new SQLiteConnection($"Data Source={AppSettings.Instance.DbPath}");
            await connection.OpenAsync();

            using var transaction = connection.BeginTransaction();
            try {
                string sql = $"INSERT INTO phrase (name, phrase) VALUES (@name, @phrase)";

                using var command = new SQLiteCommand(sql, connection);
                command.Parameters.AddWithValue("@name", name);
                command.Parameters.AddWithValue("@phrase", phrasesText);

                await command.ExecuteNonQueryAsync();

                transaction.Commit();
            }
            catch (Exception) {
                transaction.Rollback();
                throw;
            }
            await memoryConnection.CloseAsync();
            await DbLoadToMemoryAsync();
        }

        // 定型句プリセット一覧Load--------------------------------------------------------------
        public async Task<List<string>> GetPhrasesAsync() {
            List<string> phrases = new List<string>();
            string sql = "SELECT name FROM phrase ORDER BY name COLLATE NOCASE";

            using (var command = new SQLiteCommand(sql, memoryConnection)) {
                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync()) {
                    phrases.Add(reader.GetString(0));
                }
            }
            return phrases;
        }

        // 定型句プリセット実体Load--------------------------------------------------------------
        public async Task<List<string>> GetPhrasePresetsAsync(string selectedPhraseItem) {
            List<string> phrases = new List<string>();
            string sql = "SELECT phrase FROM phrase WHERE name = @selectedPhraseItem";
            try {
                using var command = new SQLiteCommand(sql, memoryConnection);
                command.Parameters.AddWithValue("@selectedPhraseItem", selectedPhraseItem);

                using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync()) {
                    phrases = reader.GetString(0).Split(Environment.NewLine).ToList();
                }
            }
            catch (Exception) {
                throw;
            }
            return phrases;
        }

        // 定型句プリセットRename--------------------------------------------------------------
        public async Task UpdatePhrasePresetNameAsync(string oldName, string newName) {
            using var connection = new SQLiteConnection($"Data Source={AppSettings.Instance.DbPath}");
            await connection.OpenAsync();

            using var transaction = connection.BeginTransaction();
            try {
                using var command = new SQLiteCommand(connection) {
                    CommandText = "UPDATE phrase SET name = @newName WHERE name = @oldName;"
                };

                command.Parameters.AddWithValue("@oldName", oldName);
                command.Parameters.AddWithValue("@newName", newName);

                int rowsAffected = await command.ExecuteNonQueryAsync();
                if (rowsAffected == 0) {
                    throw new Exception("No matching record found to update.");
                }
                transaction.Commit();
            }
            catch (Exception ex) {
                transaction.Rollback();
                throw new Exception($"Updating the name from '{oldName}' to '{newName}': {ex.Message}", ex);
            }
            await memoryConnection.CloseAsync();
            await DbLoadToMemoryAsync();
        }

        // 定型句プリセットUpdate--------------------------------------------------------------
        public async Task UpdatePhrasePresetAsync(string name, string phrasesText) {
            using var connection = new SQLiteConnection($"Data Source={AppSettings.Instance.DbPath}");
            await connection.OpenAsync();

            using var transaction = connection.BeginTransaction();
            try {
                using var command = new SQLiteCommand(connection) {
                    CommandText = "UPDATE phrase SET phrase = @phrasesText WHERE name = @name;"
                };

                command.Parameters.AddWithValue("@name", name);
                command.Parameters.AddWithValue("@phrasesText", phrasesText);

                int rowsAffected = await command.ExecuteNonQueryAsync();
                if (rowsAffected == 0) {
                    throw new Exception("No matching record found to update.");
                }
                transaction.Commit();
            }
            catch (Exception ex) {
                transaction.Rollback();
                throw new Exception($"Updating the phrase preset '{name}': {ex.Message}", ex);
            }
            await memoryConnection.CloseAsync();
            await DbLoadToMemoryAsync();
        }

        // 定型句プリセットDelete--------------------------------------------------------------
        public async Task DeletePhrasePresetAsync(string selectedPhraseItem) {
            try {
                using var connection = new SQLiteConnection($"Data Source={AppSettings.Instance.DbPath}");
                await connection.OpenAsync();

                using var transaction = connection.BeginTransaction(System.Data.IsolationLevel.Serializable);
                try {
                    string sql = "DELETE FROM phrase WHERE name = @selectedPhraseItem";
                    using var command = new SQLiteCommand(sql, connection, transaction);
                    command.Parameters.AddWithValue("@selectedPhraseItem", selectedPhraseItem);

                    await command.ExecuteNonQueryAsync();
                    transaction.Commit();
                }
                catch (Exception ex) {
                    transaction.Rollback();
                    throw new Exception("Occurred while deleting the selected preset.", ex);
                }
            }
            catch (Exception ex) {
                throw new Exception("Occurred while connecting to the database.", ex);
            }
            await memoryConnection.CloseAsync();
            await DbLoadToMemoryAsync();
        }

        // 定型句プリセットImport--------------------------------------------------------------
        public async Task<ObservableCollection<string>> ImportPhrasesFromTxtAsync(string selectedFilePath) {
            ObservableCollection<string> phrases = new ObservableCollection<string>();

            try {
                // Check if the file exists
                if (!File.Exists(selectedFilePath)) {
                    throw new FileNotFoundException("The specified file does not exist.", selectedFilePath);
                }

                // Read the file asynchronously
                using (StreamReader reader = new StreamReader(selectedFilePath)) {
                    int lineCount = 0;
                    while (lineCount < 20) {
                        if (reader.EndOfStream) {
                            phrases.Add(""); // Add an empty string if there are less than 20 lines
                        }
                        else {
                            phrases.Add(await reader.ReadLineAsync());
                        }
                        lineCount++;
                    }
                }
            }
            catch (Exception) {
                throw;
            }
            return phrases;
        }

        // CSVインポート--------------------------------------------------------------
        public async Task<string> ImportCsvToTableAsync(string fileName, string tableName = "chatlog") {
            string msg;
            int processedCount = 0;
            int columnEnd;
            string columnNames;
            if (tableName == "editorlog") {
                columnEnd = 2;
                columnNames = "date, text";
            }
            else {
                columnEnd = 6;
                columnNames = "date, title, json, text, category, lastprompt, jsonprev";
            }

            try {
                // CSVファイルからデータを読み込む
                using var reader = new StreamReader(fileName, System.Text.Encoding.UTF8);
                var config = new CsvConfiguration(CultureInfo.InvariantCulture) {
                    HasHeaderRecord = true,
                    Delimiter = ","
                };
                using var csvReader = new CsvReader(reader, config);
                csvReader.Read(); // ヘッダー行をスキップ

                using var con = new SQLiteConnection($"Data Source={AppSettings.Instance.DbPath}");
                await con.OpenAsync();
                using (var transaction = con.BeginTransaction()) {
                    try {
                        while (await csvReader.ReadAsync()) // データ行を読み込む
                        {

                            // データを取得
                            var rowData = new List<string>();
                            for (int i = 1, loopTo = columnEnd; i <= loopTo; i++) // 2列目から8列目まで
                                rowData.Add(csvReader.GetField(i));
                            // INSERT文を作成
                            string values = string.Join(", ", Enumerable.Range(0, rowData.Count).Select(i => $"@value{i}"));

                            string insertQuery = $"INSERT INTO {tableName} ({columnNames}) VALUES ({values});";

                            // データをデータベースに挿入
                            using (var command = new SQLiteCommand(insertQuery, con)) {
                                for (int i = 0, loopTo1 = rowData.Count - 1; i <= loopTo1; i++)
                                    command.Parameters.AddWithValue($"@value{i}", rowData[i]);
                                await command.ExecuteNonQueryAsync();
                            }
                            processedCount += 1;
                        }

                        transaction.Commit();
                        msg = $"Successfully imported log. ({processedCount} Records)";
                    }
                    catch (Exception) {
                        transaction.Rollback();
                        throw;
                    }
                }
                await con.CloseAsync();
            }
            catch (Exception) {
                throw;
            }
            // インメモリをいったん閉じてまた開く
            await memoryConnection.CloseAsync();
            await DbLoadToMemoryAsync();

            return msg;
        }

        // CSVエクスポート--------------------------------------------------------------
        public async Task<string> ExportTableToCsvAsync(string fileName, string tableName = "chatlog") {
            string msg;
            try {
                int processedCount = 0;

                // SELECT クエリを実行し、テーブルのデータを取得
                var command = new SQLiteCommand($"SELECT * FROM {tableName};", memoryConnection);
                using (SQLiteDataReader reader = (SQLiteDataReader)await command.ExecuteReaderAsync()) {

                    // CSV ファイルに書き込むための StreamWriter を作成
                    using var writer = new StreamWriter(fileName, false, System.Text.Encoding.UTF8);

                    // CsvWriter を作成し、設定を適用
                    var config = new CsvConfiguration(CultureInfo.InvariantCulture) {
                        HasHeaderRecord = true,
                        Delimiter = ","
                    };
                    using var csvWriter = new CsvWriter(writer, config);

                    var commandRowCount = new SQLiteCommand($"SELECT COUNT(*) FROM {tableName};", memoryConnection);
                    int rowCount = Convert.ToInt32(commandRowCount.ExecuteScalar());

                    // ヘッダー行を書き込む
                    for (int i = 0, loopTo = reader.FieldCount - 1; i <= loopTo; i++)
                        csvWriter.WriteField(reader.GetName(i));
                    csvWriter.NextRecord();

                    // データ行を書き込む

                    while (await reader.ReadAsync()) {
                        for (int i = 0, loopTo1 = reader.FieldCount - 1; i <= loopTo1; i++) {
                            if (reader.GetFieldType(i) == typeof(DateTime)) {
                                var dateValue = reader.GetDateTime(i);
                                csvWriter.WriteField(dateValue.ToString("yyyy-MM-dd HH:mm:ss.fffffff"));
                            }
                            else {
                                csvWriter.WriteField(reader.GetValue(i));
                            }
                        }
                        csvWriter.NextRecord();
                        // Report progress
                        processedCount += 1;
                        int progressPercentage = (int)Math.Round(processedCount / (double)rowCount * 100d);
                    }
                }
                msg = $"Successfully exported log. ({processedCount} Records)";
                return msg;
            }
            catch (Exception) {
                throw;
            }
        }

        // データベースからチャットログを検索--------------------------------------------------------------
        public async Task<ObservableCollection<ChatList>> SearchChatDatabaseAsync(string searchKey = "") {
            string query;
            if (string.IsNullOrEmpty(searchKey)) {
                query = "SELECT id, date, title, category FROM chatlog ORDER BY date DESC;";
            }
            else {
                query = "SELECT id, date, title, category FROM chatlog WHERE LOWER(text) LIKE LOWER(@searchKey) ORDER BY date DESC;";
            }

            var chatList = new ObservableCollection<ChatList>();
            using (var cmd = new SQLiteCommand(query, memoryConnection)) {
                if (!string.IsNullOrEmpty(searchKey)) {
                    searchKey = "%" + searchKey.ToLower() + "%";
                    cmd.Parameters.AddWithValue("@searchKey", searchKey);
                }
                using SQLiteDataReader reader = (SQLiteDataReader)await cmd.ExecuteReaderAsync();
                bool isFirstLine = true;
                while (await reader.ReadAsync()) {
                    var chatItem = new ChatList();
                    if (isFirstLine) {
                        chatItem.Id = reader.GetInt32(0);
                        isFirstLine = false;
                    }
                    else {
                        chatItem.Id = reader.GetInt32(0);
                    }
                    chatItem.Date = reader.GetDateTime(1);
                    chatItem.Title = reader.GetString(2);
                    chatItem.Category = reader.GetString(3);
                    chatList.Add(chatItem);
                }
            }
            return chatList;
        }

        // データベースから表示用チャットログを取得--------------------------------------------------------------
        public async Task<List<string>> GetChatLogDatabaseAsync(long chatId) {
            string query = $"SELECT title, json, text, category, lastprompt, jsonprev FROM chatlog WHERE id = {chatId}";
            var result = new List<string>();
            using (var cmd = new SQLiteCommand(query, memoryConnection)) {
                using SQLiteDataReader reader = (SQLiteDataReader)await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync()) {
                    result.Add(reader.GetString(0));
                    result.Add(reader.GetString(1));
                    result.Add(reader.GetString(2));
                    result.Add(reader.GetString(3));
                    result.Add(reader.GetString(4));
                    result.Add(reader.GetString(5));
                }
            }
            return result;
        }

        // チャットログ削除--------------------------------------------------------------
        public async Task DeleteChatLogDatabaseAsync(long chatId) {
            using (var connection = new SQLiteConnection($"Data Source={AppSettings.Instance.DbPath}")) {
                connection.Open();
                using var transaction = connection.BeginTransaction();
                try {
                    using (var command = new SQLiteCommand($"DELETE FROM chatlog WHERE id = '{chatId}'", connection, transaction)) {
                        await command.ExecuteNonQueryAsync();
                    }
                    transaction.Commit();
                }
                catch (Exception) {
                    transaction.Rollback();
                    throw;
                }
            }
            // インメモリをいったん閉じてまた開く
            await memoryConnection.CloseAsync();
            await DbLoadToMemoryAsync();
            return;
        }

        // タイトルの更新--------------------------------------------------------------
        public async Task UpdateTitleDatabaseAsync(long chatId, string title) {
            try {
                using var connection = new SQLiteConnection($"Data Source={AppSettings.Instance.DbPath}");
                await connection.OpenAsync();

                string query = $"UPDATE chatlog SET title=@title WHERE id = {chatId}";
                using var command = new SQLiteCommand(query, connection);
                command.Parameters.AddWithValue("@title", title);

                await command.ExecuteNonQueryAsync();
            }
            catch (Exception) {
                throw;
            }
            // インメモリをいったん閉じてまた開く
            await memoryConnection.CloseAsync();
            await DbLoadToMemoryAsync();
            VMLocator.DataGridViewModel.ChatList = await SearchChatDatabaseAsync();
        }

        // カテゴリの更新--------------------------------------------------------------
        public async Task UpdateCategoryDatabaseAsync(long chatId, string category) {
            try {
                using var connection = new SQLiteConnection($"Data Source={AppSettings.Instance.DbPath}");
                await connection.OpenAsync();

                string query = $"UPDATE chatlog SET category=@category WHERE id = {chatId}";

                using var command = new SQLiteCommand(query, connection);

                command.Parameters.AddWithValue("@category", category);
                await command.ExecuteNonQueryAsync();
            }
            catch (Exception) {
                throw;
            }
            // インメモリをいったん閉じてまた開く
            await memoryConnection.CloseAsync();
            await DbLoadToMemoryAsync();
            VMLocator.DataGridViewModel.ChatList = await SearchChatDatabaseAsync();
        }

        // Webチャットログのインポート--------------------------------------------------------------
        public async Task<string> InsertWebChatLogDatabaseAsync(string webChatTitle, List<Dictionary<string, object>> webConversationHistory, string webLog, string chatService) {
            if (!string.IsNullOrEmpty(webLog)) {
                int? matchingId = null;
                string query = "";

                using (var command = new SQLiteCommand(memoryConnection)) {
                    command.CommandText = "SELECT id FROM chatlog WHERE title = @webChatTitle LIMIT 1";
                    command.Parameters.AddWithValue("@webChatTitle", webChatTitle);

                    using var reader = await command.ExecuteReaderAsync();
                    if (await reader.ReadAsync()) {
                        matchingId = reader.GetInt32(0);
                    }
                }

                if (matchingId.HasValue) {
                    Console.WriteLine("Match found. ID: " + matchingId.Value);
                    var dialog = new ContentDialog() { Title = $"A chat log with the same name exists.{Environment.NewLine}Do you want to overwrite it? {Environment.NewLine}{Environment.NewLine}'{webChatTitle}'", PrimaryButtonText = "Overwrite", SecondaryButtonText = "Rename", CloseButtonText = "Cancel" };
                    var dialogResult = await VMLocator.MainViewModel.ContentDialogShowAsync(dialog);
                    if (dialogResult == ContentDialogResult.Primary) {
                        query = $"UPDATE chatlog SET date=@date, title=@title, json=@json, text=@text, category=category WHERE id={matchingId.Value}";
                    }
                    else if (dialogResult == ContentDialogResult.Secondary) {
                        dialog = new ContentDialog() {
                            Title = "Please enter a new chat name.",
                            PrimaryButtonText = "OK",
                            CloseButtonText = "Cancel"
                        };

                    }
                    else {
                        return "Cancel";
                    }
                }
                else {
                    query = "INSERT INTO chatlog(date, title, json, text, category) VALUES (@date, @title, @json, @text, @category)";
                }

                DateTime nowDate = DateTime.Now;
                string jsonConversationHistory = JsonSerializer.Serialize(webConversationHistory);

                using var connection = new SQLiteConnection($"Data Source={AppSettings.Instance.DbPath}");
                await connection.OpenAsync();
                // トランザクションを開始する
                using var transaction = connection.BeginTransaction();
                try {
                    // logテーブルにデータをインサートする
                    using (var command = new SQLiteCommand(query, connection)) {
                        await Task.Run(() => command.Parameters.AddWithValue("@date", nowDate));
                        await Task.Run(() => command.Parameters.AddWithValue("@title", webChatTitle));
                        await Task.Run(() => command.Parameters.AddWithValue("@json", jsonConversationHistory));
                        await Task.Run(() => command.Parameters.AddWithValue("@text", webLog));
                        await Task.Run(() => command.Parameters.AddWithValue("@category", chatService));
                        await command.ExecuteNonQueryAsync();
                    }

                    // トランザクションをコミットする
                    await Task.Run(() => transaction.Commit());
                }
                catch (Exception) {
                    // エラーが発生した場合、トランザクションをロールバックする
                    transaction.Rollback();
                    throw;
                }
            }
            // インメモリをいったん閉じてまた開く
            await memoryConnection.CloseAsync();
            await DbLoadToMemoryAsync();
            VMLocator.DataGridViewModel.ChatList = await SearchChatDatabaseAsync();
            return "OK";
        }


        // Template Rename--------------------------------------------------------------
        public async Task UpdateTemplateNameAsync(string oldName, string newName) {
            using var connection = new SQLiteConnection($"Data Source={AppSettings.Instance.DbPath}");
            await connection.OpenAsync();

            using var transaction = connection.BeginTransaction();
            try {
                using var command = new SQLiteCommand(connection) {
                    CommandText = "UPDATE template SET title = @newName WHERE title = @oldName;"
                };

                command.Parameters.AddWithValue("@oldName", oldName);
                command.Parameters.AddWithValue("@newName", newName);

                int rowsAffected = await command.ExecuteNonQueryAsync();
                if (rowsAffected == 0) {
                    throw new Exception("No matching record found to update.");
                }
                transaction.Commit();
            }
            catch (Exception ex) {
                transaction.Rollback();
                throw new Exception($"Updating the name from '{oldName}' to '{newName}': {ex.Message}", ex);
            }
            await memoryConnection.CloseAsync();
            await DbLoadToMemoryAsync();
        }

        // Template Delete--------------------------------------------------------------
        public async Task DeleteTemplateAsync(string selectedTemplateItem) {
            try {
                using var connection = new SQLiteConnection($"Data Source={AppSettings.Instance.DbPath}");
                await connection.OpenAsync();

                using var transaction = connection.BeginTransaction(System.Data.IsolationLevel.Serializable);
                try {
                    string sql = "DELETE FROM template WHERE title = @selectedTemplateItem";
                    using var command = new SQLiteCommand(sql, connection, transaction);
                    command.Parameters.AddWithValue("@selectedTemplateItem", selectedTemplateItem);

                    await command.ExecuteNonQueryAsync();
                    transaction.Commit();
                }
                catch (Exception ex) {
                    transaction.Rollback();
                    throw new Exception("Occurred while deleting the selected template.", ex);
                }
            }
            catch (Exception ex) {
                throw new Exception("Occurred while connecting to the database.", ex);
            }
            await memoryConnection.CloseAsync();
            await DbLoadToMemoryAsync();
        }

        // Template Import--------------------------------------------------------------
        public async Task<string> ImportTemplateFromTxtAsync(string selectedFilePath) {
            string text = "";

            try {
                // Check if the file exists
                if (!File.Exists(selectedFilePath)) {
                    throw new FileNotFoundException("The specified file does not exist.", selectedFilePath);
                }

                // Read the file asynchronously
                using (StreamReader reader = new StreamReader(selectedFilePath)) {
                    int lineCount = 0;
                    while (lineCount > -1) {
                        if (reader.EndOfStream) {
                            break;
                        }
                        else {
                            text = text + await reader.ReadLineAsync();
                        }
                        lineCount++;
                    }
                    
                }
            }
            catch (Exception) {
                throw;
            }
            return text;
        }







        // データベースのEditorログをクリンナップ--------------------------------------------------------------
        public async Task CleanUpEditorLogDatabaseAsync() {
            // SQLiteデータベースに接続
            using SQLiteConnection connection = new SQLiteConnection($"Data Source={AppSettings.Instance.DbPath}");
            await connection.OpenAsync();

            // テーブルの行数を取得
            using (SQLiteCommand command = new SQLiteCommand("SELECT COUNT(*) FROM editorlog", connection)) {
                var rowCount = (long)command.ExecuteScalar();

                // 行数が500を超えている場合
                if (rowCount > 500) {
                    // 日付が新しいもの500を残して削除
                    using (SQLiteCommand deleteCommand = new SQLiteCommand(@"DELETE FROM editorlog WHERE rowid NOT IN ( SELECT rowid FROM editorlog ORDER BY date DESC LIMIT 500 )", connection)) {
                        await deleteCommand.ExecuteNonQueryAsync();
                    }
                }
            }
            await connection.CloseAsync();
        }

        // チャットログを更新--------------------------------------------------------------
        public async Task InsertDatabaseChatAsync(DateTime postDate, string postText, DateTime resDate, string resText) {
            var insertText = new List<string>();

            if (!string.IsNullOrWhiteSpace(resText)) {
                insertText = new List<string>
                {
                    $"[{postDate}] by You" + Environment.NewLine,
                    postText + Environment.NewLine,
                    "(!--editable--)" + Environment.NewLine,
                    $"[{resDate}] by AI",
                    resText
                };
            }
            else {
                // AIの返答が空の場合(システムメッセージのみ)
                insertText = new List<string>
                {
                    $"[{postDate}] by You" + Environment.NewLine,
                    postText +
                    "---" + Environment.NewLine,
                    "(!--editable--)" + Environment.NewLine,
                };
            }
            string promptTextForSave = postText;

           long lastRowId = VMLocator.ChatViewModel.LastId;
            string titleText = VMLocator.ChatViewModel.ChatTitle;
            if (string.IsNullOrWhiteSpace(titleText)) {
                titleText = "";
            }

            string categoryText = VMLocator.ChatViewModel.ChatCategory;
            if (string.IsNullOrWhiteSpace(categoryText)) {
                categoryText = "";
            }

            string jsonConversationHistory = JsonSerializer.Serialize(VMLocator.ChatViewModel.ConversationHistory);
            string jsonLastConversationHistory = JsonSerializer.Serialize(VMLocator.ChatViewModel.LastConversationHistory);
            if (string.IsNullOrWhiteSpace(jsonLastConversationHistory)) {
                jsonLastConversationHistory = "";
            }

            using (var connection = new SQLiteConnection($"Data Source={AppSettings.Instance.DbPath}")) {
                connection.Open();
                // トランザクションを開始する
                using var transaction = connection.BeginTransaction();
                try {
                    if (lastRowId != -1) {
                        // 指定されたIDのデータを取得する
                        string currentText = "";
                        using (var command = new SQLiteCommand("SELECT text FROM chatlog WHERE id=@id", connection)) {
                            command.Parameters.AddWithValue("@id", lastRowId);
                            using SQLiteDataReader reader = (SQLiteDataReader)await command.ExecuteReaderAsync();
                            if (reader.Read()) {
                                currentText = reader.GetString(0);
                            }
                        }

                        currentText = Regex.Replace(currentText, @"\r\n|\r|\n", Environment.NewLine).Trim() + Environment.NewLine + Environment.NewLine; ;

                        string searchText = $"(!--editable--){Environment.NewLine}";
                        string byYouText = "] by You";

                        if (VMLocator.ChatViewModel.ReEditIsOn) {
                            // 既存のテキストに(!--editable--)を見つけたら、直前の[*] by Youから最後までを削除する
                            if (currentText.Contains(searchText)) {
                                int editableIndex = currentText.IndexOf(searchText);
                                string textBeforeEditable = currentText.Substring(0, editableIndex);
                                int lastByYouIndex = textBeforeEditable.LastIndexOf(byYouText);
                                if (lastByYouIndex >= 0) {
                                    int lastNewLineIndex = textBeforeEditable.LastIndexOf(Environment.NewLine, lastByYouIndex);
                                    if (lastNewLineIndex >= 0) {
                                        currentText = textBeforeEditable.Substring(0, lastNewLineIndex).Trim();
                                    }
                                    else {
                                        // lastByYouIndex以前に改行が存在しない場合は初回メッセージと判断
                                        currentText = "";
                                    }
                                }
                                else {
                                    // [*] by Youが存在しない場合の処理をここに書く
                                    throw new Exception("Error : Incorrect log data. [*] by You ");
                                }
                            }
                        }
                        else {
                            // 既存のテキストの(!--editable--)を削除する
                            if (currentText.Contains(searchText)) {
                                currentText = currentText.Replace(searchText, "");
                            }
                        }


                        // 既存のテキストに新しいメッセージを追加する
                        string newText = (currentText + Environment.NewLine + string.Join(Environment.NewLine, insertText)).Trim() + Environment.NewLine + Environment.NewLine;

                        // 指定されたIDに対してデータを更新する
                        using (var command = new SQLiteCommand("UPDATE chatlog SET date=@date, title=@title, json=@json, text=@text, category=@category, lastprompt=@lastprompt, jsonprev=@jsonprev WHERE id=@id", connection)) {
                            await Task.Run(() => command.Parameters.AddWithValue("@date", resDate));
                            await Task.Run(() => command.Parameters.AddWithValue("@title", titleText));
                            await Task.Run(() => command.Parameters.AddWithValue("@json", jsonConversationHistory));
                            await Task.Run(() => command.Parameters.AddWithValue("@text", newText));
                            await Task.Run(() => command.Parameters.AddWithValue("@category", categoryText));
                            await Task.Run(() => command.Parameters.AddWithValue("@lastprompt", promptTextForSave));
                            await Task.Run(() => command.Parameters.AddWithValue("@jsonprev", jsonLastConversationHistory));
                            await Task.Run(() => command.Parameters.AddWithValue("@id", lastRowId));
                            await command.ExecuteNonQueryAsync();
                        }
                    }
                    else {
                        // logテーブルにデータをインサートする
                        using (var command = new SQLiteCommand("INSERT INTO chatlog(date, title, json, text, category, lastprompt, jsonprev) VALUES (@date, @title, @json, @text, @category, @lastprompt, @jsonprev)", connection)) {
                            await Task.Run(() => command.Parameters.AddWithValue("@date", resDate));
                            await Task.Run(() => command.Parameters.AddWithValue("@title", titleText));
                            await Task.Run(() => command.Parameters.AddWithValue("@json", jsonConversationHistory));
                            await Task.Run(() => command.Parameters.AddWithValue("@text", string.Join(Environment.NewLine, insertText)));
                            await Task.Run(() => command.Parameters.AddWithValue("@category", categoryText));
                            await Task.Run(() => command.Parameters.AddWithValue("@lastprompt", promptTextForSave));
                            await Task.Run(() => command.Parameters.AddWithValue("@jsonprev", jsonLastConversationHistory));
                            await command.ExecuteNonQueryAsync();
                        }

                        // 更新中チャットのIDを取得
                        string sqlLastRowId = "SELECT last_insert_rowid();";
                        using (var command = new SQLiteCommand(sqlLastRowId, connection)) {
                            long insertedId = Convert.ToInt64(command.ExecuteScalar());
                            if (insertedId != VMLocator.ChatViewModel.LastId) {
                                VMLocator.ChatViewModel.LastId = insertedId;
                            }
                        }
                    }
                    // トランザクションをコミットする
                    await Task.Run(() => transaction.Commit());

                    // 成功したら各種変数を更新する
                    VMLocator.ChatViewModel.LastPrompt = promptTextForSave;
                    VMLocator.ChatViewModel.ReEditIsOn = false;
                }
                catch (Exception) {
                    // エラーが発生した場合、トランザクションをロールバックする
                    transaction.Rollback();
                    //var dialog = new ContentDialog() { Title = "Error : " + ex.Message, PrimaryButtonText = "OK" };
                    //await VMLocator.MainViewModel.ContentDialogShowAsync(dialog);
                    throw;
                }
            }
            // インメモリをいったん閉じてまた開く
            await memoryConnection.CloseAsync();
            await DbLoadToMemoryAsync();
        }

        // データベースをチェック--------------------------------------------------------------
        public async Task<bool> CheckTableExists(string selectedFilePath) {
            // テーブル名のリスト
            string[] tableNames = { "phrase", "chatlog", "editorlog", "template" };

            try {
                // データベースに接続
                using (var connection = new SQLiteConnection($"Data Source={selectedFilePath};Version=3;")) {
                    connection.Open();

                    foreach (var tableName in tableNames) {
                        // テーブルが存在するかどうかをチェック
                        string commandText = $"SELECT name FROM sqlite_master WHERE type='table' AND name='{tableName}';";
                        using (var command = new SQLiteCommand(commandText, connection)) {
                            // テーブルが存在しない場合、ExecuteScalar() は null を返す
                            var result = await command.ExecuteScalarAsync();
                            if (result == null) {
                                // テーブルが存在しないため、false を返す
                                return false;
                            }
                        }
                    }

                    // インメモリをいったん閉じる
                    await memoryConnection.CloseAsync();

                    // すべてのテーブルが存在するため、true を返す
                    return true;

                }
            }
            catch (Exception) {
                throw;
            }
        }
    }
}
