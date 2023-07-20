using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerraMours.Chat.Ava.Models.Class {
    /// <summary>
    /// 扩展方法
    /// </summary>
    public static class ObservableCollectionExtensions {
        public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> source) {
            return new ObservableCollection<T>(source);
        }
    }
}
