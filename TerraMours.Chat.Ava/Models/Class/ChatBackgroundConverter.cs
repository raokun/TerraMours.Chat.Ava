using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerraMours.Chat.Ava.Models.Class
{
    /// <summary>
    /// 聊天消息框背景
    /// </summary>
    public class ChatBackgroundConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is string Role)
                switch (Role) {
                    case "User":
                        return new SolidColorBrush(Color.Parse("#b6a5a2"));
                        break;
                    case "Assient":
                    default:
                        return new SolidColorBrush(Color.Parse("#444654"));
                        break;
                }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
