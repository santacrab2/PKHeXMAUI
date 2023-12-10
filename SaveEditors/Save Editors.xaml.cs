namespace PKHeXMAUI;

public partial class SaveEditors : ContentPage
{
	public SaveEditors()
	{
		InitializeComponent();
	}

    private void OpenItems(object sender, EventArgs e)
    {
        Navigation.PushModalAsync(new Items());
    }
}