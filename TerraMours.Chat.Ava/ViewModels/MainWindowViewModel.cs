using ReactiveUI;
using System.Collections.Generic;
using System.Windows.Input;

namespace TerraMours.Chat.Ava.ViewModels {
    public class MainWindowViewModel : ViewModelBase {
        public MainWindowViewModel() {
            ApiSettingIsOpened = false;
            ClosingApiSettingsCommand = ReactiveCommand.Create(ClosingApiSettings);
            ResetApiSettingsCommand = ReactiveCommand.Create(ResetApiSettings);
        }
        public ICommand ClosingApiSettingsCommand { get; }
        public ICommand ResetApiSettingsCommand { get; }

        private void ClosingApiSettings() {
            ApiSettingIsOpened = false;
            VMLocator.ChatViewModel.ChatViewIsVisible = true;
        }
        private bool _apiSettingIsOpened;
        public bool ApiSettingIsOpened {
            get => _apiSettingIsOpened;
            set => this.RaiseAndSetIfChanged(ref _apiSettingIsOpened, value);
        }

        #region 系统设置
        private void ResetApiSettings() {
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
            //ApiKey = "";
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
        }
        /// <summary>
        /// 模型选择
        /// </summary>
        public List<string> ModelList { get; } = new List<string>
       {
            "gpt-4",
            "gpt-4-32k",
            "gpt-4-32k-0613",
            "gpt-4-32k-0314",
            "gpt-4-0613",
            "gpt-4-0314",
            "gpt-3.5-turbo",
            "gpt-3.5-turbo-16k",
            "gpt-3.5-turbo-16k-0613",
            "gpt-3.5-turbo-0613",
            "gpt-3.5-turbo-0301"
        };

        private AppSettings _appSettings => AppSettings.Instance;

        #region 参数
        public int ApiMaxTokens {
            get => _appSettings.ApiMaxTokens;
            set {
                if (_appSettings.ApiMaxTokens != value) {
                    _appSettings.ApiMaxTokens = value;
                    this.RaisePropertyChanged(nameof(ApiMaxTokens));
                }
            }
        }

        public double ApiTemperature {
            get => _appSettings.ApiTemperature;
            set {
                if (_appSettings.ApiTemperature != value) {
                    _appSettings.ApiTemperature = value;
                    this.RaisePropertyChanged(nameof(ApiTemperature));
                }
            }
        }

        public double ApiTopP {
            get => _appSettings.ApiTopP;
            set {
                if (_appSettings.ApiTopP != value) {
                    _appSettings.ApiTopP = value;
                    this.RaisePropertyChanged(nameof(ApiTopP));
                }
            }
        }

        public int ApiN {
            get => _appSettings.ApiN;
            set {
                if (_appSettings.ApiN != value) {
                    _appSettings.ApiN = value;
                    this.RaisePropertyChanged(nameof(ApiN));
                }
            }
        }

        public int ApiLogprobs {
            get => _appSettings.ApiLogprobs;
            set {
                if (_appSettings.ApiLogprobs != value) {
                    _appSettings.ApiLogprobs = value;
                    this.RaisePropertyChanged(nameof(ApiLogprobs));
                }
            }
        }

        public double ApiPresencePenalty {
            get => _appSettings.ApiPresencePenalty;
            set {
                if (_appSettings.ApiPresencePenalty != value) {
                    _appSettings.ApiPresencePenalty = value;
                    this.RaisePropertyChanged(nameof(ApiPresencePenalty));
                }
            }
        }

        public double ApiFrequencyPenalty {
            get => _appSettings.ApiFrequencyPenalty;
            set {
                if (_appSettings.ApiFrequencyPenalty != value) {
                    _appSettings.ApiFrequencyPenalty = value;
                    this.RaisePropertyChanged(nameof(ApiFrequencyPenalty));
                }
            }
        }

        public int ApiBestOf {
            get => _appSettings.ApiBestOf;
            set {
                if (_appSettings.ApiBestOf != value) {
                    _appSettings.ApiBestOf = value;
                    this.RaisePropertyChanged(nameof(ApiBestOf));
                }
            }
        }

        public string ApiStop {
            get => _appSettings.ApiStop;
            set {
                if (_appSettings.ApiStop != value) {
                    _appSettings.ApiStop = value;
                    this.RaisePropertyChanged(nameof(ApiStop));
                }
            }
        }

        public string ApiLogitBias {
            get => _appSettings.ApiLogitBias;
            set {
                if (_appSettings.ApiLogitBias != value) {
                    _appSettings.ApiLogitBias = value;
                    this.RaisePropertyChanged(nameof(ApiLogitBias));
                }
            }
        }

        public string ApiModel {
            get => _appSettings.ApiModel;
            set {
                if (_appSettings.ApiModel != value) {
                    _appSettings.ApiModel = value;
                    this.RaisePropertyChanged(nameof(ApiModel));
                }
            }
        }

        public string ApiUrl {
            get => _appSettings.ApiUrl;
            set {
                if (_appSettings.ApiUrl != value) {
                    _appSettings.ApiUrl = value;
                    this.RaisePropertyChanged(nameof(ApiUrl));
                }
            }
        }

        public string ApiKey {
            get => _appSettings.ApiKey;
            set {
                if (_appSettings.ApiKey != value) {
                    _appSettings.ApiKey = value;
                    this.RaisePropertyChanged(nameof(ApiKey));
                }
            }
        }

        public int MaxContentLength {
            get => _appSettings.MaxContentLength;
            set {
                if (_appSettings.MaxContentLength != value) {
                    _appSettings.MaxContentLength = value;
                    this.RaisePropertyChanged(nameof(MaxContentLength));
                }
            }
        }

        public bool ApiMaxTokensIsEnable {
            get => _appSettings.ApiMaxTokensIsEnable;
            set {
                if (_appSettings.ApiMaxTokensIsEnable != value) {
                    _appSettings.ApiMaxTokensIsEnable = value;
                    this.RaisePropertyChanged(nameof(ApiMaxTokensIsEnable));
                }
            }
        }

        public bool ApiTemperatureIsEnable {
            get => _appSettings.ApiTemperatureIsEnable;
            set {
                if (_appSettings.ApiTemperatureIsEnable != value) {
                    _appSettings.ApiTemperatureIsEnable = value;
                    this.RaisePropertyChanged(nameof(ApiTemperatureIsEnable));
                }
            }
        }

        public bool ApiTopPIsEnable {
            get => _appSettings.ApiTopPIsEnable;
            set {
                if (_appSettings.ApiTopPIsEnable != value) {
                    _appSettings.ApiTopPIsEnable = value;
                    this.RaisePropertyChanged(nameof(ApiTopPIsEnable));
                }
            }
        }

        public bool ApiNIsEnable {
            get => _appSettings.ApiNIsEnable;
            set {
                if (_appSettings.ApiNIsEnable != value) {
                    _appSettings.ApiNIsEnable = value;
                    this.RaisePropertyChanged(nameof(ApiNIsEnable));
                }
            }
        }
        public bool ApiLogprobIsEnable {
            get => _appSettings.ApiLogprobIsEnable;
            set {
                if (_appSettings.ApiLogprobIsEnable != value) {
                    _appSettings.ApiLogprobIsEnable = value;
                    this.RaisePropertyChanged(nameof(ApiLogprobIsEnable));
                }
            }
        }
        public bool ApiPresencePenaltyIsEnable {
            get => _appSettings.ApiPresencePenaltyIsEnable;
            set {
                if (_appSettings.ApiPresencePenaltyIsEnable != value) {
                    _appSettings.ApiPresencePenaltyIsEnable = value;
                    this.RaisePropertyChanged(nameof(ApiPresencePenaltyIsEnable));
                }
            }
        }
        public bool ApiFrequencyPenaltyIsEnable {
            get => _appSettings.ApiFrequencyPenaltyIsEnable;
            set {
                if (_appSettings.ApiFrequencyPenaltyIsEnable != value) {
                    _appSettings.ApiFrequencyPenaltyIsEnable = value;
                    this.RaisePropertyChanged(nameof(ApiFrequencyPenaltyIsEnable));
                }
            }
        }
        public bool ApiBestOfIsEnable {
            get => _appSettings.ApiBestOfIsEnable;
            set {
                if (_appSettings.ApiBestOfIsEnable != value) {
                    _appSettings.ApiBestOfIsEnable = value;
                    this.RaisePropertyChanged(nameof(ApiBestOfIsEnable));
                }
            }
        }
        public bool ApiStopIsEnable {
            get => _appSettings.ApiStopIsEnable;
            set {
                if (_appSettings.ApiStopIsEnable != value) {
                    _appSettings.ApiStopIsEnable = value;
                    this.RaisePropertyChanged(nameof(ApiStopIsEnable));
                }
            }
        }
        public bool ApiLogitBiasIsEnable {
            get => _appSettings.ApiLogitBiasIsEnable;
            set {
                if (_appSettings.ApiLogitBiasIsEnable != value) {
                    _appSettings.ApiLogitBiasIsEnable = value;
                    this.RaisePropertyChanged(nameof(ApiLogitBiasIsEnable));
                }
            }
        }
        public bool MaxContentLengthIsEnable {
            get => _appSettings.MaxContentLengthIsEnable;
            set {
                if (_appSettings.MaxContentLengthIsEnable != value) {
                    _appSettings.MaxContentLengthIsEnable = value;
                    this.RaisePropertyChanged(nameof(MaxContentLengthIsEnable));
                }
            }
        }
        #endregion

        #endregion

    }
}