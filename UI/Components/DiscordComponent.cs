using System;
using System.Xml;
using System.Linq;
using System.Windows.Forms;
using System.Collections.Generic;
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
        private DiscordComponentDeltaFormatter DeltaFormatter { get; set; }
        private DiscordComponentTimeFormatter TimeFormatter { get; set; }
        private bool Initialized;

        private Dictionary<string, string> ComparisonDict;

        private DiscordSettings Settings { get; set; }

        public DiscordComponent(LiveSplitState state)
        {
            try
            {
                discord = new Discord.Discord(763054362107838504, (UInt64)CreateFlags.Default);
                activityManager = discord.GetActivityManager();
                Initialized = true;
            }
            catch
            {
                MessageBox.Show("Something went wrong when initializing Discord. Make sure the client is open!" + Environment.NewLine + "LiveSplit will continue running as normal.");
                Initialized = false;
            }
            State = state;
            Settings = new DiscordSettings()
            {
                CurrentState = State
            };

            DeltaFormatter = new DiscordComponentDeltaFormatter(Settings.Accuracy, Settings.DropDecimals);
            TimeFormatter = new DiscordComponentTimeFormatter(TimeAccuracy.Seconds);
            state.ComparisonRenamed += state_ComparisonRenamed;

            ComparisonDict = new Dictionary<string, string>()
            {
                { "%delta_pb", "Personal Best" },
                { "%delta_bst", BestSplitTimesComparisonGenerator.ComparisonName },
                { "%delta_best", BestSegmentsComparisonGenerator.ComparisonName },
                { "%delta_avg", AverageSegmentsComparisonGenerator.ComparisonName },
                { "%delta_med", MedianSegmentsComparisonGenerator.ComparisonName },
                { "%delta_worst", WorstSegmentsComparisonGenerator.ComparisonName },
                { "%delta_bal", PercentileComparisonGenerator.ComparisonName },
                { "%delta_latest", LatestRunComparisonGenerator.ComparisonName }
            };
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
            DeltaFormatter.Accuracy = Settings.Accuracy;
            DeltaFormatter.DropDecimals = Settings.DropDecimals;

            string GlobalComparison = Settings.Comparison;
            if (GlobalComparison == "Current Comparison")
                GlobalComparison = state.CurrentComparison;

            TimerPhase RunState = state.CurrentPhase;
            string CategoryName = state.Run.CategoryName;
            string DetailedCategoryName = state.Run.GetExtendedCategoryName();
            string GameName = state.Run.GameName;
            string ShortGameName = state.Run.GameName.GetAbbreviations().Last();

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
            string SplitName = "";

            if (RunState == TimerPhase.Running || RunState == TimerPhase.Paused)
            {
                SplitName = state.CurrentSplit.Name;
                // Want to do more with this, maybe more advanced formatting. For now, simple removal.
                if (Settings.SubSplitCount && SplitName.Length > 0)
                {
                    int bracket1 = SplitName.IndexOf("{");
                    int bracket2 = SplitName.IndexOf("}");
                    if (SplitName.Substring(0, 1) == "-")
                        SplitName = SplitName.Substring(1);

                    if ((bracket1 != -1 && bracket2 != -1) && SplitName.Length > bracket2)
                        SplitName = SplitName.Substring(bracket2 + 1);
                }
            }

            if (RunState != TimerPhase.NotRunning)
            {
                var timestring = "";
                if (state.CurrentSplitIndex > 0)
                    GetDelta(GlobalComparison);

                if (RunState != TimerPhase.Paused && timestring.Length > 0)
                    RunningImage = timestring.Substring(0, 1) == "+" ? "red_square" : "green_square";
            }

            long StartTime = 0;

            if (Settings.DisplayElapsedTimeType == ElapsedTimeType.DisplayAttemptDuration || Settings.DisplayElapsedTimeType == ElapsedTimeType.DisplayADwOffset)
            {
                DateTime sTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

                StartTime = (long)(state.AttemptStarted - sTime).TotalSeconds;

                if (Settings.DisplayElapsedTimeType == ElapsedTimeType.DisplayADwOffset)
                    StartTime -= (long)state.Run.Offset.TotalSeconds;
            }
            else if (Settings.DisplayElapsedTimeType == ElapsedTimeType.DisplayGameTime)
                StartTime = DateTime.UtcNow.Ticks - (long)state.CurrentTime[state.CurrentTimingMethod].Value.TotalSeconds;

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
                            return "Ended. Final Time: " + TimeFormatter.Format(state.CurrentTime[state.CurrentTimingMethod]);
                        else if (RunState == TimerPhase.Paused)
                            return "Paused";
                        else
                        {
                            text = FindDelta(text);
                            text = text.Replace("%split", SplitName);
                        }
                    }
                }
                else
                {
                    text = FindDelta(text);
                    text = text.Replace("%split", SplitName);
                }

                text = text.Replace("%game_short", ShortGameName);
                text = text.Replace("%game", GameName);
                text = text.Replace("%category_detailed", DetailedCategoryName);
                text = text.Replace("%category", CategoryName);
                text = text.Replace("%attempts", state.Run.AttemptCount.ToString());
                text = text.Replace("%comparison", GlobalComparison);
                text = text.Replace("%time", TimeFormatter.Format(state.CurrentTime[state.CurrentTimingMethod]));

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

            string GetDelta(string Comparison)
            {
                int SplitIndex = (RunState == TimerPhase.Ended ? state.CurrentSplitIndex - 1 : state.CurrentSplitIndex);

                delta = LiveSplitStateHelper.GetLastDelta(state, SplitIndex, Comparison, state.CurrentTimingMethod);

                return DeltaFormatter.Format(delta);
            }

            string FindDelta(string SearchText)
            {
                if (SearchText.IndexOf("%delta_") != -1)
                {
                    if (SearchText.IndexOf("%delta_cur") != -1)
                        return SearchText.Replace("%delta_cur", GetDelta(state.CurrentComparison));

                    foreach (KeyValuePair<string, string> deltaCheck in ComparisonDict)
                    {
                        if (SearchText.IndexOf(deltaCheck.Key) != -1)
                            return SearchText.Replace(deltaCheck.Key, GetDelta(deltaCheck.Value));
                    }
                }
                return SearchText.Replace("%delta", GetDelta(GlobalComparison));
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