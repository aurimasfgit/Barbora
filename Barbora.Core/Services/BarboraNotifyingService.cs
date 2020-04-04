using Barbora.Core.Clients;
using Barbora.Core.Models;
using Barbora.Core.Models.Exceptions;
using Barbora.Core.Notifiers.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Barbora.Core.Services
{
    public interface IBarboraNotifyingService
    {
        void Start();
        void Stop();

        void SetAvailableTimeNotifier(IAvailableTimeNotifier availableTimeNotifier);
        void SetJobDoneNotifier(IJobDoneNotifier jobDoneNotifier);
        void SetExceptionHandler(IExceptionHandler exceptionHandler);
        void SetInterval(int seconds);

        void Dispose();
    }

    public class BarboraNotifyingService : IBarboraNotifyingService
    {
        private IBarboraApiClient barboraApiClient;

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
        private IAvailableTimeNotifier availableTimeNotifier;
        private IJobDoneNotifier jobDoneNotifier;
        private IExceptionHandler exceptionHandler;

        public void SetAvailableTimeNotifier(IAvailableTimeNotifier availableTimeNotifier)
        {
            if (availableTimeNotifier == null)
                throw new ArgumentNullException("availableTimeNotifier");

            this.availableTimeNotifier = availableTimeNotifier;
        }

        public void SetJobDoneNotifier(IJobDoneNotifier jobDoneNotifier)
        {
            if (jobDoneNotifier == null)
                throw new ArgumentNullException("jobDoneNotifier");

            this.jobDoneNotifier = jobDoneNotifier;
        }

        public void SetExceptionHandler(IExceptionHandler exceptionHandler)
        {
            if (exceptionHandler == null)
                throw new ArgumentNullException("exceptionHandler");

            this.exceptionHandler = exceptionHandler;
        }

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
                                var info = new AvailableTimeInfo();

                                info.DayId = deliveryDay.id;
                                info.Day = deliveryDay.day;

                                info.HourId = hour.id;
                                info.Hour = hour.hour;

                                info.IsExpressDelivery = deliveryDay.isExpressDelivery;

                                info.Price = hour.price;

                                if (availableTimeNotifier != null)
                                    availableTimeNotifier.Notify(info);

                                jobCompletedWithResults = true;
                            }
                        }
                    }
                }

                if (jobDoneNotifier != null)
                    jobDoneNotifier.Notify(jobCompletedWithResults);
            }
            catch (Exception exc)
            {
                if (exceptionHandler != null)
                    exceptionHandler.Handle(exc);
            }
        }

        #endregion
    }
}