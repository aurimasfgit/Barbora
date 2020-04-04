using Barbora.Core.Notifiers.Interfaces;
using System;

namespace Barbora.Core.Notifiers
{
    public class CustomJobDoneNotifier : IJobDoneNotifier
    {
        private Action<bool> customJobDoneNotifyAction;

        public CustomJobDoneNotifier(Action<bool> customJobDoneNotifyAction)
        {
            this.customJobDoneNotifyAction = customJobDoneNotifyAction;
        }

        public void Notify(bool endedWithResults)
        {
            customJobDoneNotifyAction(endedWithResults);
        }
    }
}