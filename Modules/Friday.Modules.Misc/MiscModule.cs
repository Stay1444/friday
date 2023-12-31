﻿using DSharpPlus.SlashCommands;
using Friday.Common;

namespace Friday.Modules.Misc;

public class MiscModule : ModuleBase
{
    public override Task OnLoad()
    {
        return Task.CompletedTask;
    }

    public override void RegisterSlashCommands(SlashCommandsExtension extension)
    {
        extension.RegisterCommands<SlashCommands.SlashCommands>();
    }

    public override Task OnUnload()
    {
        return Task.CompletedTask;
    }
}