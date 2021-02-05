using LiveSplit.Model;
using LiveSplit.Options;
using LiveSplit.TimeFormatters;
using LiveSplit.UI;
using System;
using System.Windows.Forms;
using System.Xml;
using System.Text.RegularExpressions;
using System.Web;
using System.IO;

namespace LiveSplit.UI.Components
{
    public partial class DiscordSettings : UserControl
    {
        public string Details { get; set; }
        public string State { get; set; }
        public string largeImageKey { get; set; }
        public string smallImageKey { get; set; }
        public bool DisplayElapsedTime { get; set; }
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

            largeText.DataBindings.Add("Text", this, "Details");
            smallText.DataBindings.Add("Text", this, "State");
            largeImageText.DataBindings.Add("Text", this, "largeImageKey");
            smallImageText.DataBindings.Add("Text", this, "smallImageKey");
            chkElapsed.DataBindings.Add("Checked", this, "DisplayElapsedTime");
        }

        public void SetSettings(XmlNode node)
        {
            var element = (XmlElement)node;
            Details = SettingsHelper.ParseString(element["Details"]);
            State = SettingsHelper.ParseString(element["State"]);
            largeImageKey = SettingsHelper.ParseString(element["largeImageKey"]);
            smallImageKey = SettingsHelper.ParseString(element["smallImageKey"]);
            DisplayElapsedTime = SettingsHelper.ParseBool(element["DisplayElapsedTime"]);
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
            SettingsHelper.CreateSetting(document, parent, "DisplayElapsedTime", DisplayElapsedTime);
        }

        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void label5_Click(object sender, EventArgs e)
        {

        }
    }
}
