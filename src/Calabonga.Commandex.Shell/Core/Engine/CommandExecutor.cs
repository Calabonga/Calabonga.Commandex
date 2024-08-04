﻿using Calabonga.Commandex.Engine.Commands;
using Calabonga.Commandex.Engine.Exceptions;
using Calabonga.Commandex.Shell.Models;
using Calabonga.OperationResults;
using Serilog;

namespace Calabonga.Commandex.Shell.Core.Engine;

/// <summary>
/// // Calabonga: Summary required (CommandExecutor 2024-08-03 09:54)
/// </summary>
public sealed class CommandExecutor
{
    private readonly IEnumerable<ICommandexCommand> _commands;
    private readonly ArtifactManager _artifactManager;

    public CommandExecutor(
        IEnumerable<ICommandexCommand> commands,
        ArtifactManager artifactManager)
    {
        _commands = commands;
        _artifactManager = artifactManager;
    }

    /// <summary>
    /// // Calabonga: Summary required (CommandExecutor 2024-08-03 09:54)
    /// </summary>
    /// <param name="commandItem"></param>
    public async Task<Operation<ICommandexCommand, CommandExecuteException>> ExecuteAsync(CommandItem commandItem)
    {
        var command = _commands.FirstOrDefault(x => x.TypeName == commandItem.TypeName);
        if (command is null)
        {
            const string errorMessage = "Command not found in the available command list.";
            Log.Logger.Error(errorMessage);
            return Operation.Error(new CommandExecuteException(errorMessage));
        }

        Log.Logger.Debug("Executing {CommandType}", command.TypeName);

        await _artifactManager.CheckDependenciesReadyAsync(command);

        var operation = command.ExecuteCommand();
        if (operation.Ok)
        {
            return Operation.Result(command);
        }
        return Operation.Error(new CommandExecuteException(operation.Error.Message, operation.Error));


    }
}