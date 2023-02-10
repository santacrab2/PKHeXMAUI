using PKHeX.Core;
using static pk9reader.MainPage;
namespace pk9reader;

public partial class TREditor : ContentPage
{
	public TREditor()
	{
		InitializeComponent();
		trcollection.ItemTemplate = new DataTemplate(() =>
		{
            Grid grid = new Grid { Padding = 10 };
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = 350 });
			
			return grid;
        });
	}

    private void applytrsandclose(object sender, EventArgs e)
    {

    }

    private void closer(object sender, EventArgs e)
    {

    }
}

public class TREditorItem
{
	public TREditorItem(ITechRecord rec)
	{

	}
}