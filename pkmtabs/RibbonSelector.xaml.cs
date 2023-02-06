using PKHeX.Core;
using PKHeX.Drawing.Misc;

using System.ComponentModel;
using System.Windows.Input;
using static pk9reader.MainPage;
namespace pk9reader;

public partial class RibbonSelector : ContentPage
{
    public RibbonSelector()
    {
        InitializeComponent();
        var ribbs = RibbonInfo.GetRibbonInfo(pk);
      
        ribboncollection.ItemTemplate = new DataTemplate(() =>
        {
            Grid grid = new Grid { Padding = 10 };
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = 350 });
            Label ribbonname = new Label() { HorizontalTextAlignment = TextAlignment.End };
            ribbonname.SetBinding(Label.TextProperty, "Name");
            grid.Add(ribbonname);
            Image allthetests = new Image() { HeightRequest = 50, WidthRequest = 50, HorizontalOptions = LayoutOptions.Start, Margin = new Thickness(50, 0, 0, 0) };
            allthetests.SetBinding(Image.SourceProperty, "spritename");
            allthetests.SetBinding(BackgroundColorProperty, "bg");
            grid.Add(allthetests);
            
            
            return grid;
            
        });
        List<Ribbonstuff> idk = new();
        foreach(var fg in ribbs)
        {
            idk.Add(new Ribbonstuff(fg));
        }
        ribboncollection.ItemsSource= idk;
        ICommand refreshCommand = new Command(async () =>
        {
            var itemso = RibbonInfo.GetRibbonInfo(pk);
            ribboncollection.ItemsSource = itemso;
            List<Ribbonstuff> idk = new();
            foreach (var fg in itemso)
            {
               
                idk.Add(new Ribbonstuff(fg));
            }
            ribboncollection.ItemsSource = idk;
          
            ribbonrefresh.IsRefreshing = false;
        });
        ribbonrefresh.Command = refreshCommand;
        
    }

    private void applyribbonsandclose(object sender, EventArgs e)
    {

        foreach (var ribs in ribboncollection.SelectedItems)
        {
            var rib = (Ribbonstuff)ribs;
            for (int c = 0; c < 110; c++)
            {
                var ribtest = (RibbonIndex)c;
                if (rib.Name == ribtest.ToString())
                {
                    if (pk.GetRibbon(c))
                        pk.SetRibbon(c, false);
                    else
                        pk.SetRibbon(c);
                }
            }
        }
        Navigation.PopModalAsync();
    }

    private void closer(object sender, EventArgs e)
    {
        Navigation.PopModalAsync();
    }

 
}

public class Ribbonstuff
{
    public Ribbonstuff(RibbonInfo rib)
    {
        if (rib.HasRibbon)
            bg = Colors.Gold;
        typer = rib.Type;
        Name = rib.Name.Replace("Ribbon","");
        spritename = $"{rib.Name}.png";
    }
    public string Name { get; set; }
   public string spritename { get; set; }
    public RibbonValueType typer{ get; set; }
    public Color bg { get; set; } = Colors.Transparent;
}