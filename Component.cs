using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using LiveSplit.Model;
using LiveSplit.Options;
using Discord;

namespace LiveSplit.UI.Components
{
    public class DiscordIntegration : LogicComponent
    {
        public override string ComponentName => "Discord Rich Presence";

        public Discord.Discord discord;

        public TimerPhase LastRunningState;
        public string LastCategory;
        public string LastGame;

        public DiscordIntegration(LiveSplitState state)
        {
            discord = new Discord.Discord(763054362107838504, (UInt64)Discord.CreateFlags.Default);

            UpdatePresence(state.CurrentPhase, state.Run.CategoryName, state.Run.GameName);

        }

        public override void Dispose()
        {
            discord.Dispose();
        }

        public void UpdatePresence(TimerPhase RunState, string CategoryName, string GameName)
        {
            LastRunningState = RunState;
            LastCategory = CategoryName;
            LastGame = GameName;

            var activityManager = discord.GetActivityManager();

            string RunningState;
            string RunningImage = "gray_square";

            if (RunState == TimerPhase.NotRunning)
                RunningState = "Not Running";
            else if (RunState == TimerPhase.Running)
            {
                RunningState = "Running";
                RunningImage = "green_square";
            }
            else if (RunState == TimerPhase.Paused)
                RunningState = "Paused";
            else
                RunningState = "Ended";


            var activity = new Discord.Activity
            {
                Details = GameName,
                State = CategoryName,
                Assets =
                {
                    LargeImage = "livesplit_icon",
                    LargeText = "LiveSplit",
                    SmallText = RunningState,
                    SmallImage = RunningImage
                }
            };
            activityManager.UpdateActivity(activity, (res) =>
            {
                if (res != Discord.Result.Ok)
                    throw new ResultException(res);
            });
        }

        public override Control GetSettingsControl(LayoutMode mode)
        { return null; }

        public override XmlNode GetSettings(XmlDocument document)
        { return null; }

        public override void SetSettings(XmlNode settings) { }

        public override void Update(IInvalidator invalidator, LiveSplitState state, float width, float height, LayoutMode mode)
        {
            if (state.CurrentPhase != LastRunningState || state.Run.CategoryName != LastCategory || state.Run.GameName != LastGame)
            {
                UpdatePresence(state.CurrentPhase, state.Run.CategoryName, state.Run.GameName);
            }

            discord.RunCallbacks();
        }
    }
}
