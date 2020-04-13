using Barbora.Core.Clients;
using Barbora.Core.Models.Exceptions;
using System;
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

    public partial class Login : BaseWindow, ILoginWindow
    {
        private readonly IMainWindow mainWindow;
        private readonly IBarboraApiClient barboraApiClient;

        public Login(IMainWindow mainWindow, IBarboraApiClient barboraApiClient)
        {
            this.mainWindow = mainWindow;
            this.barboraApiClient = barboraApiClient;

            InitializeComponent();

            SetRegionsComboBox();
        }

        private void SetRegionsComboBox()
        {
            RegionComboBox.SelectedValuePath = "Id";
            RegionComboBox.DisplayMemberPath = "Value";

            LoadRegions();

            RegionComboBox.SelectedIndex = 0;
        }

        private void LoadRegions()
        {
            RegionComboBox.Items.Add(new { Id = 0, Value = "Numatytasis" });

            // TODO: [fill region combobox with real data]
        }

        private bool CloseMainWindow { get; set; } = true;

        private bool RememberMe
        {
            get
            {
                return RememberMeCheckBox.IsChecked ?? true;
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            if (mainWindow != null && CloseMainWindow)
                mainWindow.Close();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (e.Key == Key.Return)
                LoginBtn_Click(null, null);
        }

        private void SetErrorMessage(string message)
        {
            errorMessage.Text = message;
        }

        private async Task OpenMainWindowAsync()
        {
            CloseMainWindow = false;

            Close();

            await mainWindow.ShowAfterLoginAsync();
        }

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
                    // TODO: [log in to selected region]
                    await barboraApiClient.LogInAsync(emailTextBox.Text, passwordTextBox.Password, RememberMe);
                    await OpenMainWindowAsync();
                }
                catch (FriendlyException exc)
                {
                    SetErrorMessage(exc.Message);
                }
                catch
                {
                    SetErrorMessage("Vidinė serverio klaida");
                    throw;
                }
            }
        }
    }
}