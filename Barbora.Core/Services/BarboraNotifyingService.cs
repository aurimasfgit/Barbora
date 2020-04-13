using Barbora.Core.Clients;
using Barbora.Core.Models;
using Barbora.Core.Models.Exceptions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Barbora.Core.Services
{
    public interface IBarboraNotifyingService
    {
        void Start();
        void Stop();

        void SetInterval(int seconds);

        event EventHandler<AvailableTimeInfo> OnAvailableTimeFound;
        event EventHandler<bool> OnJobCompleted;
        event EventHandler<Exception> OnException;

        void Dispose();
    }

    public class BarboraNotifyingService : IBarboraNotifyingService
    {
        private readonly IBarboraApiClient barboraApiClient;

        public BarboraNotifyingService(IBarboraApiClient barboraApiClient)
        {
            this.barboraApiClient = barboraApiClient;
        }

        public void Start()
        {
            var cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;

            cancelWork = () => { cancellationTokenSource.Cancel(); };

            Task.Run(() => DoRepeatableWork(cancellationToken), cancellationToken);
        }

        public void Stop()
        {
            if (cancelWork != null)
                cancelWork.Invoke();
        }

        public void Dispose()
        {
            if (barboraApiClient != null)
                barboraApiClient.Dispose();
        }

        private Action cancelWork;

        private int delayInSeconds = 60;

        public event EventHandler<AvailableTimeInfo> OnAvailableTimeFound;
        public event EventHandler<bool> OnJobCompleted;
        public event EventHandler<Exception> OnException;

        public void SetInterval(int seconds)
        {
            if (seconds < 5)
                throw new FriendlyException("Mažiausias galimas kartojimo intervalas yra 5 sekundės");

            delayInSeconds = seconds;
        }

        #region Private methods

        private async Task DoRepeatableWork(CancellationToken token)
        {
            for (; ; )
            {
                if (token.IsCancellationRequested)
                    break;

                await DoWork();

                await Task.Delay(delayInSeconds * 1000);
            }
        }

        private async Task DoWork()
        {
            var jobCompletedWithResults = false;

            try
            {
                // gets deliveries for current address
                var deliveriesResponse = await barboraApiClient.GetDeliveriesAsync();

                foreach (var delivery in deliveriesResponse.deliveries)
                {
                    foreach (var deliveryDay in delivery.@params.matrix)
                    {
                        foreach (var hour in deliveryDay.hours)
                        {
                            if (hour.available)
                            {
                                var info = new AvailableTimeInfo
                                {
                                    DayId = deliveryDay.id,
                                    Day = deliveryDay.day,

                                    HourId = hour.id,
                                    Hour = hour.hour,

                                    IsExpressDelivery = deliveryDay.isExpressDelivery,

                                    Price = hour.price
                                };

                                OnAvailableTimeFound?.Invoke(this, info);

                                jobCompletedWithResults = true;
                            }
                        }
                    }
                }

                OnJobCompleted?.Invoke(this, jobCompletedWithResults);
            }
            catch (Exception exc)
            {
                if (OnException != null)
                    OnException.Invoke(this, exc);
                else
                    throw exc;
            }
        }

        #endregion
    }
}