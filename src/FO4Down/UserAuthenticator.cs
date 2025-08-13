using SteamKit2.Authentication;

namespace FO4Down
{
    public class UserAuthenticator : IAuthenticator
    {

        /// <inheritdoc />
        public async Task<string> GetDeviceCodeAsync(bool previousCodeWasIncorrect)
        {
            if (previousCodeWasIncorrect)
            {
                AppContext.Warn("The previous 2-factor auth code you have provided is incorrect.");
            }

            string? code;

            do
            {
                AppContext.Notify("STEAM GUARD! Please enter your 2-factor auth code from your authenticator app");

                code = await AppContext.RequestAsync<string>("auth_code", "Please enter your 2-factor auth code from your authenticator app");

                if (code == null)
                {
                    break;
                }
                code = code.Trim();
            }
            while (string.IsNullOrEmpty(code));

            return code!;
        }

        /// <inheritdoc />
        public async Task<string> GetEmailCodeAsync(string email, bool previousCodeWasIncorrect)
        {
            if (previousCodeWasIncorrect)
            {
                AppContext.Warn("The previous 2-factor auth code you have provided is incorrect.");
            }

            string? code;

            do
            {
                AppContext.Notify($"STEAM GUARD! Please enter the auth code sent to the email at {email}");

                code = await AppContext.RequestAsync<string>("auth_code", $"Please enter the auth code sent to the email at {email}");

                if (code == null)
                {
                    break;
                }

                code = code.Trim();
            }
            while (string.IsNullOrEmpty(code));

            return code!;
        }

        /// <inheritdoc />
        public Task<bool> AcceptDeviceConfirmationAsync()
        {
            AppContext.Notify("STEAM GUARD! Use the Steam Mobile App to confirm your sign in...");

            return Task.FromResult(true);
        }
    }
}
