using Barbora.Core.Clients;
using Barbora.Core.Models.Exceptions;
using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Barbora.App
{
    public interface ILoginWindow
    {
        void Show();
    }

    public partial class Login : Window, ILoginWindow
    {
        private IMainWindow mainWindow;
        private IBarboraApiClient barboraApiClient;

        public Login(IMainWindow mainWindow, IBarboraApiClient barboraApiClient)
        {
            this.mainWindow = mainWindow;
            this.barboraApiClient = barboraApiClient;

            InitializeComponent();
        }

        private bool CloseMainWindow { get; set; } = true;

        private async Task OpenMainWindowAsync()
        {
            CloseMainWindow = false;

            Close();

            await mainWindow.InitializeAfterLoginAsync();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            if (mainWindow != null && CloseMainWindow)
                mainWindow.Close();
        }

        protected override void OnKeyDown(System.Windows.Input.KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (e.Key == Key.Return)
                LoginBtn_Click(null, null);
        }

        private void SetErrorMessage(string message)
        {
            errorMessage.Text = message;
        }

        // TODO: [add region (Vilnius, Kaunas, Alytus, ...) dropdown]

        private async void LoginBtn_Click(object sender, RoutedEventArgs e)
        {
            if (emailTextBox.Text.Length == 0)
            {
                SetErrorMessage("Neįvestas el. pašto adresas");
                emailTextBox.Focus();
            }
            else if (!Regex.IsMatch(emailTextBox.Text, @"^[a-zA-Z][\w\.-]*[a-zA-Z0-9]@[a-zA-Z0-9][\w\.-]*[a-zA-Z0-9]\.[a-zA-Z][a-zA-Z\.]*[a-zA-Z]$"))
            {
                SetErrorMessage("Įvestas netinkamas el. pašto adresas");
                emailTextBox.Select(0, emailTextBox.Text.Length);
                emailTextBox.Focus();
            }
            else if (passwordTextBox.Password.Length == 0)
            {
                SetErrorMessage("Neįvestas slaptažodis");
                passwordTextBox.Focus();
            }
            else
            {
                try
                {
                    // TODO: [add "remember me" checkbox]
                    await barboraApiClient.LogInAsync(emailTextBox.Text, passwordTextBox.Password, true);

                    await OpenMainWindowAsync();
                }
                catch (FriendlyException exc)
                {
                    SetErrorMessage(exc.Message);
                }
                catch (Exception exc)
                {
                    // TODO: [LOG EXCEPTION]

                    SetErrorMessage("Vidinė serverio klaida");

                    Debug.WriteLine(exc?.Message);
                }
            }
        }
    }
}