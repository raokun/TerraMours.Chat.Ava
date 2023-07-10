using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    }
}
