using CsvHelper.Configuration;
using CsvHelper;
using Microsoft.EntityFrameworkCore;
using OpenAI.ObjectModels.ResponseModels;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerraMours.Chat.Ava.Models;

namespace TerraMours.Chat.Ava
{
    /// <summary>
    /// 数据操作类
    /// </summary>
    public class ChatProcess {
        /// <summary>
        /// 创建并初始化数据库
        /// </summary>
        public void CreateDatabase() {
            var context = new ChatDbcontext();
            context.Database.Migrate();
            VMLocator.ChatDbcontext = context;
        }

        /// <summary>
        /// 判断加载的数据库表是否完整
        /// </summary>
        public async Task<bool> CheckTableExists(string selectedFilePath) {
            VMLocator.ChatDbcontext.ChangeConnection(selectedFilePath);
            return (VMLocator.ChatDbcontext.CheckIfTableExists<ChatMessage>() && VMLocator.ChatDbcontext.CheckIfTableExists<ChatList>());
        }

        #region CSV
        /// <summary>
        /// 导入
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public async Task<string> ImportCsvToTableAsync(string fileName) {
            int columnEnd = 2;
            string msg;
                //读取文件
                using var reader = new StreamReader(fileName, System.Text.Encoding.UTF8);
                var config = new CsvConfiguration(CultureInfo.InvariantCulture) {
                    HasHeaderRecord = true,
                    Delimiter = ","
                };
                using var csvReader = new CsvReader(reader, config);
                csvReader.Read();
                var rowData = new List<ChatMessage>();

                while (await csvReader.ReadAsync()) {
                    //todo:完成数据写入
                    rowData.Add(new ChatMessage() { Message = csvReader.GetField(1) });
                }
                if (rowData.Count > 0) {
                    await VMLocator.ChatDbcontext.ChatMessages.AddRangeAsync(rowData);
                }
             msg = $"Successfully imported log. ({rowData.Count} Records)";
            return msg;
            #endregion
        }
        public async Task<string> ExportTableToCsvAsync(string fileName) {
            //todo：完善导出
            string msg;
            using var writer = new StreamWriter(fileName, false, System.Text.Encoding.UTF8);
            var config = new CsvConfiguration(CultureInfo.InvariantCulture) {
                HasHeaderRecord = true,
                Delimiter = ","
            };
            using var csvWriter = new CsvWriter(writer, config);
            var rowData =await VMLocator.ChatDbcontext.ChatMessages.ToListAsync();
            msg = $"Successfully exported log. ({rowData.Count} Records)";
            return msg;
        }
    }
}
