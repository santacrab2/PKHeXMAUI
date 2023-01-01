using System;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;
using PKHeX.Core;
using static pk9reader.MainPage;


namespace pk9reader;

public partial class BoxTab : ContentPage
{
	public BoxTab()
	{
		InitializeComponent();
        for(int k = 1; k < 31; k++)
        {
            boxnum.Items.Add(k.ToString());
        }
        ICommand refreshCommand = new Command(() =>
        {
			
			fillbox();
            boxrefresh.IsRefreshing = false;
        });
        boxrefresh.Command = refreshCommand;

    }
    public static IList<boxsprite> boxsprites = new List<boxsprite>();
    
	public void fillbox()
	{
		
		boxsprites = new List<boxsprite>();
		foreach (var boxpk in sav.GetBoxData(boxnum.SelectedIndex))
		{
			var boxinfo = new boxsprite(boxpk);
			boxsprites.Add(boxinfo);
		}
		
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
        boxview.ItemsSource = boxsprites;


    }

    private async void readboxdata(object sender, EventArgs e)
    {
        IEnumerable<long> jumps = new long[] { 0x4384B18, 0x128, 0x9B0, 0x0 };
        var off = await botBase.PointerRelative(jumps);
        for (int i = 0; i < 30; i++)
        {
            var bytes = await botBase.ReadBytesAsync((uint)off + (uint)(i * 10320), 10320);

            sav.SetBoxBinary(bytes, i);
        }
   
		
		
    }

    private async void applypkfrombox(object sender, SelectionChangedEventArgs e)
    {
        foreach(boxsprite b in e.CurrentSelection)
        {
            if (b.pkm.Species != 0)
            {
                pk = (PK9)b.pkm;
            }
        
        }
        
    }

    private async void injectboxtab(object sender, EventArgs e)
    {
        pk.ResetPartyStats();
        IEnumerable<long> jumps = new long[] { 0x4384B18, 0x128, 0x9B0, 0x0 };
        var off = await botBase.PointerRelative(jumps);
        await botBase.WriteBytesAsync(pk.EncryptedPartyData, (uint)off + (uint)(344 * 30 * boxnum.SelectedIndex) + ((uint)(344 * boxsprites.IndexOf((boxsprite)boxview.SelectedItem) )));
    }

    private void changebox(object sender, EventArgs e)
    {
        fillbox();
    }
}
public class boxsprite
{
	public boxsprite(PKM pk9)
	{
		pkm = pk9;
		species = $"{(Species)pk9.Species}";
        if (pk9.Species == 0)
            url = $"https://raw.githubusercontent.com/santacrab2/Resources/main/gen9sprites/{pk9.Species:0000}{(pk9.Form != 0 ? $"-{pk9.Form:00}" : "")}.png";
        else if (pk9.IsShiny)
            url = $"https://www.serebii.net/Shiny/SV/{pk9.Species}.png";
        else if(pk9.Form != 0)
            url = $"https://raw.githubusercontent.com/santacrab2/Resources/main/gen9sprites/{pk9.Species:0000}{(pk9.Form != 0 ? $"-{pk9.Form:00}" : "")}.png";
        else
            url = $"https://www.serebii.net/scarletviolet/pokemon/{pk9.Species}.png";
    }
	public PKM pkm { get; set; }
	public string url { get; set; }
	public string species { get; set; }
}