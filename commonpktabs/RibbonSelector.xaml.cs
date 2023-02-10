using PKHeX.Core;

using System.ComponentModel;
using System.Windows.Input;
using static pk9reader.MainPage;
namespace pk9reader;

public partial class RibbonSelector : ContentPage
{
    public RibbonSelector()
    {
        InitializeComponent();
        var clone = pk.Clone();
        RibbonApplicator.SetAllValidRibbons(clone);
        var ribbs = RibbonInfo.GetRibbonInfo(clone);
      
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
            grid.Add(allthetests);
            
            
            return grid;
            
        });
        List<Ribbonstuff> idk = new();
        foreach(var fg in ribbs)
        {
            if(fg.HasRibbon)
                idk.Add(new Ribbonstuff(fg));
        }
        var selectedribbonslist = new List<object>();
        var pkhasribbonslist = RibbonInfo.GetRibbonInfo(pk);
        int o = 0;
        foreach(var pkrib in pkhasribbonslist)
        {
            if (pkrib.HasRibbon)
            {
                foreach(var imrunningoutofnames in idk)
                {
                    if(imrunningoutofnames.index== o)
                    {
                        selectedribbonslist.Add(imrunningoutofnames);
                    }
                }
            }
            o++;
        }
        ribboncollection.ItemsSource= idk;
        ribboncollection.UpdateSelectedItems(selectedribbonslist);
        
        
    }

    private void applyribbonsandclose(object sender, EventArgs e)
    {
        for (int c = 0; c < 110; c++)
        {
            if (pk is IRibbonIndex ri)
                ri.SetRibbon(c, false);
                

        }

        foreach (var ribs in ribboncollection.SelectedItems)
        {
            var rib = (Ribbonstuff)ribs;
            for (int c = 0; c < 110; c++)
            {
                var ribtest = (RibbonIndex)c;
                if (rib.Name == ribtest.ToString())
                {
                    if (pk is IRibbonIndex ri)
                    {
                        ri.SetRibbon(c);
                    }
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
        for (int c = 0; c < 110; c++)
        {
            var ribtest = (RibbonIndex)c;
            if (rib.Name.Replace("Ribbon", "") == ribtest.ToString())
            {
                index = c;
            }
        }
       
         
        typer = rib.Type;
        Name = rib.Name.Replace("Ribbon","");
        spritename = $"{rib.Name}.png";
    }
    public string Name { get; set; }
   public string spritename { get; set; }
    public RibbonValueType typer{ get; set; }
    public int index { get; set; }
}