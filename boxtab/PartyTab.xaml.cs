using System;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;
using System.Linq;
using PKHeX.Core;
using static PKHeXMAUI.MainPage;
using Microsoft.Maui.Controls.PlatformConfiguration.GTKSpecific;

namespace PKHeXMAUI;

public partial class PartyTab : ContentPage
{
	public PartyTab()
	{
		InitializeComponent();
        PartyView.ItemTemplate = new DataTemplate(() =>
        {
            Grid grid = new Grid { Padding = 10 };
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
         
            Image image = new Image() { HeightRequest = 75, WidthRequest = 75 };
            image.SetBinding(Image.SourceProperty, "url");
            Image shiny = new Image() { Source = "rare_icon.png", HeightRequest = 16, WidthRequest = 16, VerticalOptions = LayoutOptions.Start };
            shiny.SetBinding(Image.IsVisibleProperty, "shiny");
            shiny.TranslateTo(shiny.TranslationX + 15, TranslationY);
            grid.Add(image);
            grid.Add(shiny);
            SwipeView swipe = new();
            SwipeItem view = new()
            {
                Text = "View",
                BackgroundColor = Colors.DeepSkyBlue,
                IconImageSource = "load.png"
            };
            view.Invoked += ApplyPKFromParty;
            swipe.BottomItems.Add(view);
            SwipeItem Set = new()
            {
                Text = "Set",
                BackgroundColor = Colors.Green,
                IconImageSource = "dump.png"
            };
            Set.Invoked += inject;
            swipe.TopItems.Add(Set);
            swipe.Content = grid;
            return swipe;
        });
        PartyView.ItemsLayout = new GridItemsLayout(2, ItemsLayoutOrientation.Vertical);
     
        ICommand refreshCommand = new Command(async() =>
        {

            PartyView.ItemsSource = await fillParty();
            PartyView.ScrollTo(0);
            PartyRefresh.IsRefreshing = false;
        });
        PartyRefresh.Command = refreshCommand;
        fillParty();
    }
    private async void ApplyPKFromParty(object sender, EventArgs e)
    {
        boxsprite b = (boxsprite)PartyView.SelectedItem;
        pk = b.pkm;

    }
    private async void inject(object sender, EventArgs e)
    {
        sav.SetPartySlotAtIndex(pk, PartySprites.IndexOf((boxsprite)PartyView.SelectedItem));
        
    }
    public static List<boxsprite> PartySprites = new List<boxsprite>();
    public async Task<List<boxsprite>> fillParty()
    {

        PartySprites = new List<boxsprite>();

        foreach(var ppk in sav.PartyData)
        {
            PartySprites.Add(new boxsprite(ppk));
        }
        
        return PartySprites;

    }
}