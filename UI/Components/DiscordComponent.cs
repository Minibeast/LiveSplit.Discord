using System;
using System.Xml;
using System.Windows.Forms;
using LiveSplit.Model;
using Discord;

namespace LiveSplit.UI.Components
{
    public class DiscordComponent : LogicComponent
    {
        public override string ComponentName => "Discord Rich Presence";

        public Discord.Discord discord;

        private DiscordSettings Settings { get; set; }
        private LiveSplitState State { get; set; }

        public DiscordComponent(LiveSplitState state)
        {
            discord = new Discord.Discord(763054362107838504, (UInt64)CreateFlags.Default);
            Settings = new DiscordSettings();
            State = state;
        }

        public void UpdatePresence(LiveSplitState state)
        {
            TimerPhase RunState = state.CurrentPhase;
            string CategoryName = state.Run.CategoryName;
            string GameName = state.Run.GameName;

            TimeSpan? delta = TimeSpan.Zero;

            var activityManager = discord.GetActivityManager();

            string RunningImage = "gray_square";
            string PlusMinus = "";
            string decimalFormat = @"\.f";
            string timestring = "";

            if (RunState == TimerPhase.Running)
            {
                if (state.CurrentSplitIndex > 0)
                {
                    delta = LiveSplitStateHelper.GetLastDelta(state, state.CurrentSplitIndex, state.CurrentComparison, state.CurrentTimingMethod);

                    if (delta != null && delta.Value > TimeSpan.Zero)
                        PlusMinus = "+";
                    else
                        PlusMinus = "-";
                }
                if (state.CurrentSplitIndex <= 0)
                    timestring = "";
                else if (delta != null && delta.Value.Minutes == 0)
                    timestring = PlusMinus + delta.Value.ToString(@"ss" + decimalFormat) + " ";
                else if (delta != null)
                    timestring = PlusMinus + delta.Value.ToString(@"mm\:ss" + decimalFormat) + " ";

                RunningImage = (PlusMinus == "+" ? "red_square" : "green_square");
            }

            DateTime sTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            long StartTime = (long)(state.AttemptStarted - sTime).TotalSeconds;

            if (RunState == TimerPhase.Running && Settings.DisplayElapsedTime)
            {
                var activity = new Activity
                {
                    Details = CheckText(Settings.Details),
                    State = CheckText(Settings.State),
                    Assets =
                    {
                        LargeImage = "livesplit_icon",
                        LargeText = CheckText(Settings.largeImageKey),
                        SmallText = CheckText(Settings.smallImageKey),
                        SmallImage = RunningImage
                    },
                    Timestamps =
                    {
                        Start = StartTime
                    }
                };
                activityManager.UpdateActivity(activity, (res) =>
                {
                    if (res != Result.Ok)
                        throw new ResultException(res);
                });
            }
            else
            {
                var activity = new Activity
                {
                    Details = CheckText(Settings.Details),
                    State = CheckText(Settings.State),
                    Assets =
                    {
                        LargeImage = "livesplit_icon",
                        LargeText = CheckText(Settings.largeImageKey),
                        SmallText = CheckText(Settings.smallImageKey),
                        SmallImage = RunningImage
                    }
                };
                activityManager.UpdateActivity(activity, (res) =>
                {
                    if (res != Result.Ok)
                        throw new ResultException(res);
                });
            }

            string CheckText(string text)
            {
                text = text.Replace("%game", GameName);
                text = text.Replace("%category", CategoryName);
                text = text.Replace("%attempts", state.Run.AttemptCount.ToString());
                text = text.Replace("%comparison", state.CurrentComparison);

                if (text.Contains("%delta") || text.Contains("%split"))
                {
                    if (RunState == TimerPhase.NotRunning)
                        return "Not Running";
                    else if (RunState == TimerPhase.Ended)
                    {
                        var time = state.CurrentTime[state.CurrentTimingMethod];
                        return "Ended. Final Time: " + time.Value.ToString(@"hh\:mm\:ss");
                    }
                    else if (RunState == TimerPhase.Paused)
                        return "Paused";
                    else
                    {
                        text = text.Replace("%delta", timestring);
                        text = text.Replace("%split", state.CurrentSplit.Name);
                    }
                }

                return text;

            }
        }

        public override void Update(IInvalidator invalidator, LiveSplitState state, float width, float height, LayoutMode mode)
        {
            UpdatePresence(state);

            discord.RunCallbacks();
        }

        public override void SetSettings(XmlNode settings)
        {
            Settings.SetSettings(settings);
        }

        public override XmlNode GetSettings(XmlDocument document)
        {
            return Settings.GetSettings(document);
        }

        public override Control GetSettingsControl(LayoutMode mode)
        {
            Settings.Mode = mode;
            return Settings;
        }

        public override void Dispose()
        {
            discord.Dispose();
        }
    }
}
