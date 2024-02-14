using PKHeX.Core;
using System.Windows.Input;
using static PKHeXMAUI.MainPage;

namespace PKHeXMAUI;

public partial class PartyTab : ContentPage
{
	public PartyTab()
	{
		InitializeComponent();
        PartyView.ItemTemplate = new DataTemplate(() =>
        {
            Border border = new() { Stroke = Colors.Black, BackgroundColor = Colors.Transparent };
            Grid grid = new() { Padding = 10 };
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });

            Image image = new() { HeightRequest = 75, WidthRequest = 75 };
            image.SetBinding(Image.SourceProperty, "url");
            Image shiny = new() { Source = "rare_icon.png", HeightRequest = 16, WidthRequest = 16, VerticalOptions = LayoutOptions.Start };
            shiny.SetBinding(Image.IsVisibleProperty, "shiny");
            shiny.TranslateTo(shiny.TranslationX + 15, TranslationY);
            grid.Add(image);
            grid.Add(shiny);
            var tap = new TapGestureRecognizer();
            tap.SetBinding(TapGestureRecognizer.CommandParameterProperty, "boxie");
            tap.Tapped += Tapety;
            grid.GestureRecognizers.Add(tap);
            border.Content = grid;
            return border;
        });
        PartyView.ItemsLayout = new GridItemsLayout(2, ItemsLayoutOrientation.Vertical);

        ICommand refreshCommand = new Command(async() =>
        {
            await fillParty();
            PartyView.ScrollTo(0);
            PartyRefresh.IsRefreshing = false;
        });
        PartyRefresh.Command = refreshCommand;
        fillParty();
    }
    private async void Tapety(object sender, TappedEventArgs e)
    {
        PartyView.SelectedItem = e.Parameter;

        var result = await DisplayActionSheet($"Slot {((boxsprite)e.Parameter).SlotNumber}", "cancel", "Delete", ["View", "Set"]);
        switch (result)
        {
            case "Delete": del(sender, e); break;
            case "View": ApplyPKFromParty(sender, e); break;
            case "Set": inject(sender, e); break;
        }
    }
    private async void ApplyPKFromParty(object sender, EventArgs e)
    {
        boxsprite b = (boxsprite)PartyView.SelectedItem;
        pk = b.pkm;
        fillParty();
    }
    private async void inject(object sender, EventArgs e)
    {
        boxsprite b = (boxsprite)PartyView.SelectedItem;
        sav.SetPartySlotAtIndex(pk, int.Parse(b.SlotNumber));
        fillParty();
    }
    private async void del(object sender, EventArgs e)
    {
        boxsprite b = (boxsprite)PartyView.SelectedItem;
        sav.DeletePartySlot(int.Parse(b.SlotNumber));
        fillParty();
    }
    public static List<boxsprite> PartySprites = [];
    public async Task fillParty()
    {
        PartySprites = [];
        int i = 0;
        foreach(var ppk in sav.PartyData)
        {
            PartySprites.Add(new boxsprite(ppk,i));
            i++;
        }
        while (i < 6)
        {
            PartySprites.Add(new boxsprite(EntityBlank.GetBlank(sav.Generation, (GameVersion)sav.Version), i));
            i++;
        }
        PartyView.ItemsSource = PartySprites;
        PartyView.ScrollTo(0);
    }
}