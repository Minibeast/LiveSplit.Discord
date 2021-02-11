using LiveSplit.Model;
using LiveSplit.Options;
using LiveSplit.TimeFormatters;
using LiveSplit.UI;
using System;
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

        public bool DisplayElapsedTime { get; set; }
        public bool NRClearActivity { get; set; }
        public LayoutMode Mode { get; set; }

        protected ITimeFormatter TimeFormatter { get; set; }

        public DiscordSettings()
        {
            InitializeComponent();

            TimeFormatter = new ShortTimeFormatter();

            Details = "%game";
            State = "%category";
            largeImageKey = "Attempt %attempts";
            smallImageKey = "%delta In %split";
            DisplayElapsedTime = true;
            NRClearActivity = false;

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

            chkElapsed.DataBindings.Add("Checked", this, "DisplayElapsedTime");
            chkClear.DataBindings.Add("Checked", this, "NRClearActivity");
        }

        public void SetSettings(XmlNode node)
        {
            var element = (XmlElement)node;
            Details = SettingsHelper.ParseString(element["Details"]);
            State = SettingsHelper.ParseString(element["State"]);
            largeImageKey = SettingsHelper.ParseString(element["largeImageKey"]);
            smallImageKey = SettingsHelper.ParseString(element["smallImageKey"]);

            EDetails = SettingsHelper.ParseString(element["EDetails"]);
            EState = SettingsHelper.ParseString(element["EState"]);
            ElargeImageKey = SettingsHelper.ParseString(element["ElargeImageKey"]);
            EsmallImageKey = SettingsHelper.ParseString(element["EsmallImageKey"]);

            PDetails = SettingsHelper.ParseString(element["PDetails"]);
            PState = SettingsHelper.ParseString(element["PState"]);
            PlargeImageKey = SettingsHelper.ParseString(element["PlargeImageKey"]);
            PsmallImageKey = SettingsHelper.ParseString(element["PsmallImageKey"]);

            NRDetails = SettingsHelper.ParseString(element["NRDetails"]);
            NRState = SettingsHelper.ParseString(element["NRState"]);
            NRlargeImageKey = SettingsHelper.ParseString(element["NRlargeImageKey"]);
            NRsmallImageKey = SettingsHelper.ParseString(element["NRsmallImageKey"]);

            DisplayElapsedTime = SettingsHelper.ParseBool(element["DisplayElapsedTime"]);
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
            return SettingsHelper.CreateSetting(document, parent, "Version", "1.4") ^
            SettingsHelper.CreateSetting(document, parent, "Details", Details) ^
            SettingsHelper.CreateSetting(document, parent, "State", State) ^
            SettingsHelper.CreateSetting(document, parent, "largeImageKey", largeImageKey) ^
            SettingsHelper.CreateSetting(document, parent, "smallImageKey", smallImageKey) ^
            SettingsHelper.CreateSetting(document, parent, "DisplayElapsedTime", DisplayElapsedTime) ^

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

            SettingsHelper.CreateSetting(document, parent, "NRClearActivity", NRClearActivity);
        }
    }
}
