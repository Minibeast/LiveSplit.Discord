using System;
using System.Windows.Forms;
using System.Xml;
using LiveSplit.Model;
using Discord;

namespace LiveSplit.UI.Components
{
    public class DiscordIntegration : LogicComponent
    {
        public override string ComponentName => "Discord Rich Presence";

        public Discord.Discord discord;

        public DiscordIntegration(LiveSplitState state)
        {
            discord = new Discord.Discord(763054362107838504, (UInt64)CreateFlags.Default);
            UpdatePresence(state);

        }

        public override void Dispose()
        {
            discord.Dispose();
        }

        public void UpdatePresence(LiveSplitState state)
        {
            TimerPhase RunState = state.CurrentPhase;
            string CategoryName = state.Run.CategoryName;
            string GameName = state.Run.GameName;

            TimeSpan? delta = TimeSpan.Zero;

            var activityManager = discord.GetActivityManager();

            string RunningState;
            string RunningImage = "gray_square";
            string PlusMinus = "";
            string decimalFormat = @"\.f";

            if (RunState == TimerPhase.NotRunning)
                RunningState = "Not Running";
            else if (RunState == TimerPhase.Running)
            {
                if (state.CurrentSplitIndex > 0)
                {
                    delta = LiveSplitStateHelper.GetLastDelta(state, state.CurrentSplitIndex, state.CurrentComparison, state.CurrentTimingMethod);

                    if (delta.Value > TimeSpan.Zero)
                        PlusMinus = "+";
                    else
                        PlusMinus = "-";
                }
                string timestring;
                if (state.CurrentSplitIndex <= 0)
                    timestring = "";
                else if (delta.Value.Minutes == 0)
                    timestring = PlusMinus + delta.Value.ToString(@"ss" + decimalFormat) + " ";
                else
                    timestring = PlusMinus + delta.Value.ToString(@"mm\:ss" + decimalFormat) + " ";

                RunningState = "In " + state.CurrentSplit.Name;
                if (state.CurrentSplitIndex > 0)
                    RunningState = timestring + RunningState;
                RunningImage = (PlusMinus == "+" ? "red_square" : "green_square");
            }
            else if (RunState == TimerPhase.Paused)
                RunningState = "Paused";
            else
            {
                var time = state.CurrentTime[state.CurrentTimingMethod];
                RunningState = "Ended. Final Time: " + time.Value.ToString(@"hh\:mm\:ss");
            }

            DateTime sTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            long StartTime = (long) (state.AttemptStarted - sTime).TotalSeconds;

            if (RunState == TimerPhase.Running) {
                var activity = new Activity
                {
                    Details = GameName,
                    State = CategoryName,
                    Assets =
                    {
                        LargeImage = "livesplit_icon",
                        LargeText = "Attempt " + state.Run.AttemptCount.ToString(),
                        SmallText = RunningState,
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
                    Details = GameName,
                    State = CategoryName,
                    Assets =
                    {
                        LargeImage = "livesplit_icon",
                        LargeText = "Attempt " + state.Run.AttemptCount.ToString(),
                        SmallText = RunningState,
                        SmallImage = RunningImage
                    }
                };
                activityManager.UpdateActivity(activity, (res) =>
                {
                    if (res != Result.Ok)
                        throw new ResultException(res);
                });
            }
        }

        public override Control GetSettingsControl(LayoutMode mode)
        { return null; }

        public override XmlNode GetSettings(XmlDocument document)
        { return null; }

        public override void SetSettings(XmlNode settings) { }

        public override void Update(IInvalidator invalidator, LiveSplitState state, float width, float height, LayoutMode mode)
        {
            UpdatePresence(state);

            discord.RunCallbacks();
        }
    }
}
