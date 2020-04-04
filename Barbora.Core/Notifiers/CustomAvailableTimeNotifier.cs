using Barbora.Core.Models;
using Barbora.Core.Notifiers.Interfaces;
using System;

namespace Barbora.Core.Notifiers
{
    public class CustomAvailableTimeNotifier : IAvailableTimeNotifier
    {
        private Action<AvailableTimeInfo> customAvailableTimeNotifyAction;

        public CustomAvailableTimeNotifier(Action<AvailableTimeInfo> customAvailableTimeNotifyAction)
        {
            this.customAvailableTimeNotifyAction = customAvailableTimeNotifyAction;
        }

        public void Notify(AvailableTimeInfo availableTimeInfo)
        {
            customAvailableTimeNotifyAction(availableTimeInfo);
        }
    }
}