using Anvil.ViewModels;
using Anvil.Views;
using System;
using System.Diagnostics.CodeAnalysis;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ReactiveUI;
using Anvil.Models;
using Anvil.Persistence;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;

namespace Anvil
{
    public class App : Application
    {
        /// <summary>
        /// The application's main logger.
        /// </summary>
        private ILogger Logger { get; set; }

        private IClassicDesktopStyleApplicationLifetime AppLifetime { get; set; }

        /// <summary>
        /// Avalonia configuration, don't remove; also used by visual designer.
        /// </summary>
        /// <returns>The application builder.</returns>
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>().UseReactiveUI().UsePlatformDetect().LogToTrace();

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        /// <summary>
        /// Builds the application with classic desktop lifetime.
        /// </summary>
        /// <param name="args">Startup arguments.</param>
        /// <returns>Application exit code..</returns>
        [SuppressMessage("ReSharper.DPA", "DPA0002: Excessive memory allocations in SOH", MessageId = "type: Avalonia.Rendering.SceneGraph.VisualNode")]
        public static int Main(string[] args)
        {
            System.Globalization.CultureInfo.CurrentCulture = new System.Globalization.CultureInfo("en-US");
            System.Globalization.CultureInfo.CurrentUICulture = new System.Globalization.CultureInfo("en-US");
            System.Globalization.CultureInfo.DefaultThreadCurrentCulture = new System.Globalization.CultureInfo("en-US");
            System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = new System.Globalization.CultureInfo("en-US");

            var builder = BuildAvaloniaApp();
            try
            {

                builder.StartWithClassicDesktopLifetime(args);
            }
            catch (Exception ex)
            {
                Log(ex);
                return -1;
            }
            return 0;
        }

        /// <summary>
        /// Initialize app lifetime and stuff.
        /// </summary>
        public override void OnFrameworkInitializationCompleted()
        {
            if (Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime)
            {
                AppLifetime = desktopLifetime;
            }

            // Get the dependencies from the service provider
            Logger = LoggerFactory.Create(x =>
            {
                x.AddSimpleConsole(o =>
                {
                    o.UseUtcTimestamp = true;
                    o.IncludeScopes = true;
                    o.ColorBehavior = LoggerColorBehavior.Enabled;
                    o.TimestampFormat = "HH:mm:ss ";
                })
                    .SetMinimumLevel(LogLevel.Trace);
            }).CreateLogger<App>();

            var suspension = new AutoSuspendHelper(AppLifetime);
            RxApp.SuspensionHost.CreateNewAppState = () => new ApplicationState();
            RxApp.SuspensionHost.SetupDefaultSuspendResume(new NewtonsoftJsonSuspensionDriver("appstate.json"));
            suspension.OnFrameworkInitializationCompleted();

            var state = RxApp.SuspensionHost.GetAppState<ApplicationState>();

            AppLifetime.MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel(state),
            };

            base.OnFrameworkInitializationCompleted();
        }

        /// <summary>
        /// Log an exception. Whenever this function is executed we don't have access to the logger service yet.
        /// </summary>
        /// <param name="ex">The exception.</param>
        private static void Log(Exception ex)
        {
            Console.WriteLine(ex.Message);
            Console.WriteLine(ex.StackTrace);
            if (ex.InnerException is { })
            {
                Log(ex.InnerException);
            }
        }
    }
}
