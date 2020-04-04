using Barbora.Core.Clients;
using Barbora.Core.Models;
using Microsoft.Extensions.Options;
using System;

namespace Barbora.Core.Services
{
    public interface IBarboraSchedulingService
    {
        void CheckAndNotify();
    }

    public class BarboraSchedulingService : IBarboraSchedulingService
    {
        private IOptions<Credentials> credentials;

        public BarboraSchedulingService(IOptions<Credentials> credentials)
        {
            this.credentials = credentials;
        }

        /// <summary>
        /// This method is for Barbora.Scheduler
        /// </summary>
        public void CheckAndNotify()
        {
            if (credentials == null || credentials.Value == null)
                throw new ArgumentNullException("credentials");

            var email = credentials.Value.Email;
            var password = credentials.Value.Password;
            var rememberMe = credentials.Value.RememberMe;

            using (var barboraApiClient = new BarboraApiClient(email, password, rememberMe))
            {
                // gets deliveries for default address
                var deliveriesResponse = barboraApiClient.GetDeliveriesAsync().Result;

                foreach (var delivery in deliveriesResponse.deliveries)
                {
                    foreach (var deliveryDay in delivery.@params.matrix)
                    {
                        foreach (var hour in deliveryDay.hours)
                        {
                            if (hour.available)
                            {
                                // [send message]
                            }
                        }
                    }
                }
            }
        }
    }
}