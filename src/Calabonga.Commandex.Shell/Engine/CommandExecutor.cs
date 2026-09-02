using Calabonga.Commandex.Engine.Base;
using Calabonga.Commandex.Engine.Exceptions;
using Calabonga.Commandex.Shell.Models;
using Calabonga.Commandex.Shell.Services;
using Calabonga.OperationResults;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Calabonga.Commandex.Shell.Engine;

/// <summary>
/// Command Executor helper
/// </summary>
public sealed class CommandExecutor
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IResultProcessor _resultProcessor;
    private readonly ArtifactService _artifactService;

    public CommandExecutor(
        IServiceScopeFactory scopeFactory,
        IResultProcessor resultProcessor,
        ArtifactService artifactService)
    {
        _scopeFactory = scopeFactory;
        _resultProcessor = resultProcessor;
        _artifactService = artifactService;
    }

    /// <summary>
    /// Fires when everything is prepared for command execution.
    /// </summary>
    public event EventHandler? CommandPreparedSuccess;

    /// <summary>
    /// Fires when everything is prepared for command execution.
    /// </summary>
    public event EventHandler? CommandPreparationFailed;

    /// <summary>
    /// Fires before command be executed and prepare command dependencies is started.
    /// </summary>
    public event EventHandler? CommandPrepareStart;

    /// <summary>
    /// Returns a status of the command executing operation without a result.
    /// Pipeline: Find command -> Prepare -> Execute -> Process result -> Dispose -> Return status.
    /// </summary>
    /// <param name="commandItem"></param>
    /// <returns><see cref="CommandExecutionResult"/> when Success and Error <see cref="ExecuteCommandexCommandException"/> when error occurred.</returns>
    public async Task<Operation<CommandExecutionResult, ExecuteCommandexCommandException>> ExecuteAsync(CommandItem commandItem)
    {
        // A dedicated scope per execution guarantees a fresh command instance every run
        // (command registrations are Scoped/Transient) and disposes it deterministically.
        using var scope = _scopeFactory.CreateScope();

        var command = scope.ServiceProvider
            .GetServices<ICommandexCommand>()
            .FirstOrDefault(x => x.TypeName == commandItem.TypeName);

        if (command is null)
        {
            const string errorMessage = "Command not found in the available commands list.";
            Log.Logger.Error(errorMessage);
            return Operation.Error(new ExecuteCommandexCommandException(errorMessage));
        }

        Log.Logger.Debug("Executing {CommandType}", command.TypeName);

        OnCommandPreparing();

        var checkOperation = await _artifactService.CheckDependenciesReadyAsync(command);
        if (!checkOperation.Ok)
        {
            OnCommandPreparationFailed();
            return Operation.Error(checkOperation.Error);
        }

        OnCommandPrepared();

        // The command instance is owned by the scope above: leaving this method disposes the
        // scope and therefore the command - deterministically, on both success and failure.
        try
        {
            var operation = await command.ExecuteCommandAsync();
            if (!operation.Ok)
            {
                return Operation.Error(new ExecuteCommandexCommandException(operation.Error.Message, operation.Error));
            }

            // The result must be consumed while the command instance is still alive.
            if (command.IsPushToShellEnabled)
            {
                _resultProcessor.ProcessCommand(command);
            }

            return Operation.Result(CommandExecutionResult.FromCommand(command));
        }
        catch (Exception exception)
        {
            Log.Logger.Error(exception, exception.Message);
            return Operation.Error(new ExecuteCommandexCommandException(exception.Message, exception));
        }
    }

    private void OnCommandPrepared()
    {
        CommandPreparedSuccess?.Invoke(this, EventArgs.Empty);
    }

    private void OnCommandPreparing()
    {
        CommandPrepareStart?.Invoke(this, EventArgs.Empty);
    }

    private void OnCommandPreparationFailed()
    {
        CommandPreparationFailed?.Invoke(this, EventArgs.Empty);
    }
}
