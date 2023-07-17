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
        public static SQLiteConnection memoryConnection; // sql连接

        // SQL db初期化--------------------------------------------------------------
        public void CreateDatabase() {
            using var connection = new SQLiteConnection($"Data Source={AppSettings.Instance.DbPath}");
            string sql = "CREATE TABLE phrase (id INTEGER PRIMARY KEY AUTOINCREMENT, name TEXT NOT NULL DEFAULT '', phrase TEXT NOT NULL DEFAULT '');";

            using var command = new SQLiteCommand(sql, connection);
            // phrase创建表格
            connection.Open();
            command.ExecuteNonQuery();

            // phrase索引
            sql = "CREATE INDEX idx_text ON phrase (phrase);";
            command.CommandText = sql;
            command.ExecuteNonQuery();

            // chatlog创建表格
            sql = "CREATE TABLE chatlog (id INTEGER PRIMARY KEY AUTOINCREMENT, date DATE, title TEXT NOT NULL DEFAULT '', json TEXT NOT NULL DEFAULT '', text TEXT NOT NULL DEFAULT '', category TEXT NOT NULL DEFAULT '', lastprompt TEXT NOT NULL DEFAULT '', jsonprev TEXT NOT NULL DEFAULT '');";
            command.CommandText = sql;
            command.ExecuteNonQuery();

            // chatlog索引
            sql = "CREATE INDEX idx_chat_text ON chatlog (text);";
            command.CommandText = sql;
            command.ExecuteNonQuery();

            // editorlog创建表格
            sql = "CREATE TABLE editorlog (id INTEGER PRIMARY KEY AUTOINCREMENT, date DATE, text TEXT NOT NULL DEFAULT '');";
            command.CommandText = sql;
            command.ExecuteNonQuery();

            // editorlog索引
            sql = "CREATE INDEX idx_editor_text ON editorlog (text);";
            command.CommandText = sql;
            command.ExecuteNonQuery();

            // template创建表格
            sql = "CREATE TABLE template (id INTEGER PRIMARY KEY AUTOINCREMENT, title TEXT NOT NULL DEFAULT '', text TEXT NOT NULL DEFAULT '');";
            command.CommandText = sql;
            command.ExecuteNonQuery();

            // template索引
            sql = "CREATE INDEX idx_template_text ON editorlog (text);";
            command.CommandText = sql;
            command.ExecuteNonQuery();
        }

        // 升级数据库聊天日志--------------------------------------------------------------
        public async Task UpdateChatLogDatabaseAsync() {
            try {
                // SQLite连接到数据库
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
            // 关闭内存并重新打开
            await memoryConnection.CloseAsync();
            await DbLoadToMemoryAsync();
        }


        // 将SQL db文件加载到内存中--------------------------------------------------------------
        public async Task DbLoadToMemoryAsync() {
            var fileConnection = new SQLiteConnection($"Data Source={AppSettings.Instance.DbPath}");
            fileConnection.Open();
            // 在内存上创建数据库文件
            memoryConnection = new SQLiteConnection("Data Source=:memory:");
            memoryConnection.Open();
            try {
                // 将SQL db复制到内存
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

        // Phrase Save--------------------------------------------------------------
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

        // Phrase Load--------------------------------------------------------------
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

        // Phrase Load--------------------------------------------------------------
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

        // Phrase Rename--------------------------------------------------------------
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

        // Phrase Update--------------------------------------------------------------
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

        // Phrase Delete--------------------------------------------------------------
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

        // Phrase Import--------------------------------------------------------------
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

        // CSV导入--------------------------------------------------------------
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
                // CSV从文件导入数据
                using var reader = new StreamReader(fileName, System.Text.Encoding.UTF8);
                var config = new CsvConfiguration(CultureInfo.InvariantCulture) {
                    HasHeaderRecord = true,
                    Delimiter = ","
                };
                using var csvReader = new CsvReader(reader, config);
                csvReader.Read(); // 跳过标题行

                using var con = new SQLiteConnection($"Data Source={AppSettings.Instance.DbPath}");
                await con.OpenAsync();
                using (var transaction = con.BeginTransaction()) {
                    try {
                        while (await csvReader.ReadAsync()) // 导入数据行
                        {

                            // 数据取得
                            var rowData = new List<string>();
                            for (int i = 1, loopTo = columnEnd; i <= loopTo; i++) // 从第二排到第八排
                                rowData.Add(csvReader.GetField(i));
                            // 创建插入语句
                            string values = string.Join(", ", Enumerable.Range(0, rowData.Count).Select(i => $"@value{i}"));

                            string insertQuery = $"INSERT INTO {tableName} ({columnNames}) VALUES ({values});";

                            // 将数据插入数据库
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
            // 关闭内存并重新打开
            await memoryConnection.CloseAsync();
            await DbLoadToMemoryAsync();

            return msg;
        }

        // CSV导出--------------------------------------------------------------
        public async Task<string> ExportTableToCsvAsync(string fileName, string tableName = "chatlog") {
            string msg;
            try {
                int processedCount = 0;

                // SELECT 执行查询并检索表中的数据
                var command = new SQLiteCommand($"SELECT * FROM {tableName};", memoryConnection);
                using (SQLiteDataReader reader = (SQLiteDataReader)await command.ExecuteReaderAsync()) {

                    // CSV 创建StreamWriter以写入文件
                    using var writer = new StreamWriter(fileName, false, System.Text.Encoding.UTF8);

                    // CsvWriter 创建并应用设置
                    var config = new CsvConfiguration(CultureInfo.InvariantCulture) {
                        HasHeaderRecord = true,
                        Delimiter = ","
                    };
                    using var csvWriter = new CsvWriter(writer, config);

                    var commandRowCount = new SQLiteCommand($"SELECT COUNT(*) FROM {tableName};", memoryConnection);
                    int rowCount = Convert.ToInt32(commandRowCount.ExecuteScalar());

                    // 写入标题行
                    for (int i = 0, loopTo = reader.FieldCount - 1; i <= loopTo; i++)
                        csvWriter.WriteField(reader.GetName(i));
                    csvWriter.NextRecord();

                    // 写入数据行

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

        // 从数据库搜索聊天日志--------------------------------------------------------------
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

        // 从数据库获取查看聊天日志--------------------------------------------------------------
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

        // 删除聊天日志--------------------------------------------------------------
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
            // 关闭内存并重新打开
            await memoryConnection.CloseAsync();
            await DbLoadToMemoryAsync();
            return;
        }

        // 标题更新--------------------------------------------------------------
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
            // 关闭内存并重新打开
            await memoryConnection.CloseAsync();
            await DbLoadToMemoryAsync();
            VMLocator.DataGridViewModel.ChatList = await SearchChatDatabaseAsync();
        }

        // 会话更新--------------------------------------------------------------
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
            // 关闭内存并重新打开
            await memoryConnection.CloseAsync();
            await DbLoadToMemoryAsync();
            VMLocator.DataGridViewModel.ChatList = await SearchChatDatabaseAsync();
        }

        // Web记录导入--------------------------------------------------------------
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
                // 启动事务
                using var transaction = connection.BeginTransaction();
                try {
                    // 在log表中插入数据
                    using (var command = new SQLiteCommand(query, connection)) {
                        await Task.Run(() => command.Parameters.AddWithValue("@date", nowDate));
                        await Task.Run(() => command.Parameters.AddWithValue("@title", webChatTitle));
                        await Task.Run(() => command.Parameters.AddWithValue("@json", jsonConversationHistory));
                        await Task.Run(() => command.Parameters.AddWithValue("@text", webLog));
                        await Task.Run(() => command.Parameters.AddWithValue("@category", chatService));
                        await command.ExecuteNonQueryAsync();
                    }

                    // 提交事务
                    await Task.Run(() => transaction.Commit());
                }
                catch (Exception) {
                    // 如果发生错误，则回滚事务
                    transaction.Rollback();
                    throw;
                }
            }
            // 关闭内存并重新打开
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







        // 清除数据库编辑器日志--------------------------------------------------------------
        public async Task CleanUpEditorLogDatabaseAsync() {
            // SQLite连接到数据库
            using SQLiteConnection connection = new SQLiteConnection($"Data Source={AppSettings.Instance.DbPath}");
            await connection.OpenAsync();

            // 获取表行数
            using (SQLiteCommand command = new SQLiteCommand("SELECT COUNT(*) FROM editorlog", connection)) {
                var rowCount = (long)command.ExecuteScalar();

                
                if (rowCount > 500) {
                    // 删除日期为新的500
                    using (SQLiteCommand deleteCommand = new SQLiteCommand(@"DELETE FROM editorlog WHERE rowid NOT IN ( SELECT rowid FROM editorlog ORDER BY date DESC LIMIT 500 )", connection)) {
                        await deleteCommand.ExecuteNonQueryAsync();
                    }
                }
            }
            await connection.CloseAsync();
        }
    }
}
