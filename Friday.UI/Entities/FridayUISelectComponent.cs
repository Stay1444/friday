using DSharpPlus.Entities;

namespace Friday.UI.Entities;

public abstract class FridayUISelectComponent : FridayUIComponent
{
    internal FridayUISelectComponent(FridayUIPage page) : base(page)
    {
    }

    internal abstract Task<bool> OnSelect(DiscordInteraction interaction, string[] values);

}