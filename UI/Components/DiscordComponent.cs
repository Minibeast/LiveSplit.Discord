using System;
using System.Xml;
using System.Windows.Forms;
using LiveSplit.Model;
using LiveSplit.Model.Comparisons;
using LiveSplit.TimeFormatters;
using Discord;

namespace LiveSplit.UI.Components
{
    public class DiscordComponent : LogicComponent
    {
        public override string ComponentName => "Discord Rich Presence";

        public Discord.Discord discord;
        public ActivityManager activityManager;
        private LiveSplitState State { get; set; }
        private DiscordComponentFormatter Formatter { get; set; }
        private bool Initialized;

        private DiscordSettings Settings { get; set; }

        public DiscordComponent(LiveSplitState state)
        {
            try
            {
                discord = new Discord.Discord(763054362107838504, (UInt64)CreateFlags.Default);
                activityManager = discord.GetActivityManager();
                Initialized = true;
            } catch
            {
                MessageBox.Show("Something went wrong when initializing Discord. Make sure the client is open!" + Environment.NewLine + "LiveSplit will continue running as normal.");
                Initialized = false;
            }
            State = state;
            Settings = new DiscordSettings()
            {
                CurrentState = State
            };

            Formatter = new DiscordComponentFormatter(Settings.Accuracy, Settings.DropDecimals);
            state.ComparisonRenamed += state_ComparisonRenamed;
        }

        void state_ComparisonRenamed(object sender, EventArgs e)
        {
            var args = (RenameEventArgs)e;
            if (Settings.Comparison == args.OldName)
            {
                Settings.Comparison = args.NewName;
                ((LiveSplitState)sender).Layout.HasChanged = true;
            }
        }

        public void UpdatePresence(LiveSplitState state)
        {
            string CurrentComparison = Settings.Comparison;
            if (CurrentComparison == "Current Comparison")
                CurrentComparison = state.CurrentComparison;

            TimerPhase RunState = state.CurrentPhase;
            string CategoryName = state.Run.CategoryName;
            string DetailedCategoryName = state.Run.GetExtendedCategoryName();
            string GameName = state.Run.GameName;

            TimeSpan? delta = TimeSpan.Zero;

            if (RunState == TimerPhase.NotRunning && Settings.NRClearActivity)
            {
                activityManager.ClearActivity((res) =>
                {
                    if (res != Result.Ok)
                        throw new ResultException(res);
                });
                return;
            }

            string RunningImage = "gray_square";
            var timestring = "";
            string SplitName = "";

            if (RunState == TimerPhase.Running || RunState == TimerPhase.Paused)
            {
                SplitName = state.CurrentSplit.Name;
                // Want to do more with this, maybe more advanced formatting. For now, simple removal.
                if (Settings.SubSplitCount)
                {
                    int bracket1 = SplitName.IndexOf("{");
                    int bracket2 = SplitName.IndexOf("}");
                    if (SplitName.Substring(0, 1) == "-")
                        SplitName = SplitName.Substring(1);

                    if (bracket1 != -1 && bracket2 != -1)
                        SplitName = SplitName.Substring(bracket2 + 1);
                }
            }

            if (RunState != TimerPhase.NotRunning)
            {
                if (state.CurrentSplitIndex > 0)
                {
                    int SplitIndex = (RunState == TimerPhase.Ended ? state.CurrentSplitIndex - 1 : state.CurrentSplitIndex);

                    delta = LiveSplitStateHelper.GetLastDelta(state, SplitIndex, CurrentComparison, state.CurrentTimingMethod);
                    timestring = Formatter.Format(delta);
                }
                if (RunState != TimerPhase.Paused && timestring.Length > 0)
                    RunningImage = (timestring.Substring(0, 1) == "+" ? "red_square" : "green_square");
            }

            long StartTime = 0;

            if (Settings.DisplayElapsedTimeType == ElapsedTimeType.DisplayAttemptDuration || Settings.DisplayElapsedTimeType == ElapsedTimeType.DisplayADwOffset)
            {
                DateTime sTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

                StartTime = (long)(state.AttemptStarted - sTime).TotalSeconds;

                if (Settings.DisplayElapsedTimeType == ElapsedTimeType.DisplayADwOffset)
                    StartTime -= (long) state.Run.Offset.TotalSeconds;
            }
            else if (Settings.DisplayElapsedTimeType == ElapsedTimeType.DisplayGameTime)
                StartTime = DateTime.UtcNow.Ticks - (long) state.CurrentTime[state.CurrentTimingMethod].Value.TotalSeconds;

            var activity = new Activity
            {
                Details = CheckText("Details"),
                State = CheckText("State"),
                Assets =
                {
                    LargeImage = "livesplit_icon",
                    LargeText = CheckText("largeImage")
                }
            };
            if (Settings.Comparison != NoneComparisonGenerator.ComparisonName)
            {
                activity.Assets.SmallText = CheckText("smallImage");
                activity.Assets.SmallImage = RunningImage;
            }
            if (RunState == TimerPhase.Running && (int)Settings.DisplayElapsedTimeType >= 1)
                activity.Timestamps.Start = StartTime;

            activityManager.UpdateActivity(activity, (res) =>
            {
                if (res != Result.Ok)
                    throw new ResultException(res);
            });

            string CheckText(string item)
            {
                string text = GetText(item);

                if (text == "%inherit")
                {
                    text = GetText(item, true);

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
                            text = text.Replace("%split", SplitName);
                        }
                    }
                }
                else
                {
                    text = text.Replace("%delta", timestring);
                    text = text.Replace("%split", SplitName);
                }

                text = text.Replace("%game", GameName);
                text = text.Replace("%category_detailed", DetailedCategoryName);
                text = text.Replace("%category", CategoryName);
                text = text.Replace("%attempts", state.Run.AttemptCount.ToString());
                text = text.Replace("%comparison", CurrentComparison);
                var nottime = state.CurrentTime[state.CurrentTimingMethod];
                text = text.Replace("%time", nottime.Value.ToString(@"hh\:mm\:ss"));


                return text;

            }

            string GetText(string item, bool GetRunning = false)
            {
                if (RunState == TimerPhase.Running || GetRunning)
                {
                    if (item == "Details")
                        return Settings.Details;

                    else if (item == "State")
                        return Settings.State;

                    else if (item == "largeImage")
                        return Settings.largeImageKey;

                    else
                        return Settings.smallImageKey;
                }

                else if (RunState == TimerPhase.NotRunning)
                {
                    if (item == "Details")
                        return Settings.NRDetails;

                    else if (item == "State")
                        return Settings.NRState;

                    else if (item == "largeImage")
                        return Settings.NRlargeImageKey;

                    else
                        return Settings.NRsmallImageKey;
                }

                else if (RunState == TimerPhase.Ended)
                {
                    if (item == "Details")
                        return Settings.EDetails;

                    else if (item == "State")
                        return Settings.EState;

                    else if (item == "largeImage")
                        return Settings.ElargeImageKey;

                    else
                        return Settings.EsmallImageKey;
                }

                else 
                {
                    if (item == "Details")
                        return Settings.PDetails;

                    else if (item == "State")
                        return Settings.PState;

                    else if (item == "largeImage")
                        return Settings.PlargeImageKey;

                    else
                        return Settings.PsmallImageKey;
                }
            }
        }

        public override void Update(IInvalidator invalidator, LiveSplitState state, float width, float height, LayoutMode mode)
        {
            if (Initialized)
            {
                discord.RunCallbacks();
                UpdatePresence(state);
            }
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
            if (Initialized)
                discord.Dispose();
        }
    }
}
