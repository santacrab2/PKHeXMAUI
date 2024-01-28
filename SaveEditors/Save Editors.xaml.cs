using PKHeX.Core;

namespace PKHeXMAUI;

public partial class SaveEditors : ContentPage
{
	public SaveEditors()
	{
		InitializeComponent();
        ToggleControls();
	}

    private void OpenItems(object sender, EventArgs e)
    {
        Navigation.PushModalAsync(new Items());
    }

    private void OpenBlockEditor(object sender, EventArgs e)
    {
        Navigation.PushModalAsync(new BlockDataTab());
    }
    private void ToggleControls()
    {
        if (!MainPage.sav.State.Exportable || MainPage.sav is BulkStorage)
            return;
        Button_BlockData.IsVisible = true;
    }
}