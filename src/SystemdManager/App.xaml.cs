using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

using SystemdManager.Objects;
using SystemdManager.Services;
using SystemdManager.ViewModels;

using AdonisMessageBox = AdonisUI.Controls.MessageBox;
using AdonisMessageBoxButton = AdonisUI.Controls.MessageBoxButton;
using AdonisMessageBoxImage = AdonisUI.Controls.MessageBoxImage;

namespace SystemdManager;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{

    public static JsonSerializerOptions JsonSerializerOptions = new()
    {
        WriteIndented = true
    };

    [DllImport("kernel32")]
    private static extern bool AllocConsole();

    private readonly ApplicationViewModel _applicationView;

    public App()
    {
        _applicationView = ApplicationService.ApplicationView;
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_OnUnhandledException;
    }

    private void CurrentDomain_OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        Current.Dispatcher.Invoke(() =>
        {
            var exc = (Exception) e.ExceptionObject;
            AdonisMessageBox.Show($"Exception: {exc.Message}", "Unhandled Exception", AdonisMessageBoxButton.OK, AdonisMessageBoxImage.Error);
        });
    }

    private async void App_OnStartup(object sender, StartupEventArgs e)
    {
        SetupLogger();
        await Task.Factory.StartNew(async () =>
        {
            Log.Information("Starting application.");
            Log.Information("Loading servers.");

            var serverView = ApplicationService.ApplicationView.ServerView;
            var servers = await serverView.LoadServersAsync();
            Dispatcher.Invoke(() =>
            {
                serverView.Servers = new ObservableCollection<Server>(servers);
            });

            Log.Information("Loaded {ServerCount} servers.",
                servers.Count);
        });
    }

    private void SetupLogger()
    {
#if DEBUG
        AllocConsole();
#endif

        var applicationDataDirectory = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        _applicationView.ApplicationDataPath = Path.Combine(applicationDataDirectory, "SystemdManager");
        Directory.CreateDirectory(_applicationView.ApplicationDataPath);

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("System", LogEventLevel.Warning)
            .Enrich.FromLogContext()
#if DEBUG
            .WriteTo.Console(
                theme: AnsiConsoleTheme.Code,
                outputTemplate: "[{Timestamp:G}] [{Level:u3}] {Message:l}{NewLine}")
#endif
            .WriteTo.File(
                path: Path.Combine(_applicationView.ApplicationDataPath, "logs", "log_.txt"),
                rollingInterval: RollingInterval.Day,
                outputTemplate:
                "[{Timestamp:G}] [{Level:u3}] {Message:l}{NewLine:1}{Properties:1j}{NewLine:1}{Exception:1}")
            .CreateLogger();
    }

    public static Stream GetAppResourceStream(string resourceName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var stream = assembly.GetManifestResourceStream($"{assembly.GetName().Name}.Resources.{resourceName}");
        return stream;
    }

}
