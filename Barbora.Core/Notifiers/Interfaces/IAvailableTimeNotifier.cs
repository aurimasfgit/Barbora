using Barbora.Core.Models;

namespace Barbora.Core.Notifiers.Interfaces
{
    public interface IAvailableTimeNotifier
    {
        public void Notify(AvailableTimeInfo availableTimeInfo);
    }
}