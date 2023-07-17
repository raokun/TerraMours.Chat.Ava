using Avalonia.Controls.Primitives;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Styling;
using Avalonia;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerraMours.Chat.Ava
{
    public class CustomNumericUpDown : NumericUpDown, IStyleable
    {
        private TextBox _textBox;

        Type IStyleable.StyleKey => typeof(NumericUpDown);

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);

            _textBox = e.NameScope.Find<TextBox>("PART_TextBox");
            if (_textBox != null)
            {
                _textBox.KeyDown += TextBox_KeyDown;
            }

            this.DetachedFromVisualTree += OnDetachedFromVisualTree;
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            var key = e.Key;
            var isCtrl = e.KeyModifiers.HasFlag(KeyModifiers.Control);

            // 允许数字键、小数点、控制键（箭头键、空格等）
            if ((key >= Avalonia.Input.Key.D0 && key <= Avalonia.Input.Key.D9) || (key >= Avalonia.Input.Key.NumPad0 && key <= Avalonia.Input.Key.NumPad9) ||
                key == Avalonia.Input.Key.Decimal || key == Avalonia.Input.Key.OemPeriod || key == Avalonia.Input.Key.Left || key == Avalonia.Input.Key.Right || key == Avalonia.Input.Key.Back ||
                key == Avalonia.Input.Key.Delete || key == Avalonia.Input.Key.Tab || isCtrl)
            {
                // 允许输入
            }
            else
            {
                e.Handled = true;
            }
        }

        private void OnDetachedFromVisualTree(object sender, VisualTreeAttachmentEventArgs e)
        {
            if (_textBox != null)
            {
                _textBox.KeyDown -= TextBox_KeyDown;
            }
            this.DetachedFromVisualTree -= OnDetachedFromVisualTree;
        }
    }
}
