using LiveSplit.Model;
using LiveSplit.UI.Components;
using System;

[assembly: ComponentFactory(typeof(DiscordFactory))]

namespace LiveSplit.UI.Components
{
    public class DiscordFactory : IComponentFactory
    {
        public string ComponentName => "Discord Rich Presence";

        public string Description => "Discord Rich Presence Integration. (Made by Minibeast)";

        public ComponentCategory Category => ComponentCategory.Other;

        public IComponent Create(LiveSplitState state) => new DiscordComponent(state);

        public string UpdateName => ComponentName;

        public string XMLURL => "https://minibeast.github.io/files/LiveSplit.Discord/update.LiveSplit.Discord.xml";

        public string UpdateURL => "https://minibeast.github.io/files/";

        public Version Version => Version.Parse("1.5.0");
    }
}