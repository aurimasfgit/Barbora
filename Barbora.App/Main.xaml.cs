using Barbora.App.Services;
using Barbora.App.Utils;
using Barbora.Core;
using Barbora.Core.Clients;
using Barbora.Core.Models;
using Barbora.Core.Models.Exceptions;
using Barbora.Core.Notifiers;
using Barbora.Core.Services;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

namespace Barbora.App
{
    public interface IMainWindow
    {
        void Show();
        void Close();

        Task InitializeAfterLoginAsync();
    }

    public partial class Main : Window, IMainWindow
    {
        private IBarboraApiClient barboraApiClient;
        private IBarboraNotifyingService barboraNotifyingService;
        private ISoundPlayerService soundPlayerService;

        public Main(IBarboraApiClient barboraApiClient, IBarboraNotifyingService barboraNotifyingService, ISoundPlayerService soundPlayerService)
        {
            this.barboraApiClient = barboraApiClient;
            this.barboraNotifyingService = barboraNotifyingService;
            this.soundPlayerService = soundPlayerService;

            InitializeComponent();

            notifyIcon = new NotifyIcon();
            notifyIcon.Icon = new System.Drawing.Icon(ResourceHelper.GetResourceStream("barbora.ico"));
            notifyIcon.MouseDoubleClick += new MouseEventHandler(NotifyIconOnDoubleClick);
            notifyIcon.BalloonTipClicked += new EventHandler(NotifyIconOnBalloonTipClick);

            StartBtn.IsEnabled = true;
            StopBtn.IsEnabled = false;
        }

        private async Task LoadAddresses()
        {
            var addressResponse = await barboraApiClient.GetAddressAsync();

            // TODO: [show all addresses in dropdown and let to choose (one or options: "Visi", "Iki duru", "Stoteles", "I automobili")]

            var homeAddress = addressResponse?.address.Where(x => x.addressType == (int)AddressTypeEnum.Home && x.deliveryMethodType == (int)DeliveryMethodTypeEnum.Home).FirstOrDefault();

            //homeAddress = null;

            if (homeAddress == null)
                throw new FriendlyException("Nepavyko nustatyti pristatymo adreso. Nueik į www.barbora.lt ir pasitikrink...");

            // TODO: [handle those "FriendlyException" exceptions]

            await SetDeliveryAddress(homeAddress.id);
        }

        private async Task SetDeliveryAddress(string addressId)
        {
            var changeDeliveryAddressResponse = await barboraApiClient.ChangeDeliveryAddressAsync(addressId);

            if (changeDeliveryAddressResponse != null)
                PushMessageToLog(string.Format("Nustatytas pristatymo adresas: {0}", changeDeliveryAddressResponse?.cart?.address));
            else
                throw new FriendlyException("Nepavyko pakeisti pristatymo adreso");
        }

        public async Task InitializeAfterLoginAsync()
        {
            ((App)System.Windows.Application.Current).IsLoggedIn = true;

            await LoadAddresses();

            Show();
        }

        private string balloonTextFoundAvailable = "Rasti galimi pristatymo laikai!";
        private string balloonTextNotStarted = "Pamiršai startuoti programą...";
        private string balloonTextStarted = "Atsiradus laikui - pranešiu ;)";
        private string restoreText = "Double click the icon to restore";

        private string GetNowDateTime()
        {
            return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }

        private NotifyIcon notifyIcon;

        private void ShowActivatedWindow()
        {
            WindowState = WindowState.Normal;
            ShowActivated = true;
        }

        private void NotifyIconOnDoubleClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            ShowActivatedWindow();
        }

        private void NotifyIconOnBalloonTipClick(object sender, EventArgs args)
        {
            ShowActivatedWindow();
        }

        protected override void OnStateChanged(EventArgs e)
        {
            base.OnStateChanged(e);

            if (WindowState == WindowState.Minimized)
            {
                ShowInTaskbar = false;

                notifyIcon.Visible = true;
                notifyIcon.ShowBalloonTip(5000, StartBtn.IsEnabled ? balloonTextNotStarted : balloonTextStarted, restoreText, StartBtn.IsEnabled ? ToolTipIcon.Warning : ToolTipIcon.Info);

                notifyIcon.Visible = true;
            }
            else if (WindowState == WindowState.Normal)
            {
                notifyIcon.Visible = false;
                ShowInTaskbar = true;
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            if (barboraApiClient != null)
                barboraApiClient.Dispose();

            if (barboraNotifyingService != null)
                barboraNotifyingService.Dispose();
        }

        private void PushMessageToLog(string message)
        {
            var msg = string.Format("{0} -> {1}", GetNowDateTime(), message);

            InfoBox.Dispatcher.Invoke(() =>
            {
                InfoBox.Items.Add(msg);
            });
        }

        private void NotifyAboutAvailableTime(AvailableTimeInfo info)
        {
            var text = string.Format("Rastas pristatymo laikas: {0} - {1} val.", info.Day, info.Hour);

            PushMessageToLog(text);

            Debug.WriteLine(string.Format("{0} - {1}", GetNowDateTime(), text));
        }

        private void NotifyAboutCompletedJob(bool endedWithResults)
        {
            var now = GetNowDateTime();

            if (endedWithResults) // found available times
            {
                soundPlayerService.Play(SoundsEnum.Default);

                if (notifyIcon != null && notifyIcon.Visible)
                    notifyIcon.ShowBalloonTip(5000, balloonTextFoundAvailable, restoreText, ToolTipIcon.Info);

                Debug.WriteLine(string.Format("{0} - job completed with results", now));
            } 
            else
                Debug.WriteLine(string.Format("{0} - job completed", now));
        }

        private void HandleException(Exception exc)
        {
            var friendlyException = exc as FriendlyException;

            if (friendlyException != null)
                PushMessageToLog(friendlyException.Message);

            Debug.WriteLine(exc.Message);
        }

        // TODO: [add dropdown where we can choose JOB repeat time in minutes]
        // TODO: [add checkbox to check that we want to try reserve first available time or not]
        // TODO: [functionality to try to make a reservation of available time]

        private void StartBtn_Click(object sender, RoutedEventArgs e)
        {
            StartBtn.IsEnabled = false;
            StopBtn.IsEnabled = true;

            var availableTimeNotifier = new CustomAvailableTimeNotifier(NotifyAboutAvailableTime);
            var jobDoneNotifier = new CustomJobDoneNotifier(NotifyAboutCompletedJob);
            var exceptionHandler = new ExceptionHandler(HandleException);

            barboraNotifyingService.SetAvailableTimeNotifier(availableTimeNotifier);
            barboraNotifyingService.SetJobDoneNotifier(jobDoneNotifier);
            barboraNotifyingService.SetExceptionHandler(exceptionHandler);
            barboraNotifyingService.SetInterval(60);

            barboraNotifyingService.Start();

            PushMessageToLog("Pristatymo laikų sekimas pradėtas");
        }

        private void StopBtn_Click(object sender, RoutedEventArgs e)
        {
            StartBtn.IsEnabled = true;
            StopBtn.IsEnabled = false;

            barboraNotifyingService.Stop();

            PushMessageToLog("Pristatymo laikų sekimas sustabdytas");
        }
    }
}