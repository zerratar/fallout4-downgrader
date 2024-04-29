using Terminal.Gui;

namespace FO4Down.Windows
{
    internal class SimpleDialog : Dialog
    {
        protected bool result;

        protected CheckBox Check(string label, View? other = null, Action<CheckBox, bool?>? onValueChanged = null)
        {
            var cb = new CheckBox
            {
                Visible = true,
                X = 1,
                Y = other != null ? Pos.Bottom(other) : 1,
                Width = Dim.Fill(2),
                Height = 1,
                Text = label,
            };
            cb.MouseClick += (s, e) =>
            {
                onValueChanged?.Invoke(cb, cb.Checked);
            };

            cb.KeyDown += (s, e) =>
            {
                if (e.KeyCode == KeyCode.Space)
                {
                    onValueChanged?.Invoke(cb, cb.Checked);
                }
            };
            Add(cb);
            return cb;
        }


        protected ComboBox Combo<T>(
            string label,
            Action<ComboBox, int, T> onSelectionChanged,
            params T[] items)
        {
            return Combo<T>(label, null, onSelectionChanged, items);
        }

        protected ComboBox Combo<T>(
            string label,
            View? other,
            Action<ComboBox, int, T> onSelectionChanged,
            params T[] items)
        {
            var lbl = Lbl(label, other);
            lbl.TextAlignment = TextAlignment.Left;

            var cb = new ComboBox
            {
                X = 1,
                Y = Pos.Bottom(lbl),
                Width = Dim.Fill(2),
                Height = 1,
                Text = label,
            };

            if (items.Length > 0)
            {
                cb.SetSource(items);
                cb.Height = items.Length;
            }
            if (onSelectionChanged != null)
            {
                cb.SelectedItemChanged += (s, e) =>
                {
                    onSelectionChanged(cb, e.Item, (T)e.Value);
                };
            }

            Add(cb);
            return cb;
        }

        protected Label Lbl(string message, View? other = null)
        {
            var lbl = new Label()
            {
                Height = message.Count(x => x == '\n') + 1,
                Width = Dim.Fill(2),
                X = 1,
                Y = other != null ? Pos.Bottom(other) + 1 : 1,
                TextAlignment = TextAlignment.Centered,
                Text = message,
            };

            Add(lbl);
            return lbl;
        }

        protected void ErrorLbl(string message, View? other = null)
        {
            var lbl = Lbl(message, other);
            lbl.ColorScheme = new ColorScheme
            {
                Normal = new Terminal.Gui.Attribute(Color.Red, lbl.ColorScheme.Normal.Background)
            };
        }

        protected Button Btn(string text, View other, Action onInvoke)
        {
            var btn = new Button()
            {
                Text = text,
                X = 1,
                Y = other != null ? Pos.Bottom(other) + 1 : 1,
            };

            btn.MouseClick += (s, e) => onInvoke();
            btn.KeyDown += (s, e) =>
            {
                if (e.KeyCode == KeyCode.Enter)
                {
                    onInvoke();
                }
            };

            AddButton(btn);
            return btn;
        }

        protected TextField Input(string label, bool isPassword = false, View other = null)
        {
            var lbl = new Label()
            {
                Height = label.Count(x => x == '\n') + 1,
                Width = Dim.Fill(2),
                X = 1,
                Y = other != null ? Pos.Bottom(other) + 1 : 1,
                Text = label,
            };
            Add(lbl);

            var txt = new TextField()
            {
                X = 1,
                Secret = isPassword,
                Y = Pos.Bottom(lbl),
                Width = Dim.Fill(2),
            };
            Add(txt);
            return txt;
        }

        public bool ShowDialog()
        {
            Application.Run(this);
            return result;
        }
    }
}
