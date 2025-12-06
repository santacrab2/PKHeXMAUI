
using PKHeX.Core;

namespace PKHeXMAUI;

public partial class TrainerEditor6 : ContentPage
{
	public SAV6 SAV = (SAV6)MainPage.sav;
	public TrainerEditor6()
	{
		InitializeComponent();
		OTNameEntry.Text = SAV.OT;
		GenderPicker.ItemsSource = GameInfo.GenderSymbolUnicode.Take(2).ToArray();
		GenderPicker.SelectedIndex = SAV.Gender;
		var recordres = RecordLists.RecordList_6;
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
		VersionPicker.ItemsSource = new object[] { "X", "Y", "AS", "OR" };
		VersionPicker.SelectedIndex = (int)(SAV.Version - 0x18);
		TIDEntry.Text = SAV.TID16.ToString("00000");
		SIDEntry.Text = SAV.SID16.ToString("00000");
		OTMoneyEntry.Text = SAV.Money.ToString();
		CountryPicker.SelectedItem = Util.GetCountryRegionList("countries", GameInfo.CurrentLanguage).FirstOrDefault(z => z.Value == SAV.Country);
		dsRegionPicker.SelectedItem = GameInfo.Sources.Regions.FirstOrDefault(z => z.Value == SAV.ConsoleRegion);
		var index = ((ComboItem?)CountryPicker.SelectedItem)?.Value??0;
		RegionPicker.SelectedItem = Util.GetCountryRegionList($"sr_{index:000}", GameInfo.CurrentLanguage).FirstOrDefault(z=>z.Value == SAV.Region);
		LanguagePicker.SelectedIndex = SAV.Language - 1;
		BPEntry.Text = SAV.BP.ToString();
		PMEntry.Text = SAV.GetRecord(63).ToString();
        var sit = SAV.Situation;
        StyleEntry.Text = sit.Style.ToString();
		HrsPlayedEntry.Text = SAV.PlayedHours.ToString();
		MinPlayedEntry.Text = SAV.PlayedMinutes.ToString();
		SecPlayedEntry.Text = SAV.PlayedSeconds.ToString();
		VivillonPicker.SelectedIndex = SAV.Vivillon;
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

    private void MaxCash(object sender, EventArgs e)
    {
		OTMoneyEntry.Text = "9,999,999";
    }

    private void UpdateRegion(object sender, EventArgs e)
    {
		var index = ((ComboItem)CountryPicker.SelectedItem).Value;
        RegionPicker.ItemsSource= Util.GetCountryRegionList($"sr_{index:000}", GameInfo.CurrentLanguage);
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
	public void SaveTE6()
	{
		SAV.Version = (GameVersion)(VersionPicker.SelectedIndex + 0x18);
		SAV.Gender = (byte)GenderPicker.SelectedIndex;
		var parsed = ushort.TryParse(TIDEntry.Text, out var result);
		if (parsed) SAV.TID16 = result;
		parsed = ushort.TryParse(SIDEntry.Text, out result);
		if (parsed) SAV.SID16 = result;
		parsed = uint.TryParse(OTMoneyEntry.Text, out var uresult);
		if (parsed) SAV.Money = uresult;
		var index = ((ComboItem)RegionPicker.SelectedItem).Value;
		SAV.Region = (byte)index;
		index = ((ComboItem)CountryPicker.SelectedItem).Value;
		SAV.Country = (byte)index;
		index = ((ComboItem)dsRegionPicker.SelectedItem).Value;
		SAV.ConsoleRegion = (byte)index;
		index = ((ComboItem)LanguagePicker.SelectedItem).Value;
		SAV.Language = index;
		SAV.OT = OTNameEntry.Text;
		parsed = ushort.TryParse(BPEntry.Text, out result);
		if (parsed) SAV.BP = result;
		parsed = int.TryParse(PMEntry.Text, out var iresult);
		if (parsed)
		{
			SAV.SetRecord(63, iresult);
			SAV.SetRecord(64, iresult);
		}
		parsed = byte.TryParse(StyleEntry.Text, out var bresult);
        var sit = SAV.Situation;
		if (parsed) sit.Style = bresult;
		parsed = ushort.TryParse(HrsPlayedEntry.Text, out result);
		if (parsed) SAV.PlayedHours = result;
		parsed = ushort.TryParse(MinPlayedEntry.Text, out result);
		if (parsed) SAV.PlayedMinutes = result % 60;
		parsed = ushort.TryParse(SecPlayedEntry.Text, out result);
		if (parsed) SAV.PlayedSeconds = result % 60;
		SAV.Vivillon = VivillonPicker.SelectedIndex;
        SAV.SecondsToStart = (uint)DateUtil.GetSecondsFrom2000(GSDatePicker.Date, GSDatePicker.Date.AddSeconds(GSTimePicker.Time.TotalSeconds));
        SAV.SecondsToFame = (uint)DateUtil.GetSecondsFrom2000(HOFDatePicker.Date, HOFDatePicker.Date.AddSeconds(HOFTimePicker.Time.TotalSeconds));
		if (SAV.Played.LastSavedDate.HasValue)
			SAV.Played.LastSavedDate = LSDatePicker.Date.AddSeconds(LSTimePicker.Time.TotalSeconds);
    }
}