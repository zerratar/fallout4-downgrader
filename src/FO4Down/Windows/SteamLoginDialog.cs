using Terminal.Gui;

namespace FO4Down.Windows
{
    internal class SteamLoginDialog : SimpleDialog
    {
        private readonly TextField txtUsername;
        private readonly TextField txtPassword;
        private readonly Button btnLogin;
        private readonly Button btnCancel;
        private DowngradeContext ctx;

        public SteamLoginDialog(DowngradeContext ctx)
            : base()
        {
            this.ctx = ctx;

            Closing += SteamLoginDialog_Closing;

            Title = "Steam Login";
            Width = Dim.Percent(50);
            Height = 12;

            txtUsername = Input("Username");
            txtPassword = Input("Password", true, txtUsername);
            btnLogin = Btn("Login", txtPassword, BtnLoginClicked);
            //btnCancel = Btn("Cancel", txtPassword, BtnCancelClicked);
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

        private void SteamLoginDialog_Closing(object? sender, ToplevelClosingEventArgs e)
        {
            if (!result)
                e.Cancel = true;
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

        //private void BtnCancelClicked()
        //{
        //    result = false;
        //    RequestStop();
        //}

        private void BtnLoginClicked()
        {
            result = true;
            RequestStop();
        }
    }
}
