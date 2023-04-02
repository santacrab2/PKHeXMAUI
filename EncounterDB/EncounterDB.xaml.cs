namespace PKHeXMAUI;

public partial class EncounterDB : ContentPage
{
	public EncounterDB()
	{
		InitializeComponent();
	}

    private void SetSearchSettings(object sender, EventArgs e)
    {
		Navigation.PushModalAsync(new SearchSettings());
    }
}