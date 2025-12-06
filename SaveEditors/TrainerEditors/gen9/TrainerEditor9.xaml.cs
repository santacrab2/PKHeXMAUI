
using PKHeX.Core;

namespace PKHeXMAUI;

public partial class TrainerEditor9 : ContentPage
{
	public TrainerEditor9()
	{
		InitializeComponent();
		TrainerNameEditor.Text = MainPage.sav.OT;
		TrainerGenderPicker.ItemsSource = GameInfo.GenderSymbolUnicode.Take(2).ToArray();
		TrainerTIDEditor.Text = MainPage.sav.DisplayTID.ToString();
		TrainerSIDEditor.Text = MainPage.sav.DisplaySID.ToString();
		TrainerGenderPicker.SelectedIndex = MainPage.sav.Gender;
		TrainerMoneyEditor.Text = MainPage.sav.Money.ToString();
		TrainerLPEditor.Text = ((SAV9SV)MainPage.sav).LeaguePoints.ToString();
        var games = GameInfo.Strings.gamelist;
		VersionPicker.Items.Add(games[(int)GameVersion.SL]);
		VersionPicker.Items.Add(games[(int)GameVersion.VL]);
		VersionPicker.SelectedIndex = MainPage.sav.Version - GameVersion.SL;
		LanguagePicker.ItemsSource = GameInfo.LanguageDataSource(MainPage.sav.Generation, MainPage.sav.Context).ToList();
		LanguagePicker.ItemDisplayBinding = new Binding("Text");
		LanguagePicker.SelectedItem = GameInfo.LanguageDataSource(MainPage.sav.Generation, MainPage.sav.Context).ToList().Find(z=>z.Value == MainPage.sav.Language);
		TrainerMinutesEditor.Text = MainPage.sav.PlayedMinutes.ToString();
		TrainerHoursEditor.Text = MainPage.sav.PlayedHours.ToString();
		TrainerSecondsEditor.Text = MainPage.sav.PlayedSeconds.ToString();
		GameStartedPicker.Date = ((SAV9SV)MainPage.sav).EnrollmentDate.Timestamp;
		LastSavedPicker.Date = ((SAV9SV)MainPage.sav).LastSaved.Timestamp;
		LastSavedTimePicker.Time = ((SAV9SV)MainPage.sav).LastSaved.Timestamp.TimeOfDay;
    }
    private void MaxMoney(object sender, EventArgs e)
    {
		TrainerMoneyEditor.Text = (MainPage.sav.Money = (uint)MainPage.sav.MaxMoney).ToString();
    }

    private void MaxLP(object sender, EventArgs e)
    {
		TrainerLPEditor.Text = (((SAV9SV)MainPage.sav).LeaguePoints = (uint)MainPage.sav.MaxMoney).ToString();
    }
	public void SaveTrainerEditor9()
	{
		MainPage.sav.OT = TrainerNameEditor.Text;
		MainPage.sav.Gender = (byte)TrainerGenderPicker.SelectedIndex;
		var parsed = uint.TryParse(TrainerTIDEditor.Text, out var result);
		MainPage.sav.DisplayTID = parsed ? result : MainPage.sav.DisplayTID;
		parsed = uint.TryParse(TrainerSIDEditor.Text, out result);
		MainPage.sav.DisplaySID = parsed ? result : MainPage.sav.DisplaySID;
		parsed = uint.TryParse(TrainerMoneyEditor.Text, out result);
		MainPage.sav.Money = parsed ? result : MainPage.sav.Money;
		parsed = uint.TryParse(TrainerLPEditor.Text, out result);
		((SAV9SV)MainPage.sav).LeaguePoints = parsed ? result : ((SAV9SV)MainPage.sav).LeaguePoints;
		MainPage.sav.Language = ((ComboItem)LanguagePicker.SelectedItem).Value;
		parsed = int.TryParse(TrainerSecondsEditor.Text, out var iresult);
		MainPage.sav.PlayedSeconds = parsed ? iresult : MainPage.sav.PlayedSeconds;
		parsed = int.TryParse(TrainerHoursEditor.Text, out iresult);
		MainPage.sav.PlayedHours = parsed ? iresult : MainPage.sav.PlayedHours;
		parsed = int.TryParse(TrainerMinutesEditor.Text, out iresult);
		MainPage.sav.PlayedMinutes = parsed ? iresult : MainPage.sav.PlayedMinutes;
		((SAV9SV)MainPage.sav).LastSaved.Timestamp = LastSavedPicker.Date;
		((SAV9SV)MainPage.sav).EnrollmentDate.Timestamp = GameStartedPicker.Date;
	}
}