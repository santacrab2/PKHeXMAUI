using PKHeX.Core;

using System.ComponentModel;
using System.Windows.Input;
using static PKHeXMAUI.MainPage;
namespace PKHeXMAUI;

public partial class RibbonSelector : ContentPage
{
    public static bool ApplicatorMode = false;
    public RibbonSelector()
    {
        InitializeComponent();
        if (ApplicatorMode)
            applyribbons.IsVisible = false;

        ribboncollection.ItemTemplate = new DataTemplate(() =>
        {
            Grid grid = new() { Padding = 10 };
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = 350 });
            Label ribbonname = new() { HorizontalTextAlignment = TextAlignment.End };
            ribbonname.SetBinding(Label.TextProperty, "Name");
            grid.Add(ribbonname);
            Image allthetests = new() { HeightRequest = 50, WidthRequest = 50, HorizontalOptions = LayoutOptions.Start, Margin = new Thickness(50, 0, 0, 0) };
            allthetests.SetBinding(Image.SourceProperty, "spritename");
            Image AffixedSprite = new() { HeightRequest = 16, WidthRequest = 16, HorizontalOptions = LayoutOptions.End, VerticalOptions = LayoutOptions.End, Margin = new Thickness(50, 0, 0, 0), Source = "valid.png" };
            AffixedSprite.SetBinding(Image.IsVisibleProperty, "affixed");
            grid.Add(allthetests);
            grid.Add(AffixedSprite);
            SwipeView Swipe = new();
            SwipeItem affix = new()
            {
                Text = "Affix",
                BackgroundColor = Colors.DeepSkyBlue,
                IconImageSource = "ribbon_affix_noneb.png"
            };
            affix.Invoked += AffixRibbon;
            if(ApplicatorMode)
                Swipe.LeftItems.Add(affix);
            Swipe.Content = grid;
            return Swipe;
        });
        var clone = pk.Clone();
        RibbonApplicator.SetAllValidRibbons(clone);
        var ribbs = RibbonInfo.GetRibbonInfo(clone);
        List<Ribbonstuff> idk = [];
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
        if (!ApplicatorMode)
        {
            ribboncollection.SelectionMode = SelectionMode.Multiple;
            ribboncollection.ItemsSource = idk;
            ribboncollection.UpdateSelectedItems(selectedribbonslist);
        }
        else
        {
            ribboncollection.SelectionMode = SelectionMode.Single;
            ribboncollection.ItemsSource = selectedribbonslist;
        }
    }
    public void Refresh()
    {
        var clone = pk.Clone();
        RibbonApplicator.SetAllValidRibbons(clone);
        var ribbs = RibbonInfo.GetRibbonInfo(clone);
        List<Ribbonstuff> idk = [];
        foreach (var fg in ribbs)
        {
            if (fg.HasRibbon)
                idk.Add(new Ribbonstuff(fg));
        }
        var selectedribbonslist = new List<object>();
        var pkhasribbonslist = RibbonInfo.GetRibbonInfo(pk);
        int o = 0;
        foreach (var pkrib in pkhasribbonslist)
        {
            if (pkrib.HasRibbon)
            {
                foreach (var imrunningoutofnames in idk)
                {
                    if (imrunningoutofnames.index == o)
                    {
                        selectedribbonslist.Add(imrunningoutofnames);
                    }
                }
            }
            o++;
        }
        if (!ApplicatorMode)
        {
            ribboncollection.SelectionMode = SelectionMode.Multiple;
            ribboncollection.ItemsSource = idk;
            ribboncollection.UpdateSelectedItems(selectedribbonslist);
        }
        else
        {
            ribboncollection.SelectionMode = SelectionMode.Single;
            ribboncollection.ItemsSource = selectedribbonslist;
        }
        ribboncollection.ScrollTo(0);
    }
    private void AffixRibbon(object sender,EventArgs e)
    {
        if (ribboncollection.SelectedItem == null)
            return;
        if(pk is IRibbonSetAffixed a)
        {
            var ribbon = (Ribbonstuff)ribboncollection.SelectedItem;
            a.AffixedRibbon = a.AffixedRibbon == -1 ? (sbyte)ribbon.index : (sbyte)-1;
        }
        Refresh();
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

        if(pk is IRibbonSetAffixed af)
        {
            affixed = af.AffixedRibbon == index;
        }
        typer = rib.Type;
        Name = rib.Name.Replace("Ribbon","");
        spritename = $"{rib.Name}.png";
    }
    public string Name { get; set; }
   public string spritename { get; set; }
    public RibbonValueType typer{ get; set; }
    public int index { get; set; }
    public bool affixed { get; set; }
}