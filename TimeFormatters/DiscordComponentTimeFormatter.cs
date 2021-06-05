using System;

namespace LiveSplit.TimeFormatters
{
    public class DiscordComponentTimeFormatter : ITimeFormatter
    {
        public TimeAccuracy Accuracy { get; set; }

        public DiscordComponentTimeFormatter(TimeAccuracy accuracy)
        {
            Accuracy = accuracy;
        }
        public string Format(TimeSpan? time)
        {
            var formatter = new RegularTimeFormatter(Accuracy);
            if (time == null)
                return "-";
            else
                return formatter.Format(time);
        }
    }
}