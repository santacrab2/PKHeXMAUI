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
		LanguagePicker.ItemsSource = (List<ComboItem>)GameInfo.LanguageDataSource(MainPage.sav.Generation);
		LanguagePicker.ItemDisplayBinding = new Binding("Text");
		LanguagePicker.SelectedItem = ((List<ComboItem>)GameInfo.LanguageDataSource(MainPage.sav.Generation)).Find(z=>z.Value == MainPage.sav.Language);
    }

    private void MaxMoney(object sender, EventArgs e)
    {
		TrainerMoneyEditor.Text = (MainPage.sav.Money = (uint)MainPage.sav.MaxMoney).ToString();
    }

    private void MaxLP(object sender, EventArgs e)
    {
		TrainerLPEditor.Text = (((SAV9SV)MainPage.sav).LeaguePoints = (uint)MainPage.sav.MaxMoney).ToString();
    }
}