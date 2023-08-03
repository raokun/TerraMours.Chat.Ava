using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerraMours.Chat.Ava {
    /// <summary>
    /// 系统配置
    /// </summary>
    public class AppSettings {
        private static AppSettings _instance;
        public static AppSettings Instance {
            get {
                if (_instance == null) {
                    _instance = new AppSettings();
                }
                return _instance;
            }
        }

        //应用程序设置
        public string AppDataPath { get; }
        public string DbPath { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public bool IsMaximized { get; set; }
        public bool EditorMode { get; set; }
        public double EditorFontSize { get; set; }
        public int SyntaxHighlighting { get; set; }
        public string PhrasePreset { get; set; }
        public bool PhraseExpanderMode { get; set; }
        public GridLength EditorHeight1 { get; set; }
        public GridLength EditorHeight2 { get; set; }
        public GridLength EditorHeight3 { get; set; }
        public GridLength EditorHeight4 { get; set; }
        public GridLength EditorHeight5 { get; set; }
        public int SeparatorMode { get; set; }

        // ChatGPT API参数
        public int ApiMaxTokens { get; set; }
        public double ApiTemperature { get; set; }
        public double ApiTopP { get; set; }
        public int ApiN { get; set; }
        public int ApiLogprobs { get; set; }
        public double ApiPresencePenalty { get; set; }
        public double ApiFrequencyPenalty { get; set; }
        public int ApiBestOf { get; set; }
        public string ApiStop { get; set; }
        public string ApiLogitBias { get; set; }
        public string ApiModel { get; set; }
        public string ApiUrl { get; set; }
        public string ApiKey { get; set; }
        public int MaxContentLength { get; set; }

        public bool ApiMaxTokensIsEnable { get; set; }
        public bool ApiTemperatureIsEnable { get; set; }
        public bool ApiTopPIsEnable { get; set; }
        public bool ApiNIsEnable { get; set; }
        public bool ApiLogprobIsEnable { get; set; }
        public bool ApiPresencePenaltyIsEnable { get; set; }
        public bool ApiFrequencyPenaltyIsEnable { get; set; }
        public bool ApiBestOfIsEnable { get; set; }
        public bool ApiStopIsEnable { get; set; }
        public bool ApiLogitBiasIsEnable { get; set; }
        public bool MaxContentLengthIsEnable { get; set; }

        //服务端接口配置
        public string BaseUrl { get; set; }

        // DefaultSetting
        public AppSettings() {
            EditorMode = false;
            EditorFontSize = 15;
            SyntaxHighlighting = 0;
            PhrasePreset = "";
            PhraseExpanderMode = true;
            SeparatorMode = 5;

            EditorHeight1 = new GridLength(0.21, GridUnitType.Star);
            EditorHeight2 = new GridLength(0.30, GridUnitType.Star);
            EditorHeight3 = new GridLength(0.17, GridUnitType.Star);
            EditorHeight4 = new GridLength(0.24, GridUnitType.Star);
            EditorHeight5 = new GridLength(0.08, GridUnitType.Star);

            AppDataPath = GetAppDataDirectory();
            DbPath = Path.Combine(AppDataPath, "log_database.db");

            ApiMaxTokens = 2048;
            ApiTemperature = 1;
            ApiTopP = 1.0;
            ApiN = 1;
            ApiLogprobs = 1;
            ApiPresencePenalty = 0.0;
            ApiFrequencyPenalty = 0.0;
            ApiBestOf = 1;
            ApiStop = "";
            ApiLogitBias = "";
            ApiModel = "gpt-3.5-turbo";
            ApiUrl = "https://api.openai.com/v1/chat/completions";
            ApiKey = null;
            MaxContentLength = 3072;

            ApiMaxTokensIsEnable = false;
            ApiTemperatureIsEnable = false;
            ApiTopPIsEnable = false;
            ApiNIsEnable = false;
            ApiLogprobIsEnable = false;
            ApiPresencePenaltyIsEnable = false;
            ApiFrequencyPenaltyIsEnable = false;
            ApiBestOfIsEnable = false;
            ApiStopIsEnable = false;
            ApiLogitBiasIsEnable = false;
            MaxContentLengthIsEnable = false;

            BaseUrl = "https://api.terramours.site";
        }

        private string GetAppDataDirectory() {
            string appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "TerraMours");
            if (!Directory.Exists(appDataPath)) {
                Directory.CreateDirectory(appDataPath);
            }
            return appDataPath;
        }
    }
}
