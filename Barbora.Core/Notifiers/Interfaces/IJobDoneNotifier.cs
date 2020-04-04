namespace Barbora.Core.Notifiers.Interfaces
{
    public interface IJobDoneNotifier
    {
        public void Notify(bool endedWithResults);
    }
}