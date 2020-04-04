using Barbora.App.Services;
using Barbora.Core.Clients;
using Barbora.Core.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Barbora.App.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void RegisterDefaultContainer(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<IBarboraApiClient, BarboraApiClient>();
            serviceCollection.AddSingleton<IBarboraNotifyingService, BarboraNotifyingService>();
            serviceCollection.AddSingleton<ISoundPlayerService, SoundPlayerService>();
        }
    }
}