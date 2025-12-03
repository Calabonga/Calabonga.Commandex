using Calabonga.Commandex.Engine.Dialogs;
using Calabonga.Commandex.Shell.Engine;
using Calabonga.Commandex.Shell.Infrastructure;
using Calabonga.Commandex.Shell.Infrastructure.Identity;
using Calabonga.Commandex.Shell.Infrastructure.Messaging;
using Calabonga.Commandex.Shell.Models;
using Calabonga.Commandex.Shell.ViewModels;
using Calabonga.Commandex.Shell.Views;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System.Windows;

namespace Calabonga.Commandex.Shell;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public App()
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .WriteTo.File("logs/commandex-.log", rollingInterval: RollingInterval.Day, rollOnFileSizeLimit: true, shared: true)
            .CreateLogger();

        Settings = SettingsFinder.Configure();

        Services = DependencyContainer.ConfigureServices();
    }

    /// <summary>
    /// Gets the current <see cref="App"/> instance in use
    /// </summary>
    public static new App Current => (App)Application.Current;

    /// <summary>
    /// Current Application settings from env-file.
    /// </summary>
    public CurrentAppSettings Settings { get; }

    /// <summary>
    /// Last exception that was occurred in application.
    /// If not null it will show as soon as possible and then will  be reset to null
    /// </summary>
    public Exception? LastException { get; set; }

    /// <summary>
    /// Gets the <see cref="IServiceProvider"/> instance to resolve application services.
    /// </summary>
    internal IServiceProvider Services { get; }

    /// <summary>
    /// Current application user 
    /// </summary>
    internal ApplicationUser? User { get; private set; }

    internal void SetUser(ApplicationUser user)
    {
        User = user;
        CommandexStorage.SetUser(user, Settings);
        WeakReferenceMessenger.Default.Send(new LoginSuccessMessage(User.Name));
    }

    /// <summary>Raises the <see cref="E:System.Windows.Application.Startup" /> event.</summary>
    /// <param name="e">A <see cref="T:System.Windows.StartupEventArgs" /> that contains the event data.</param>
    protected override void OnStartup(StartupEventArgs e)
    {
        Log.Debug("Commandex Executor starting...");

        base.OnStartup(e);

        SetupExceptionHandling();

        Log.Debug("Commandex Executor prepare Views & ViewModels...");
        var shell = Services.GetRequiredService<ShellWindow>();
        var shellViewModel = Services.GetRequiredService<ShellWindowViewModel>();
        shell.DataContext = shellViewModel;

        if (LastException is not null)
        {
            Services.GetRequiredService<IDialogService>().ShowError(LastException.Message);
            LastException = null;
        }

        Log.Debug("Commandex Executor checks user authenticated...");

        CommandexStorage.GetUser(Settings);

        shell.Show();

        Log.Debug("Commandex Executor started.");
    }

    /// <summary>
    ///     Required method to load Application Resources
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Application_Startup(object sender, StartupEventArgs e)
    {
        InitializeComponent();
    }

    private void SetupExceptionHandling()
    {
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
        {
            LogUnhandledException((Exception)e.ExceptionObject, "AppDomain.CurrentDomain.UnhandledException");
        };

        DispatcherUnhandledException += (_, e) =>
        {
            LogUnhandledException(e.Exception, "Application.Current.DispatcherUnhandledException");
            e.Handled = true;
        };

        TaskScheduler.UnobservedTaskException += (_, e) =>
        {
            LogUnhandledException(e.Exception, "TaskScheduler.UnobservedTaskException");
            e.SetObserved();
        };
    }

    private void LogUnhandledException(Exception exception, string source)
    {
        var message = $"Unhandled exception ({source})";
        try
        {
            var assemblyName = System.Reflection.Assembly.GetExecutingAssembly().GetName();
            message = $"Unhandled exception in {assemblyName.Name} v{assemblyName.Version}";
        }
        catch (Exception ex)
        {
            Log.Logger.Error(ex, "Exception in LogUnhandledException");
        }
        finally
        {
            Log.Logger.Error(exception, message);
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        Log.Information("Commandex Executor successfully shutdown");
        Log.CloseAndFlush();
        base.OnExit(e);
    }
}
