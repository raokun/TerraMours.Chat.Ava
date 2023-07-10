using ReactiveUI;
using System;
using System.Threading.Tasks;

namespace TerraMours.Chat.Ava.ViewModels {
    public partial class LoadViewModel : ViewModelBase {
        private double _progress;

        public double Progress {
            get => _progress;
            set => this.RaiseAndSetIfChanged(ref _progress, value);
        }
        public Action ToMainAction { get; set; }
        public LoadViewModel() {
            // 在适当的时机更新进度条的值
            UpdateProgress();
        }

        private async void UpdateProgress() {
            // 模拟登录加载过程
            for (int i = 0; i <= 100; i++) {
                Progress = i;
                await Task.Delay(100); // 延迟一段时间，以模拟加载过程
            }
            ToMainAction?.Invoke();
        }
    }

}
