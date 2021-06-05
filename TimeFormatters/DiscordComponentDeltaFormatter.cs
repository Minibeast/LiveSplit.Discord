using System;

namespace LiveSplit.TimeFormatters
{
    public class DiscordComponentDeltaFormatter : ITimeFormatter
    {
        public TimeAccuracy Accuracy { get; set; }
        public bool DropDecimals { get; set; }

        public DiscordComponentDeltaFormatter(TimeAccuracy accuracy, bool dropDecimals)
        {
            Accuracy = accuracy;
            DropDecimals = dropDecimals;
        }

        public string Format(TimeSpan? time)
        {
            var deltaTime = new DeltaTimeFormatter();
            deltaTime.Accuracy = Accuracy;
            deltaTime.DropDecimals = DropDecimals;
            var formattedTime = deltaTime.Format(time);
            if (time == null)
                return "";
            else
                return formattedTime;
        }
    }
}