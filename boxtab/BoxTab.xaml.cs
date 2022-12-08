using System;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using PKHeX.Core;
using static pk9reader.MainPage;


namespace pk9reader;

public partial class BoxTab : ContentPage
{
	public BoxTab()
	{
		InitializeComponent();
        ICommand refreshCommand = new Command(() =>
        {
			
			fillbox();
            boxrefresh.IsRefreshing = false;
        });
        boxrefresh.Command = refreshCommand;

    }
	public void fillbox()
	{
		
		IList<boxsprite> boxsprites = new List<boxsprite>();
		foreach (var boxpk in sav.GetBoxData(int.Parse(boxnum.Text)))
		{
			var boxinfo = new boxsprite(boxpk);
			boxsprites.Add(boxinfo);
		}
		boxview.ItemsSource= boxsprites;
        boxview.ItemTemplate = new DataTemplate(() =>
        {
            Grid grid = new Grid { Padding = 10 };
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });


            Image image = new Image() { HeightRequest = 100, WidthRequest = 100 };
            image.SetBinding(Image.SourceProperty, "url");
            grid.Add(image);
            return grid;
        });


    }

    private async void readboxdata(object sender, EventArgs e)
    {
        IEnumerable<long> jumps = new long[] { 0x4384B18, 0x128, 0x9B0, 0x0 };
        var off = await botBase.PointerRelative(jumps);
        var bytes = await botBase.ReadBytesAsync((uint)off+(uint.Parse(boxnum.Text)*10320), 10320);
		sav.SetBoxBinary(bytes, int.Parse(boxnum.Text));
   
		
		
    }

    private async void applypkfrombox(object sender, SelectionChangedEventArgs e)
    {
        foreach(boxsprite b in e.CurrentSelection)
        {
            pk = (PK9)b.pkm;
        }
        
    }
}
public class boxsprite
{
	public boxsprite(PKM pk9)
	{
		pkm = pk9;
		species = $"{(Species)pk9.Species}";
		url= $"https://raw.githubusercontent.com/santacrab2/Resources/main/gen9sprites/{pk9.Species:0000}{(pk9.Form != 0 ? $"-{pk9.Form:00}" : "")}.png";
    }
	public PKM pkm { get; set; }
	public string url { get; set; }
	public string species { get; set; }
}