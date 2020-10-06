using System;
using LiveSplit.Model;
using LiveSplit.UI.Components;

[assembly: ComponentFactory(typeof(Factory))]

namespace LiveSplit.UI.Components
{
    public class Factory : IComponentFactory
    {
        public string ComponentName => "Discord Rich Presence";
        public string Description => "Discord Rich Presence Integration";
        public ComponentCategory Category => ComponentCategory.Other;
        public Version Version => Version.Parse("1.8.0");

        public string UpdateName => ComponentName;
        public string UpdateURL => "http://livesplit.org/update/";
        public string XMLURL => "http://livesplit.org/update/Components/update.LiveSplit.ScriptableAutoSplit.xml";

        public IComponent Create(LiveSplitState state) => new DiscordIntegration(state);
    }
}
