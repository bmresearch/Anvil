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
    /// <summary>
    /// The application entrypoint.
    /// </summary>
    public class App : Application
    {
        /// <summary>
        /// The application's logger.
        /// </summary>
        private ILogger Logger { get; set; }

        /// <summary>
        /// The app lifetime.
        /// </summary>
        private IClassicDesktopStyleApplicationLifetime AppLifetime { get; set; }

        /// <summary>
        /// The avalonia dependency resolver.
        /// </summary>
        private IAvaloniaDependencyResolver DependencyResolver { get; set; }

        /// <summary>
        /// The main window view model.
        /// </summary>
        private MainWindowViewModel MainWindowViewModel { get; set; }

        /// <summary>
        /// The main window.
        /// </summary>
        private MainWindow MainWindow { get; set; }

        /// <summary>
        /// Avalonia configuration, don't remove; also used by visual designer.
        /// </summary>
        /// <returns>The application builder.</returns>
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>().UseReactiveUI().UsePlatformDetect().LogToTrace();

        /// <inheritdoc cref="Initialize"/>
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        /// <summary>
        /// Builds the application with classic desktop lifetime.
        /// </summary>
        /// <param name="args">Startup arguments.</param>
        /// <returns>Application exit code..</returns>
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

        /// <inheritdoc cref="OnFrameworkInitializationCompleted"/>
        public override void OnFrameworkInitializationCompleted()
        {
            if (Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime)
            {
                AppLifetime = desktopLifetime;
            }

            if (AppLifetime == null)
            {
                Log(new Exception("Something went wrong during framework initialization."));
                return;
            }

            if (AvaloniaLocator.Current is IAvaloniaDependencyResolver resolver)
            {
                DependencyResolver = resolver;
            }

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

            MainWindowViewModel = new MainWindowViewModel(DependencyResolver, AppLifetime, state, Logger);

            MainWindow = new MainWindow
            {
                DataContext = MainWindowViewModel,
            };
            AppLifetime.MainWindow = MainWindow;

            // TODO: Add support for the app to remain in the tray
            // Likely will need to rewrite/refactor a bunch of services because of the way the wallets are unlocked/initialized
            // AppLifetime.ShutdownRequested += AppLifetime_ShutdownRequested;

            base.OnFrameworkInitializationCompleted();
        }

        // TODO: Add support for the app to remain in the tray
        // Likely will need to rewrite/refactor a bunch of services because of the way the wallets are unlocked/initialized
        //private void AppLifetime_ShutdownRequested(object? sender, ShutdownRequestedEventArgs e)
        //{
        //    e.Cancel = true;
        //}

        // TODO: Add support for the app to remain in the tray
        // Likely will need to rewrite/refactor a bunch of services because of the way the wallets are unlocked/initialized
        //private void TrayIcon_OnClicked(object? sender, EventArgs e)
        //{
        //    MainWindow = new MainWindow
        //    {
        //        DataContext = MainWindowViewModel,
        //    };
        //    AppLifetime.MainWindow = MainWindow;
        //    MainWindow.Show();
        //}

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
