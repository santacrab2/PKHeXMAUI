
using PKHeX.Core;

namespace PKHeXMAUI;

public partial class TrainerEditor8a : ContentPage
{
	SAV8LA SAV = (SAV8LA)MainPage.sav;
	public TrainerEditor8a()
	{
		InitializeComponent();
		OTName.Text = SAV.OT;
		TE8aSIDEntry.Text = SAV.DisplaySID.ToString();
		TE8aTIDEntry.Text = SAV.DisplayTID.ToString();
		TE8aMoneyEntry.Text = SAV.Money.ToString();
		TE8aGenderPicker.ItemsSource = GameInfo.GenderSymbolUnicode.Take(2).ToArray();
		TE8aGenderPicker.SelectedIndex = SAV.Gender;
		TE8aLanguagePicker.ItemsSource = (List<ComboItem>)GameInfo.LanguageDataSource(SAV.Generation);
		TE8aLanguagePicker.ItemDisplayBinding = new Binding("Text");
		TE8aLanguagePicker.SelectedItem = ((List<ComboItem>)GameInfo.LanguageDataSource(SAV.Generation)).Find(z => z.Value == SAV.Language);
		var CMP = (uint)SAV.Blocks.GetBlockValue(SaveBlockAccessor8LA.KMeritCurrent);
		TE8aCMPEntry.Number = Math.Min(CMP, 999999999);
		var EMP = (uint)SAV.Blocks.GetBlockValue(SaveBlockAccessor8LA.KMeritEarnedTotal);
		TE8aEMPEntry.Number = Math.Min(EMP, 999999999);
		var GR = (uint)SAV.Blocks.GetBlockValue(SaveBlockAccessor8LA.KExpeditionTeamRank);
		TE8aEMPEntry.Number = Math.Min(GR, 999999999);
		var SU = (uint)SAV.Blocks.GetBlockValue(SaveBlockAccessor8LA.KSatchelUpgrades);
		TE8aSUEntry.Number = Math.Min(SU, 999999999);
		TE8aHPEntry.Text = SAV.PlayedHours.ToString();
		TE8aMPEntry.Text = SAV.PlayedMinutes.ToString();
		TE8aSPEntry.Text = SAV.PlayedSeconds.ToString();
		TE8aGSDatePicker.Date = SAV.AdventureStart.Timestamp;
		TE8aGSTimePicker.Time = SAV.AdventureStart.Timestamp.TimeOfDay;
		TE8aLSDatePicker.Date = SAV.LastSaved.Timestamp;
		TE8aLSTimePicker.Time = SAV.LastSaved.Timestamp.TimeOfDay;
    }

    private void MaxMoney(object sender, EventArgs e)
    {
		TE8aMoneyEntry.Text = MainPage.sav.MaxMoney.ToString();
    }
	public void SaveTE8a()
	{
		SAV.OT = OTName.Text;
		var parsed = uint.TryParse(TE8aSIDEntry.Text, out var result);
		SAV.DisplaySID = parsed ? result : SAV.DisplaySID;
		parsed = uint.TryParse(TE8aTIDEntry.Text, out result);
		SAV.DisplayTID = parsed ? result : SAV.DisplayTID;
		parsed = uint.TryParse(TE8aMoneyEntry.Text, out result);
		SAV.Money = parsed ? result : SAV.Money;
		SAV.Gender = (byte)TE8aGenderPicker.SelectedIndex;
		SAV.Language = ((ComboItem)TE8aLanguagePicker.SelectedItem).Value;
		SAV.Blocks.SetBlockValue(SaveBlockAccessor8LA.KMeritCurrent, TE8aCMPEntry.Number);
		SAV.Blocks.SetBlockValue(SaveBlockAccessor8LA.KMeritEarnedTotal, TE8aEMPEntry.Number);
		SAV.Blocks.SetBlockValue(SaveBlockAccessor8LA.KExpeditionTeamRank, TE8aGREntry.Number);
		SAV.Blocks.SetBlockValue(SaveBlockAccessor8LA.KSatchelUpgrades, TE8aSUEntry.Number);
		parsed = int.TryParse(TE8aHPEntry.Text, out var iresult);
		SAV.PlayedHours = parsed ? iresult : SAV.PlayedHours;
		parsed = int.TryParse(TE8aMPEntry.Text, out iresult);
		SAV.PlayedMinutes = parsed ? iresult : SAV.PlayedMinutes;
		parsed = int.TryParse(TE8aSPEntry.Text, out iresult);
		SAV.PlayedSeconds = parsed ? iresult : SAV.PlayedSeconds;
		var GS = TE8aGSDatePicker.Date;
		GS.AddSeconds(TE8aGSTimePicker.Time.TotalSeconds);
		SAV.AdventureStart.Timestamp = GS;
		var LS = TE8aLSDatePicker.Date;
		LS.AddSeconds(TE8aLSTimePicker.Time.TotalSeconds);
		SAV.LastSaved.Timestamp = LS;
	}
}