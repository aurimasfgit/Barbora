using Barbora.App.Helpers;
using Barbora.App.Models;
using Barbora.App.Services;
using Barbora.Core.Clients;
using Barbora.Core.Models;
using Barbora.Core.Models.Exceptions;
using Barbora.Core.Services;
using Barbora.Core.Utils;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Security;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

namespace Barbora.App
{
    public interface IMainWindow
    {
        void Show();
        void Close();

        Task ShowAfterLoginAsync();
    }

    public partial class Main : BaseWindow, IMainWindow
    {
        private readonly IBarboraApiClient barboraApiClient;
        private readonly IBarboraNotifyingService barboraNotifyingService;
        private readonly ISoundPlayerService soundPlayerService;

        public Main(IBarboraApiClient barboraApiClient, IBarboraNotifyingService barboraNotifyingService, ISoundPlayerService soundPlayerService)
        {
            this.barboraApiClient = barboraApiClient;
            this.barboraNotifyingService = barboraNotifyingService;
            this.soundPlayerService = soundPlayerService;

            InitializeComponent();
            InitializeNotifyIcon();
            InitializeApiClientEvents();

            InfoBox.SelectedValuePath = "Value";
            InfoBox.DisplayMemberPath = "Text";

            StartBtn.IsEnabled = true;
            StopBtn.IsEnabled = false;
            LogOutBtn.IsEnabled = true;
        }

        #region Notify Icon

        private NotifyIcon notifyIcon;

        private void InitializeNotifyIcon()
        {
            notifyIcon = new NotifyIcon
            {
                Icon = new System.Drawing.Icon(ResourceHelper.GetResourceStream("Icons.barbora.ico"))
            };

            notifyIcon.MouseDoubleClick += new MouseEventHandler(NotifyIconOnDoubleClick);
            notifyIcon.BalloonTipClicked += new EventHandler(NotifyIconOnBalloonTipClick);
        }

        private void NotifyIconOnDoubleClick(object sender, MouseEventArgs e)
        {
            ShowActivatedWindow();
        }

        private void NotifyIconOnBalloonTipClick(object sender, EventArgs args)
        {
            ShowActivatedWindow();
        }

        #endregion

        #region Notifying Service Events

        private void InitializeApiClientEvents()
        {
            barboraNotifyingService.OnAvailableTimeFound += OnAvailableTimeFound;
            barboraNotifyingService.OnJobCompleted += OnJobCompleted;
            barboraNotifyingService.OnException += OnException;
        }

        private void OnAvailableTimeFound(object sender, AvailableTimeInfo info)
        {
            var text = string.Format(Messages.FoundAvailableTime, info.Day, info.Hour);

            PushMessageToLog(text, JsonConvert.SerializeObject(info));

            DebugUtils.WriteLineToDebugConsole(text);
        }

        private void OnJobCompleted(object sender, bool endedWithResults)
        {
            if (endedWithResults) // found available times
            {
                soundPlayerService.Play(SoundsEnum.Default);

                if (notifyIcon != null && notifyIcon.Visible)
                    notifyIcon.ShowBalloonTip(5000, Messages.BalloonFoundAvailable, Messages.Restore, ToolTipIcon.Info);

                DebugUtils.WriteLineToDebugConsole("Job completed founding available times");
            }
            else
                DebugUtils.WriteLineToDebugConsole("Job completed");
        }

        private void OnException(object sender, Exception exc)
        {
            if (exc is SecurityException securityException)
            {
                Dispatcher.Invoke(() =>
                {
                    StopTracking();
                });
            }

            if (exc is FriendlyException friendlyException)
                PushMessageToLog(friendlyException.Message);

            throw exc;
        }

        #endregion

        public async Task ShowAfterLoginAsync()
        {
            await LoadAddresses();

            Show();
        }

        #region Private Methods

        private void ShowActivatedWindow()
        {
            WindowState = WindowState.Normal;
            ShowActivated = true;
        }

        private void PushMessageToLog(string message, string value = null)
        {
            var msg = string.Format("{0} -> {1}", BaseUtils.GetNowDateTime(), message);

            InfoBox.Dispatcher.Invoke(() =>
            {
                InfoBox.Items.Add(new BaseListItem { Value = value, Text = msg });
            });
        }

        #endregion

        #region Events

        protected override void OnStateChanged(EventArgs e)
        {
            base.OnStateChanged(e);

            if (WindowState == WindowState.Minimized)
            {
                ShowInTaskbar = false;

                notifyIcon.Visible = true;
                notifyIcon.ShowBalloonTip(5000, StartBtn.IsEnabled ? Messages.BalloonNotStarted : Messages.BalloonStarted, Messages.Restore, StartBtn.IsEnabled ? ToolTipIcon.Warning : ToolTipIcon.Info);

                notifyIcon.Visible = true;
            }
            else if (WindowState == WindowState.Normal)
            {
                notifyIcon.Visible = false;
                ShowInTaskbar = true;
            }
        }

        private bool DisposeOnClosed { get; set; } = true;

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            if (DisposeOnClosed)
            {
                if (barboraApiClient != null)
                    barboraApiClient.Dispose();

                if (barboraNotifyingService != null)
                    barboraNotifyingService.Dispose();
            }
        }

        #endregion

        private async Task LoadAddresses()
        {
            var addressResponse = await barboraApiClient.GetAddressAsync();

            // TODO: [show all addresses in dropdown and let to choose (one or options: "Visi", "Iki duru", "Stoteles", "I automobili")]

            var homeAddress = addressResponse?.address.Where(x => x.addressType == (int)AddressTypeEnum.Home && x.deliveryMethodType == (int)DeliveryMethodTypeEnum.Home).FirstOrDefault();

            if (homeAddress == null)
                throw new FriendlyException(Messages.HomeDeliveryAddressNotFound);

            // TODO: [handle exceptions of type "FriendlyException"]

            await SetDeliveryAddress(homeAddress.id);
        }

        private async Task SetDeliveryAddress(string addressId)
        {
            var changeDeliveryAddressResponse = await barboraApiClient.ChangeDeliveryAddressAsync(addressId);

            if (changeDeliveryAddressResponse != null)
                PushMessageToLog(string.Format(Messages.DeliveryAddressSetTo, changeDeliveryAddressResponse?.cart?.address));
            else
                throw new FriendlyException(Messages.FailedToChangeDeliveryAddress);
        }

        // TODO: [add dropdown where we can choose JOB repeat time in minutes]

        private void StartTracking()
        {
            StartBtn.IsEnabled = false;
            StopBtn.IsEnabled = true;
            LogOutBtn.IsEnabled = false;

            barboraNotifyingService.SetInterval(60);

            barboraNotifyingService.Start();

            PushMessageToLog(Messages.TrackingStarted);
        }

        private void StopTracking()
        {
            StartBtn.IsEnabled = true;
            StopBtn.IsEnabled = false;
            LogOutBtn.IsEnabled = true;

            barboraNotifyingService.Stop();

            PushMessageToLog(Messages.TrackingStoppped);
        }

        private async Task TryMakeReservation(AvailableTimeInfo info)
        {
            var response = await barboraApiClient.ReserveDeliveryTimeSlotAsync(info.DayId, info.HourId, info.IsExpressDelivery);

            if (response.reservationValidForSeconds != 0)
                PushMessageToLog(string.Format(Messages.ReservationSuccessful, info.Day, info.Hour, response.reservationValidForSeconds / 60));
            else
                PushMessageToLog(response.messages.GetFirstErrorMessage());
        }

        #region Button Click Events

        private void StartBtn_Click(object sender, RoutedEventArgs e)
        {
            StartTracking();
        }

        private void StopBtn_Click(object sender, RoutedEventArgs e)
        {
            StopTracking();
        }

        private async void ReserveBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!(InfoBox.SelectedItem is BaseListItem selectedBaseListItem))
            {
                PushMessageToLog(Messages.RowNotSelected);
                return;
            }

            if (string.IsNullOrEmpty(selectedBaseListItem.Value))
            {
                PushMessageToLog(Messages.SelectedRowMissingInformation);
                return;
            }

            var availableTimeInfo = JsonConvert.DeserializeObject<AvailableTimeInfo>(selectedBaseListItem.Value);

            await TryMakeReservation(availableTimeInfo);
        }

        public void LogOutBtn_Click(object sender, RoutedEventArgs e)
        {
            AuthCookieHelper.RemoveAuthCookie();
            ((App)System.Windows.Application.Current).OpenLoginWindow();
            DisposeOnClosed = false;
            Close();
        }

        #endregion
    }
}