using static PKHeXMAUI.MainPage;


namespace PKHeXMAUI;

public partial class SearchSettings : ContentPage
{
	public SearchSettings()
	{
		InitializeComponent();
		EncSpecies.ItemsSource = datasourcefiltered.Species;
		
    }



    private void ChangeFontColor(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
		if (EncSpecies.IsDropDownOpen)
			EncSpecies.TextColor = Colors.Black;
		else
			EncSpecies.TextColor = Colors.White;
    }
}