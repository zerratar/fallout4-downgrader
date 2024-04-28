using SteamKit2;
using Terminal.Gui;

namespace FO4Down.Windows
{

    internal class SimpleDialog : Dialog
    {
        protected bool result;

        protected Label Lbl(string message, View other)
        {
            var lbl = new Label()
            {
                Height = 1,
                Width = Dim.Fill(2),
                X = 1,
                Y = Pos.Bottom(other) + 1,
                TextAlignment = TextAlignment.Centered,
                Text = message,
            };

            Add(lbl);
            return lbl;
        }

        protected void ErrorLbl(string message, View other)
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
                Y = Pos.Bottom(other) + 1,
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

    internal class SteamLoginDialog : SimpleDialog
    {
        private readonly TextField txtUsername;
        private readonly TextField txtPassword;
        private readonly Button btnLogin;
        private readonly Button btnCancel;
        private StepContext ctx;

        public SteamLoginDialog(StepContext ctx)
            : base()
        {
            this.ctx = ctx;

            Title = "Steam Login";
            Width = Dim.Percent(50);
            Height = 12;

            txtUsername = Input("Username");
            txtPassword = Input("Password", true, txtUsername);
            btnLogin = Btn("Login", txtPassword, BtnLoginClicked);
            btnCancel = Btn("Cancel", txtPassword, BtnCancelClicked);
            btnCancel = Btn("QR", txtPassword, BtnQRClicked);
            btnCancel.X = Pos.Right(btnLogin) + 1;

            if (ctx != null && ctx.IsError)
            {
                ErrorLbl(ctx.LastErrorMessage, txtPassword);
            }
            else
            {
                Lbl("Please enter your Steam user/pass", txtPassword);
            }
        }


        private void BtnQRClicked()
        {
            QR = true;
            result = true;
            RequestStop();
        }
        public string Username => txtUsername.Text;
        public string Password => txtPassword.Text;
        public bool QR { get; private set; }

        private void BtnCancelClicked()
        {
            result = false;
            RequestStop();
        }

        private void BtnLoginClicked()
        {
            result = true;
            RequestStop();
        }
    }

    internal class SteamAuthCodeDialog : SimpleDialog
    {
        private StepContext ctx;
        private TextField txtAuthCode;
        private Button btnLogin;

        public SteamAuthCodeDialog(StepContext ctx)
            : base()
        {
            this.ctx = ctx;

            Title = "Steam Login - Auth Code";
            Width = Dim.Percent(50);
            Height = 7;

            txtAuthCode = Input(WordWrap(ctx.Request.Arguments?.FirstOrDefault()
                ?? "Please enter your auth code", (int)(ContentSize.Width * .5)));

            txtAuthCode.TextAlignment = TextAlignment.Centered;
            btnLogin = Btn("OK", txtAuthCode, BtnLoginClicked);

            if (ctx != null && ctx.IsError)
            {
                ErrorLbl(ctx.LastErrorMessage, txtAuthCode);
            }
        }

        private string WordWrap(string text, int maxWidth)
        {
            if (text.Length < maxWidth)
            {
                return text;
            }

            var words = text.Split(' ');
            var len = 0;
            var str = "";
            for (var i = 0; i < words.Length; i++)
            {
                len += words[i].Length;
                len++; // space
                str += words[i] + " ";
                if (len > maxWidth)
                {
                    str = str.Trim() + "\n";
                    len = 0;
                }
            }
            return str.Trim();
        }

        public string AuthCode => txtAuthCode.Text;

        private void BtnLoginClicked()
        {
            result = true;
            RequestStop();
        }
    }
}
