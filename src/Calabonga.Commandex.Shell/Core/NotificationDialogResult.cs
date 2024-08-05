﻿using System.Windows;
using Calabonga.Commandex.Engine;

namespace Calabonga.Commandex.Shell.Core;

/// <summary>
/// // Calabonga: Summary required (NotificationDialogResult 2024-07-29 04:07)
/// </summary>
public partial class NotificationDialogResult : DefaultDialogResult
{
    /// <summary>
    /// Default value <see cref="WindowStyle.ToolWindow"/>
    /// </summary>
    public override WindowStyle WindowStyle => WindowStyle.ToolWindow;
}