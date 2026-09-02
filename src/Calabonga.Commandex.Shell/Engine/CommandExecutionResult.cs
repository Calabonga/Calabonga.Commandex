using Calabonga.Commandex.Engine.Base;
using Calabonga.Commandex.Engine.Extensions;
using System.Text.Json;

namespace Calabonga.Commandex.Shell.Engine;

/// <summary>
/// Immutable snapshot of a command execution outcome.
/// It is captured while the command instance is still alive so that callers
/// can use the result after the command (and its DI-scope) has been disposed.
/// </summary>
public sealed record CommandExecutionResult(
    string TypeName,
    string DisplayName,
    string Version,
    bool IsPushToShellEnabled,
    string? SerializedResult)
{
    /// <summary>
    /// Creates a snapshot from the command before it gets disposed.
    /// </summary>
    /// <param name="command">executed command</param>
    public static CommandExecutionResult FromCommand(ICommandexCommand command)
    {
        var serializedResult = command.IsPushToShellEnabled
            ? JsonSerializer.Serialize(command, JsonSerializerOptionsExt.Cyrillic)
            : null;

        return new CommandExecutionResult(
            command.TypeName,
            command.DisplayName,
            command.Version,
            command.IsPushToShellEnabled,
            serializedResult);
    }
}
