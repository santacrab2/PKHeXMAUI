using PKHeX.Core;

namespace PKHeXMAUI;

public partial class TrainerEditor9Blueberry : ContentPage
{
	public SAV9SV SAV = (SAV9SV)MainPage.sav;
	public TrainerEditor9Blueberry()
	{
		InitializeComponent();
        BPEditor.Text = SAV.BlueberryPoints.ToString();
        SQEditor.Number = (decimal)SAV.BlueberryQuestRecord.QuestsDoneSolo;
        GQEditor.Number = (decimal)SAV.BlueberryQuestRecord.QuestsDoneGroup;
        ThrowStylePicker.ItemsSource = Util.GetStringList("throw_styles","en");
        ThrowStylePicker.SelectedIndex = (int)SAV.ThrowStyle - 1;
    }

    private void MaxBP(object sender, EventArgs e)
    {
		BPEditor.Text = (SAV.BlueberryPoints = (uint)SAV.MaxMoney).ToString();
    }
    private void UnlockLegends(object sender, EventArgs e)
    {
        SAV.ActivateSnacksworthLegendaries();
        ActivateLegendsButton.IsEnabled = false;
        DisplayAlertAsync("Legends", "Snacksworth Activated", "cancel");
    }

    private void UnlockCoaches(object sender, EventArgs e)
    {
        SAV.UnlockAllCoaches();
        CoachesButton.IsEnabled = false;
        DisplayAlertAsync("Coaches", "All Coaches Unlocked", "cancel");
    }

    private void UnlockThrowStyles(object sender, EventArgs e)
    {
        SAV.UnlockAllThrowStyles();
        ThrowStyleButton.IsEnabled = false;
        DisplayAlertAsync("ThrowStyles", "All ThrowStyles Unlocked", "cancel");
    }

    public void SaveTE9Blueberry()
    {
        var parsed = uint.TryParse(BPEditor.Text, out var result);
        SAV.BlueberryPoints = parsed ? result : SAV.BlueberryPoints;
        SAV.BlueberryQuestRecord.QuestsDoneSolo = (uint)SQEditor.Number;
        SAV.BlueberryQuestRecord.QuestsDoneGroup = (uint)GQEditor.Number;
        SAV.ThrowStyle = (ThrowStyle9)ThrowStylePicker.SelectedIndex + 1;
    }
}