using PKHeX.Core;

namespace PKHeXMAUI;

public partial class TrainerEditor9Blueberry : ContentPage
{
	public SAV9SV SAV = (SAV9SV)MainPage.sav;
	public TrainerEditor9Blueberry()
	{
		InitializeComponent();
        BPEditor.Text = SAV.BlueberryPoints.ToString();
        SQEditor.Text = SAV.BlueberryQuestRecord.QuestsDoneSolo.ToString();
        GQEditor.Text = SAV.BlueberryQuestRecord.QuestsDoneGroup.ToString();
        ThrowStylePicker.ItemsSource = Util.GetStringList("throw_styles","en");
        ThrowStylePicker.SelectedIndex = (int)SAV.ThrowStyle - 1;

    }

    private void MaxBP(object sender, EventArgs e)
    {
		BPEditor.Text = (SAV.BlueberryPoints = (uint)SAV.MaxMoney).ToString();
    }

    private void AddSQ(object sender, EventArgs e)
    {
        SQEditor.Text = (SAV.BlueberryQuestRecord.QuestsDoneSolo++).ToString();
    }
    private void SubtractSQ(object sender, EventArgs e)
    {
        SQEditor.Text = (SAV.BlueberryQuestRecord.QuestsDoneSolo--).ToString();
    }
    private void AddGQ(object sender, EventArgs e)
    {
        GQEditor.Text = (SAV.BlueberryQuestRecord.QuestsDoneGroup++).ToString();
    }
    private void SubtractGQ(object sender, EventArgs e)
    {
        GQEditor.Text = (SAV.BlueberryQuestRecord.QuestsDoneGroup--).ToString();
    }

    private void UnlockLegends(object sender, EventArgs e)
    {
        SAV.ActivateSnacksworthLegendaries();
        ActivateLegendsButton.IsEnabled = false;
        DisplayAlert("Legends", "Snacksworth Activated", "cancel");
    }

    private void UnlockCoaches(object sender, EventArgs e)
    {
        SAV.UnlockAllCoaches();
        CoachesButton.IsEnabled = false;
        DisplayAlert("Coaches", "All Coaches Unlocked", "cancel");
    }

    private void UnlockThrowStyles(object sender, EventArgs e)
    {
        SAV.UnlockAllThrowStyles();
        ThrowStyleButton.IsEnabled = false;
        DisplayAlert("ThrowStyles", "All ThrowStyles Unlocked", "cancel");
    }
}