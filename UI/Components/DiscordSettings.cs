using LiveSplit.Model;
using LiveSplit.TimeFormatters;
using System;
using System.Linq;
using System.Windows.Forms;
using System.Xml;

namespace LiveSplit.UI.Components
{
    public partial class DiscordSettings : UserControl
    {
        public string Details { get; set; }
        public string State { get; set; }
        public string largeImageKey { get; set; }
        public string smallImageKey { get; set; }

        // Garbage
        public string EDetails { get; set; }
        public string EState { get; set; }
        public string ElargeImageKey { get; set; }
        public string EsmallImageKey { get; set; }

        public string PDetails { get; set; }
        public string PState { get; set; }
        public string PlargeImageKey { get; set; }
        public string PsmallImageKey { get; set; }

        public string NRDetails { get; set; }
        public string NRState { get; set; }
        public string NRlargeImageKey { get; set; }
        public string NRsmallImageKey { get; set; }
        //

        public ElapsedTimeType DisplayElapsedTimeType { get; set; }
        public bool NRClearActivity { get; set; }
        public bool SubSplitCount { get; set; }
        public LayoutMode Mode { get; set; }
        public LiveSplitState CurrentState { get; set; }
        public string Comparison { get; set; }

        public TimeAccuracy Accuracy { get; set; }
        public bool DropDecimals { get; set; }

        public DiscordSettings()
        {
            InitializeComponent();

            Details = "%game";
            State = "%category";
            largeImageKey = "Attempt %attempts";
            smallImageKey = "%delta In %split";
            DisplayElapsedTimeType = ElapsedTimeType.DisplayAttemptDuration;
            NRClearActivity = false;
            SubSplitCount = false;
            Accuracy = TimeAccuracy.Tenths;
            DropDecimals = true;
            Comparison = "Current Comparison";

            // Garbage
            EDetails = "%inherit";
            EState = "%inherit";
            ElargeImageKey = "%inherit";
            EsmallImageKey = "%inherit";

            PDetails = "%inherit";
            PState = "%inherit";
            PlargeImageKey = "%inherit";
            PsmallImageKey = "%inherit";

            NRDetails = "%inherit";
            NRState = "%inherit";
            NRlargeImageKey = "%inherit";
            NRsmallImageKey = "%inherit";
            //

            largeText.DataBindings.Add("Text", this, "Details");
            smallText.DataBindings.Add("Text", this, "State");
            largeImageText.DataBindings.Add("Text", this, "largeImageKey");
            smallImageText.DataBindings.Add("Text", this, "smallImageKey");

            PlargeText.DataBindings.Add("Text", this, "PDetails");
            PsmallText.DataBindings.Add("Text", this, "PState");
            PlargeImageText.DataBindings.Add("Text", this, "PlargeImageKey");
            PsmallImageText.DataBindings.Add("Text", this, "PsmallImageKey");

            ElargeText.DataBindings.Add("Text", this, "EDetails");
            EsmallText.DataBindings.Add("Text", this, "EState");
            ElargeImageText.DataBindings.Add("Text", this, "ElargeImageKey");
            EsmallImageText.DataBindings.Add("Text", this, "EsmallImageKey");

            NRlargeText.DataBindings.Add("Text", this, "NRDetails");
            NRsmallText.DataBindings.Add("Text", this, "NRState");
            NRlargeImageText.DataBindings.Add("Text", this, "NRlargeImageKey");
            NRsmallImageText.DataBindings.Add("Text", this, "NRsmallImageKey");

            chkClear.DataBindings.Add("Checked", this, "NRClearActivity");
            chkSubSplits.DataBindings.Add("Checked", this, "SubSplitCount");
            combComparison.DataBindings.Add("SelectedItem", this, "Comparison", false, DataSourceUpdateMode.OnPropertyChanged);
            combBoxElapsed.DataBindings.Add("SelectedItem", this, "DisplayElapsedTimeType", false, DataSourceUpdateMode.OnPropertyChanged);
            chkDropDecimals.DataBindings.Add("Checked", this, "DropDecimals", false, DataSourceUpdateMode.OnPropertyChanged);
        }

        void DiscordSettings_Load(object sender, EventArgs e)
        {
            combBoxElapsed.SelectedIndex = (int)DisplayElapsedTimeType;

            combComparison.Items.Clear();
            combComparison.Items.Add("Current Comparison");
            combComparison.Items.AddRange(CurrentState.Run.Comparisons.ToArray());
            if (!combComparison.Items.Contains(Comparison))
                combComparison.Items.Add(Comparison);

            rdoSeconds.Checked = Accuracy == TimeAccuracy.Seconds;
            rdoTenths.Checked = Accuracy == TimeAccuracy.Tenths;
            rdoHundredths.Checked = Accuracy == TimeAccuracy.Hundredths;
        }

        void combBoxElapsed_SelectedIndexChanged(object sender, EventArgs e)
        {
            DisplayElapsedTimeType = (ElapsedTimeType)combBoxElapsed.SelectedIndex;
        }

        void rdoHundredths_CheckedChanged(object sender, EventArgs e)
        {
            UpdateAccuracy();
        }

        void rdoSeconds_CheckedChanged(object sender, EventArgs e)
        {
            UpdateAccuracy();
        }

        void UpdateAccuracy()
        {
            if (rdoSeconds.Checked)
                Accuracy = TimeAccuracy.Seconds;
            else if (rdoTenths.Checked)
                Accuracy = TimeAccuracy.Tenths;
            else
                Accuracy = TimeAccuracy.Hundredths;
        }

        public void SetSettings(XmlNode node)
        {
            var element = (XmlElement)node;

            Version version = SettingsHelper.ParseVersion(element["Version"]);

            if (version >= new Version(1, 6))
            {
                Comparison = SettingsHelper.ParseString(element["Comparison"]);
                SubSplitCount = SettingsHelper.ParseBool(element["SubSplitCount"]);
                DropDecimals = SettingsHelper.ParseBool(element["DropDecimals"]);
                Accuracy = SettingsHelper.ParseEnum<TimeAccuracy>(element["Accuracy"]);
            }
            else
            {
                Comparison = "Current Comparison";
                SubSplitCount = false;
                DropDecimals = true;
                Accuracy = TimeAccuracy.Tenths;
            }
            if (version >= new Version(1, 5))
                DisplayElapsedTimeType = (ElapsedTimeType) SettingsHelper.ParseInt(element["DisplayElapsedTimeType"]);
            else
            {
                bool oldTime = SettingsHelper.ParseBool(element["DisplayElapsedTime"], true);
                DisplayElapsedTimeType = (oldTime ? ElapsedTimeType.DisplayAttemptDuration : ElapsedTimeType.DoNotDisplay);
            }
            Details = SettingsHelper.ParseString(element["Details"]);
            State = SettingsHelper.ParseString(element["State"]);
            largeImageKey = SettingsHelper.ParseString(element["largeImageKey"]);
            smallImageKey = SettingsHelper.ParseString(element["smallImageKey"]);

            EDetails = SettingsHelper.ParseString(element["EDetails"], "%inherit");
            EState = SettingsHelper.ParseString(element["EState"], "%inherit");
            ElargeImageKey = SettingsHelper.ParseString(element["ElargeImageKey"], "%inherit");
            EsmallImageKey = SettingsHelper.ParseString(element["EsmallImageKey"], "%inherit");

            PDetails = SettingsHelper.ParseString(element["PDetails"], "%inherit");
            PState = SettingsHelper.ParseString(element["PState"], "%inherit");
            PlargeImageKey = SettingsHelper.ParseString(element["PlargeImageKey"], "%inherit");
            PsmallImageKey = SettingsHelper.ParseString(element["PsmallImageKey"], "%inherit");

            NRDetails = SettingsHelper.ParseString(element["NRDetails"], "%inherit");
            NRState = SettingsHelper.ParseString(element["NRState"], "%inherit");
            NRlargeImageKey = SettingsHelper.ParseString(element["NRlargeImageKey"], "%inherit");
            NRsmallImageKey = SettingsHelper.ParseString(element["NRsmallImageKey"], "%inherit");

            NRClearActivity = SettingsHelper.ParseBool(element["NRClearActivity"]);
        }

        public XmlNode GetSettings(XmlDocument document)
        {
            var parent = document.CreateElement("Settings");
            CreateSettingsNode(document, parent);
            return parent;
        }

        public int GetSettingsHashCode()
        {
            return CreateSettingsNode(null, null);
        }

        private int CreateSettingsNode(XmlDocument document, XmlElement parent)
        {
            return SettingsHelper.CreateSetting(document, parent, "Version", "1.6") ^
            SettingsHelper.CreateSetting(document, parent, "Details", Details) ^
            SettingsHelper.CreateSetting(document, parent, "State", State) ^
            SettingsHelper.CreateSetting(document, parent, "largeImageKey", largeImageKey) ^
            SettingsHelper.CreateSetting(document, parent, "smallImageKey", smallImageKey) ^
            SettingsHelper.CreateSetting(document, parent, "DisplayElapsedTimeType", (int)DisplayElapsedTimeType) ^

            SettingsHelper.CreateSetting(document, parent, "EDetails", EDetails) ^
            SettingsHelper.CreateSetting(document, parent, "EState", EState) ^
            SettingsHelper.CreateSetting(document, parent, "ElargeImageKey", ElargeImageKey) ^
            SettingsHelper.CreateSetting(document, parent, "EsmallImageKey", EsmallImageKey) ^

            SettingsHelper.CreateSetting(document, parent, "PDetails", PDetails) ^
            SettingsHelper.CreateSetting(document, parent, "PState", PState) ^
            SettingsHelper.CreateSetting(document, parent, "PlargeImageKey", PlargeImageKey) ^
            SettingsHelper.CreateSetting(document, parent, "PsmallImageKey", PsmallImageKey) ^

            SettingsHelper.CreateSetting(document, parent, "NRDetails", NRDetails) ^
            SettingsHelper.CreateSetting(document, parent, "NRState", NRState) ^
            SettingsHelper.CreateSetting(document, parent, "NRlargeImageKey", NRlargeImageKey) ^
            SettingsHelper.CreateSetting(document, parent, "NRsmallImageKey", NRsmallImageKey) ^

            SettingsHelper.CreateSetting(document, parent, "NRClearActivity", NRClearActivity) ^
            SettingsHelper.CreateSetting(document, parent, "SubSplitCount", SubSplitCount) ^

            SettingsHelper.CreateSetting(document, parent, "Accuracy", Accuracy) ^
            SettingsHelper.CreateSetting(document, parent, "DropDecimals" , DropDecimals) ^

            SettingsHelper.CreateSetting(document, parent, "Comparison", Comparison);
        }
    }
}
