using Terminal.Gui;

namespace FO4Down.Windows
{
    internal class SteamAuthCodeDialog : SimpleDialog
    {
        private ApplicationContext ctx;
        private TextField txtAuthCode;
        private Button btnLogin;

        public SteamAuthCodeDialog(ApplicationContext ctx)
            : base()
        {
            this.ctx = ctx;

            Title = "Steam Login - Auth Code";
            Width = Dim.Percent(50);
            Height = 10;

            txtAuthCode = Input(WordWrap(ctx.Request.Arguments?.FirstOrDefault()
                ?? "Please enter your auth code", (int)(ContentSize.Width * .5)));

            txtAuthCode.TextAlignment = TextAlignment.Centered;

            View view = txtAuthCode;

            if (ctx != null && ctx.IsError)
            {
                view = ErrorLbl(ctx.LastErrorMessage, txtAuthCode);
            }

            btnLogin = Btn("OK", view, BtnLoginClicked);
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
