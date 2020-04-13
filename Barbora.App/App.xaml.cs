using Barbora.App.Extensions;
using Barbora.App.Services;
using Barbora.App.Helpers;
using Barbora.Core.Clients;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net;
using System.Threading.Tasks;
using System.Windows;

namespace Barbora.App
{
    public partial class App : Application
    {
        public IConfiguration Configuration { get; private set; }
        public IServiceProvider ServiceProvider { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            BuildConfiguration();
            BuildServiceProvider();

            SetupExceptionHandling();

            var barboraApiClient = ServiceProvider.GetRequiredService<IBarboraApiClient>();

            if (barboraApiClient != null)
                barboraApiClient.OnAuthCookieSet += OnAuthCookieSet;

            if (IsLoggedIn(barboraApiClient))
                OpenMainWindow();
            else
                OpenLoginWindow();
        }

        public void OpenLoginWindow()
        {
            var loginWindow = ServiceProvider.GetRequiredService<ILoginWindow>();

            if (loginWindow == null)
                throw new ArgumentNullException("loginWindow");

            loginWindow.Show();
        }

        private void OpenMainWindow()
        {
            var mainWindow = ServiceProvider.GetRequiredService<IMainWindow>();

            if (mainWindow == null)
                throw new ArgumentNullException("mainWindow");

            mainWindow.ShowAfterLoginAsync();
        }

        private bool IsLoggedIn(IBarboraApiClient barboraApiClient)
        {
            var authCookie = AuthCookieHelper.GetAuthCookie();

            if (authCookie != null && authCookie.Expires > DateTime.Now)
            {
                barboraApiClient.LogIn(authCookie);
                return true;
            }

            return false;
        }

        private void OnAuthCookieSet(object sender, Cookie e)
        {
            if (e != null)
                AuthCookieHelper.SetAuthCookie(e);
        }

        private void BuildConfiguration()
        {
            var builder = new ConfigurationBuilder();

            // [???]

            Configuration = builder.Build();
        }

        private void BuildServiceProvider()
        {
            var serviceCollection = new ServiceCollection();

            serviceCollection.AddTransient<IExceptionHandlingService, ExceptionHandlingService>();

            serviceCollection.AddTransient<ILoginWindow, Login>();
            serviceCollection.AddTransient<IMainWindow, Main>();

            serviceCollection.RegisterDefaultContainer();

            ServiceProvider = serviceCollection.BuildServiceProvider();
        }

        private void SetupExceptionHandling()
        {
            var exceptionHandlingService = ServiceProvider.GetRequiredService<IExceptionHandlingService>();

            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
                exceptionHandlingService.LogUnhandledException((Exception)e.ExceptionObject, "AppDomain.CurrentDomain.UnhandledException");

            DispatcherUnhandledException += (s, e) =>
            {
                exceptionHandlingService.LogUnhandledException(e.Exception, "Application.Current.DispatcherUnhandledException");

                e.Handled = true;
            };

            TaskScheduler.UnobservedTaskException += (s, e) =>
            {
                exceptionHandlingService.LogUnhandledException(e.Exception, "TaskScheduler.UnobservedTaskException");

                e.SetObserved();
            };
        }
    }
}