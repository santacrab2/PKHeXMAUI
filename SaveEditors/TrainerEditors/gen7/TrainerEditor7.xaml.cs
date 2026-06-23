
using PKHeX.Core;

namespace PKHeXMAUI;

public partial class TrainerEditor7 : ContentPage
{
    public SAV7 SAV = (SAV7)MainPage.sav;
	public TrainerEditor7()
	{
		InitializeComponent();
        OTNameEntry.Text = SAV.OT;
        GenderPicker.ItemsSource = GameInfo.GenderSymbolUnicode.Take(2).ToArray();
        GenderPicker.SelectedIndex = SAV.Gender;
        TimeOffsetPicker.ItemsSource = GetAlolaTimeList();
        TimeOffsetPicker.ItemDisplayBinding = new Binding("Text");
        var timeA = SAV.GameTime.AlolaTime;
        if (timeA == 0)
            timeA = 24 * 60 * 60; // Patch up any bad times from previous program versions.
        if (timeA == 9_999_999)
            TimeOffsetPicker.IsVisible = false;
        else
            TimeOffsetPicker.SelectedItem = GetAlolaTimeList().FirstOrDefault(z => z.Value == (int)timeA);
        if (TimeOffsetPicker.SelectedItem == null)
            TimeOffsetPicker.IsVisible = false;
        var recordres = RecordLists.RecordList_7;
        List<string> recordlist = [];
        for (int i = 0; i < ((ITrainerStatRecord)SAV).RecordCount; i++)
        {
            if (!recordres.TryGetValue(i, out var name))
                name = $"{i:D3}";
            recordlist.Add(name);
        }
        TrainerPropPicker.ItemsSource = recordlist;
        TrainerPropPicker.SelectedIndex = recordres.FirstOrDefault().Key;
        dsRegionPicker.ItemsSource = (System.Collections.IList)GameInfo.Sources.Regions;
        dsRegionPicker.ItemDisplayBinding = new Binding("Text");
        LanguagePicker.ItemsSource = (System.Collections.IList)GameInfo.LanguageDataSource(SAV.Generation, SAV.Context);
        LanguagePicker.ItemDisplayBinding = new Binding("Text");
        CountryPicker.ItemsSource = Util.GetCountryRegionList("countries", GameInfo.CurrentLanguage);
        CountryPicker.ItemDisplayBinding = new Binding("Text");
        VivillonPicker.ItemsSource = FormConverter.GetFormList((int)Species.Vivillon, GameInfo.Strings.types, GameInfo.Strings.forms, GameInfo.GenderSymbolUnicode, SAV.Context);
        VersionPicker.ItemsSource = new object[] { "Sun", "Moon", "US", "UM" };
        VersionPicker.SelectedIndex = (int)(SAV.Version - GameVersion.SN);
        TIDEntry.Text = SAV.DisplayTID.ToString("000000");
        SIDEntry.Text = SAV.DisplaySID.ToString("0000");
        OTMoneyEntry.Text = SAV.Money.ToString();
        CountryPicker.SelectedItem = Util.GetCountryRegionList("countries", GameInfo.CurrentLanguage).FirstOrDefault(z => z.Value == SAV.Country);
        dsRegionPicker.SelectedItem = GameInfo.Sources.Regions.FirstOrDefault(z => z.Value == SAV.ConsoleRegion);
        var index = ((ComboItem?)CountryPicker.SelectedItem)?.Value??0;
        RegionPicker.SelectedItem = Util.GetCountryRegionList($"sr_{index:000}", GameInfo.CurrentLanguage).FirstOrDefault(z => z.Value == SAV.Region);
        LanguagePicker.SelectedIndex = SAV.Language - 1;
        BPEntry.Number = SAV.Misc.BP;
        FCEntry.Number = SAV.Festa.FestaCoins;
        HrsPlayedEntry.Text = SAV.PlayedHours.ToString();
        MinPlayedEntry.Text = SAV.PlayedMinutes.ToString();
        SecPlayedEntry.Text = SAV.PlayedSeconds.ToString();
        VivillonPicker.SelectedIndex = SAV.Misc.Vivillon;
        if (SAV.Played.LastSavedDate.HasValue)
        {
            LSDatePicker.Date = SAV.Played.LastSavedDate.Value;
            LSTimePicker.Time = SAV.Played.LastSavedDate.Value.TimeOfDay;
        }
        else
        {
            LSLabel.IsVisible = LSDatePicker.IsVisible = LSTimePicker.IsVisible = false;
        }

        DateUtil.GetDateTime2000(SAV.SecondsToStart, out var date, out var time);
        GSDatePicker.Date = date;
        GSTimePicker.Time = time.TimeOfDay;
        DateUtil.GetDateTime2000(SAV.SecondsToFame, out date, out time);
        HOFDatePicker.Date = date;
        HOFTimePicker.Time = time.TimeOfDay;
    }
    private static ComboItem[] GetAlolaTimeList()
    {
        var alolatime_list = new ComboItem[24];
        for (int i = 1; i < alolatime_list.Length; i++)
            alolatime_list[i] = new ComboItem($"+{i:00} Hours", i * 60 * 60);
        alolatime_list[0] = new ComboItem("Sun Time", 24 * 60 * 60);
        alolatime_list[12] = new ComboItem("Moon Time", 12 * 60 * 60);
        return alolatime_list;
    }
    private void MaxCash(object sender, EventArgs e)
    {
        OTMoneyEntry.Text = "9999999";
    }
    private void UpdateRegion(object sender, EventArgs e)
    {
        var index = ((ComboItem)CountryPicker.SelectedItem).Value;
        RegionPicker.ItemsSource = Util.GetCountryRegionList($"sr_{index:000}", GameInfo.CurrentLanguage);
        RegionPicker.ItemDisplayBinding = new Binding("Text");
    }
    public bool Editing = false;
    private void GetStats(object sender, EventArgs e)
    {
        Editing = true;
        int index = TrainerPropPicker.SelectedIndex;
        int val = SAV.GetRecord(index);
        TPEntry.Number = val;
        int offset = SAV.GetRecordOffset(index);
        OffsetValueLabel.Text = $"0x{offset:X3}";
        Editing = false;
    }

    private void ChangeStats(object sender, EventArgs e)
    {
        if (Editing)
            return;
        int index = TrainerPropPicker.SelectedIndex;
        SAV.SetRecord(index, (int)TPEntry.Number);
    }
    public void SaveTE7()
    {
        SAV.Version = (GameVersion)(VersionPicker.SelectedIndex + 30);
        SAV.Gender = (byte)GenderPicker.SelectedIndex;
        SAV.Money = Util.ToUInt32(OTMoneyEntry.Text);
        SAV.Region = (byte)((ComboItem)RegionPicker.SelectedItem).Value;
        SAV.Country = (byte)((ComboItem)CountryPicker.SelectedItem).Value;
        SAV.ConsoleRegion = (byte)((ComboItem)dsRegionPicker.SelectedItem).Value;
        SAV.Language = ((ComboItem)LanguagePicker.SelectedItem).Value;
        SAV.GameTime.AlolaTime =(ulong) ((ComboItem)TimeOffsetPicker.SelectedItem).Value;
        SAV.OT = OTNameEntry.Text;
        var parsed = int.TryParse(TIDEntry.Text, System.Globalization.CultureInfo.InvariantCulture, out var result);
        if (parsed) SAV.DisplayTID = (uint)result;
        parsed = int.TryParse(SIDEntry.Text, System.Globalization.CultureInfo.InvariantCulture, out result);
        if (parsed) SAV.DisplaySID = (uint)result;
        parsed = int.TryParse(HrsPlayedEntry.Text, System.Globalization.CultureInfo.InvariantCulture, out result);
        if (parsed) SAV.PlayedHours = result;
        parsed = int.TryParse(MinPlayedEntry.Text, System.Globalization.CultureInfo.InvariantCulture, out result);
        if (parsed) SAV.PlayedMinutes = result;
        parsed = int.TryParse(SecPlayedEntry.Text, System.Globalization.CultureInfo.InvariantCulture, out result);
        if (parsed) SAV.PlayedSeconds = result;
        SAV.SecondsToStart = (uint)DateUtil.GetSecondsFrom2000(GSDatePicker.Date.GetValueOrDefault(), GSDatePicker.Date.GetValueOrDefault().AddSeconds(GSTimePicker.Time.GetValueOrDefault().TotalSeconds));
        SAV.SecondsToFame = (uint)DateUtil.GetSecondsFrom2000(HOFDatePicker.Date.GetValueOrDefault(), HOFDatePicker.Date.GetValueOrDefault().AddSeconds(HOFTimePicker.Time.GetValueOrDefault().TotalSeconds));
        if (SAV.Played.LastSavedDate.HasValue)
            SAV.Played.LastSavedDate = LSDatePicker.Date.GetValueOrDefault().AddSeconds(LSTimePicker.Time.GetValueOrDefault().TotalSeconds);
        SAV.Misc.Vivillon = VivillonPicker.SelectedIndex;
        SAV.Misc.BP = (uint)BPEntry.Number;
        SAV.Festa.FestaCoins = (int)FCEntry.Number;
    }
}