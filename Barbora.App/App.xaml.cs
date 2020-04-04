using Barbora.App.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;

namespace Barbora.App
{
    public partial class App : Application
    {
        public IConfiguration Configuration { get; private set; }
        public IServiceProvider ServiceProvider { get; private set; }

        public bool IsLoggedIn { get; set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            BuildConfiguration();
            BuildServiceProvider();

            SetupExceptionHandling();

            if (IsLoggedIn)
            {
                var mainWindow = ServiceProvider.GetRequiredService<IMainWindow>();

                if (mainWindow == null)
                    throw new ArgumentNullException("mainWindow");

                mainWindow.Show();
            }
            else
            {
                var loginWindow = ServiceProvider.GetRequiredService<ILoginWindow>();

                if (loginWindow == null)
                    throw new ArgumentNullException("loginWindow");

                loginWindow.Show();
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
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

            serviceCollection.AddTransient<ILoginWindow, Login>();
            serviceCollection.AddTransient<IMainWindow, Main>();

            serviceCollection.RegisterDefaultContainer();

            ServiceProvider = serviceCollection.BuildServiceProvider();
        }

        // TODO: [test exception handling]
        private void SetupExceptionHandling()
        {
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
                LogUnhandledException((Exception)e.ExceptionObject, "AppDomain.CurrentDomain.UnhandledException");

            DispatcherUnhandledException += (s, e) =>
            {
                LogUnhandledException(e.Exception, "Application.Current.DispatcherUnhandledException");

                e.Handled = true;
            };

            TaskScheduler.UnobservedTaskException += (s, e) =>
            {
                LogUnhandledException(e.Exception, "TaskScheduler.UnobservedTaskException");

                e.SetObserved();
            };
        }

        private void LogUnhandledException(Exception exception, string source)
        {
            string message = $"Unhandled exception ({source})";

            try
            {
                System.Reflection.AssemblyName assemblyName = System.Reflection.Assembly.GetExecutingAssembly().GetName();
                message = string.Format("Unhandled exception in {0} v{1}", assemblyName.Name, assemblyName.Version);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            finally
            {
                Debug.WriteLine(exception.Message);
            }
        }
    }
}