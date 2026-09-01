using Calabonga.Commandex.Engine.Base;
using Calabonga.Commandex.Engine.Dialogs;
using Calabonga.Commandex.Engine.Settings;
using Calabonga.Commandex.Engine.ToastNotifications;
using Calabonga.Commandex.Engine.ToastNotifications.Controls;
using Calabonga.Commandex.Engine.Zones;
using Calabonga.Commandex.Shell.Engine;
using Calabonga.Commandex.Shell.Infrastructure.Messaging;
using Calabonga.Commandex.Shell.Models;
using Calabonga.Commandex.Shell.ViewModels.Dialogs;
using Calabonga.Commandex.Shell.Views.Dialogs;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace Calabonga.Commandex.Shell.ViewModels;

public partial class CommandListViewModel : ZoneViewModelBase,
    IRecipient<SearchTermChangedMessage>
{
    private readonly ICommandService _commandService;
    private readonly CommandExecutor _commandExecutor;
    private readonly IConfigurationFinder _configurationFinder;
    private readonly ILogger<CommandListViewModel> _logger;
    private readonly INotificationManager _notificationManager;
    private readonly IDialogService _dialogService;

    public CommandListViewModel(
        INotificationManager notificationManager,
        IDialogService dialogService,
        IConfigurationFinder configurationFinder,
        CommandExecutor commandExecutor,
        ICommandService commandService,
        IAppSettings appSettings,
        ILogger<CommandListViewModel> logger)
    {
        _notificationManager = notificationManager;
        _dialogService = dialogService;
        _logger = logger;
        _configurationFinder = configurationFinder;
        _commandExecutor = commandExecutor;
        _commandService = commandService;

        ApplyViewTemplate(appSettings);

        Subscriptions();
    }

    #region properties

    public bool IsNotAuthenticated => !IsAuthenticated;

    public bool CanExecuteAction => SelectedCommand is not null && SelectedCommand!.TypeName != nameof(CommandGroup);

    #region property ListViewName

    /// <summary>
    /// ListView Selected view
    /// </summary>
    [ObservableProperty]
    private string _listViewName;

    #endregion

    #region property SelectedCommand

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ExecuteActionCommand))]
    [NotifyCanExecuteChangedFor(nameof(OpenCommandConfigurationCommand))]
    [NotifyPropertyChangedFor(nameof(CanExecuteAction))]
    private CommandItem? _selectedCommand;

    #endregion

    #region property IsAuthenticated

    /// <summary>
    /// Indicated that user login into OAuth2.0 successful
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotAuthenticated))]
    private bool _isAuthenticated;

    #endregion

    #region property IsFindEnabled
    [ObservableProperty]
    private bool _isFindEnabled = App.Current.Settings.ShowSearchPanelOnStartup;
    #endregion

    #region property SearchTerm
    [ObservableProperty]
    private string? _searchTerm;
    #endregion

    #region property CommandItems

    [ObservableProperty]
    private ObservableCollection<CommandItem> _commandItems = new();
    #endregion

    #region property CommandItemDataTemplate

    [ObservableProperty]
    private DataTemplate _commandItemDataTemplate;
    #endregion

    #region property Username

    /// <summary>
    /// UserName logged in
    /// </summary>
    [ObservableProperty]
    private string _username;

    #endregion

    #endregion

    #region commands

    #region command ToggleSearchBarVisibilityCommand
    [RelayCommand]
    private void ToggleSearchBarVisibility()
    {
        IsFindEnabled = !IsFindEnabled;
    }

    #endregion

    #region command ExecuteActionCommand

    [RelayCommand(CanExecute = nameof(CanExecuteAction))]
    private async Task ExecuteActionAsync()
    {
        try
        {
            var operation = await _commandExecutor.ExecuteAsync(SelectedCommand!);
            if (!operation.Ok)
            {
                _logger.LogError(operation.Error, $"[COMMANDEX] {operation.Error.Message}");
                _notificationManager.Show(NotificationManager.CreateErrorToast(operation.Error.Message), nameof(NotificationZone));
                return;
            }

            var result = operation.Result;
            if (!result.IsPushToShellEnabled)
            {
                return;
            }

            _notificationManager.Show(NotificationManager.CreateSuccessToast($"Command executed successfully. {result.SerializedResult} More results in the logs-file."), nameof(NotificationZone));
        }
        finally
        {
            IsBusy = false;
        }
    }
    #endregion

    #region command OpenCommandConfigurationCommand

    [RelayCommand(CanExecute = nameof(CanExecuteAction))]
    private void OpenCommandConfiguration()
    {
        _configurationFinder.OpenOrCreateCommandConfigurationFile(SelectedCommand!.Scope);
    }

    #endregion

    #region command SwitchViewCommand

    [RelayCommand]
    private void SwitchView(CommandViewType viewType)
    {
        ListViewName = viewType.ToString();
        CommandItems = new ObservableCollection<CommandItem>(_commandService.GetCommands(viewType, SearchTerm));
        var result = App.Current.TryFindResource(CurrentAppSettings.GetViewResourceName(viewType.ToString()));
        CommandItemDataTemplate = (DataTemplate)result;
    }
    #endregion

    #region command OpenLogsFolderCommand

    [RelayCommand]
    private void OpenLogsFolder()
        => _dialogService.ShowConfirmation("Open logs folder?", result =>
        {
            if (result.ConfirmResult != ConfirmationType.Ok)
            {
                return;
            }

            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");

            if (!Path.Exists(path))
            {
                _dialogService.ShowNotification($"Folder not found: {path}");
                return;
            }

            Process.Start(new ProcessStartInfo
            {
                FileName = path,
                UseShellExecute = true,
                Verb = "open"
            });
        }, ConfirmationTypes.OkCancel);

    #endregion

    #region command OpenSiteCommand

    [RelayCommand]
    private void OpenSite(string linkName)
    {
        OpenWebLink(linkName);
    }

    #endregion

    #region command ShowAboutCommand

    [RelayCommand]
    private void ShowAbout() => _dialogService.ShowDialog<AboutDialog, AboutViewModel>();
    #endregion

    #endregion

    #region handlers

    public void Receive(SearchTermChangedMessage message)
    {
        SearchTerm = message.SearchTerm;
        LoadData();
    }

    #endregion

    #region privates

    private void LoadData()
    {
        IsBusy = true;
        var viewType = Enum.Parse<CommandViewType>(ListViewName);
        CommandItems = new ObservableCollection<CommandItem>(_commandService.GetCommands(viewType, SearchTerm));
        IsBusy = false;
    }

    #region Initializations

    private void ApplyViewTemplate(IAppSettings appSettings)
    {
        var view = ((CurrentAppSettings)appSettings).DefaultViewName;
        var result = App.Current.TryFindResource(CurrentAppSettings.GetViewResourceName(view));
        ListViewName = view;
        CommandItemDataTemplate = (DataTemplate)result;
    }

    private void Subscriptions()
    {
        _commandExecutor.CommandPreparedSuccess += (_, _) => { IsBusy = false; };
        _commandExecutor.CommandPrepareStart += (_, _) => { IsBusy = true; };
        _commandExecutor.CommandPreparationFailed += (_, _) => { IsBusy = false; };

        WeakReferenceMessenger.Default.Register<SearchTermChangedMessage>(this);
    }

    private void OpenWebLink(string link)
    {
        var linkUrl = link switch
        {
            "repo_engine" => "https://github.com/Calabonga/Calabonga.Commandex.Engine",
            "repo_commands" => "https://github.com/Calabonga/Calabonga.Commandex.Commands",
            "repo_devshell" => "https://github.com/Calabonga/Calabonga.Commandex.Shell.Develop.Template",
            "repo_shell" => "https://github.com/Calabonga/Calabonga.Commandex.Shell",
            _ or "blog" => "https://www.calabonga.net"
        };

        var processStartInfo = new ProcessStartInfo(linkUrl) { UseShellExecute = true, Verb = "open" };
        Process.Start(processStartInfo);
    }

    #endregion

    #endregion

    #region partials

    partial void OnIsFindEnabledChanged(bool value)
    {
        WeakReferenceMessenger.Default.Send(new ToggleFindVisibilityMessage());
    }

    #endregion

    public override void OnActivate()
    {
        LoadData();
    }
}

