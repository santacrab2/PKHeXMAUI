

using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Input;
using PKHeX.Core;

using static pk9reader.MainPage;

namespace pk9reader;

public partial class MetTab : ContentPage
{
	public MetTab()
	{
		InitializeComponent();
      
        mettabpic.Source = spriteurl;
        origingamepicker.ItemsSource = GameInfo.Strings.gamelist;
        battleversionpicker.ItemsSource = GameInfo.Strings.gamelist;
        metlocationpicker.ItemsSource = (System.Collections.IList)GameInfo.GetLocationList((GameVersion)pk.Version, pk.Context);
        metlocationpicker.ItemDisplayBinding = new Binding("Text");
        eggmetpicker.ItemsSource = (System.Collections.IList)GameInfo.GetLocationList((GameVersion)sav.Version, sav.Context, true);
        eggmetpicker.ItemDisplayBinding = new Binding("Text");
        ballpicker.ItemsSource = Enum.GetValues(typeof(Ball));

        ICommand refreshCommand = new Command(() =>
        {
            if(pk.Species !=0)
                applymetinfo(pk);
            metrefresh.IsRefreshing = false;
        });
        metrefresh.Command = refreshCommand;
        if(pk.Species !=0)
            applymetinfo(pk);
    }
    public static string ballspriteurl;
    public void applymetinfo(PKM pkm)
    {
        if (pkm.IsShiny)
            shinysparklessprite.IsVisible = true;
        else
            shinysparklessprite.IsVisible = false;
        mettabpic.Source = spriteurl;
        origingamepicker.SelectedIndex = pkm.Version > -1?pkm.Version:0;
        if (pkm is IBattleVersion bv)
        {
            battleversionlabel.IsVisible = true;
            battleversionpicker.IsVisible = true;
            battleversionpicker.SelectedIndex = bv.BattleVersion;
        }
        
        metlocationpicker.SelectedItem = GameInfo.GetLocationList((GameVersion)pk.Version, pk.Context).Where(z=>z.Value == pkm.Met_Location).FirstOrDefault();
        ballpicker.SelectedIndex = pkm.Ball>-1?pkm.Ball:0;
        ballspriteurl = $"{(pkm.Ball>-1?$"{pkm.Ball}":"ball4")}.png";
        ballimage.Source = ballspriteurl;
       
        var metdate = pkm.MetDate!=null? (DateOnly)pkm.MetDate:DateOnly.MinValue;
        metdatepicker.Date = pkm.MetDate != null?metdate.ToDateTime(TimeOnly.Parse("10:00 PM")) :DateTime.Now;
        metleveldisplay.Text = pkm.Met_Level>-1?pkm.Met_Level.ToString():"0";
        if (pkm is IObedienceLevel ob)
        {
            obediencelevellabel.IsVisible = true;
            obedienceleveldisplay.IsVisible = true;
            obedienceleveldisplay.Text = ob.Obedience_Level.ToString();
        }
        fatefulcheck.IsChecked = pkm.FatefulEncounter;
        eggcheck.IsChecked = pkm.WasEgg;
        
        var eggmetdate = pkm.EggMetDate!=null? (DateOnly)pkm.EggMetDate:DateOnly.MinValue;
        eggdatepicker.Date = pkm.EggMetDate != null?eggmetdate.ToDateTime(TimeOnly.Parse("10:00 PM")):DateTime.Now;
        if(pkm.Egg_Location > -1)
        {
            eggmetpicker.SelectedItem = GameInfo.GetLocationList((GameVersion)sav.Version, sav.Context,true).Where(z => z.Value == pkm.Egg_Location).FirstOrDefault();
        }
        
    }

    public void applyorigingame(object sender, EventArgs e)
    {
        pk.Version = origingamepicker.SelectedIndex;

    }

    private void applybattleversion(object sender, EventArgs e)
    {
        if(pk is IBattleVersion bv)
            bv.BattleVersion = (byte)battleversionpicker.SelectedIndex;
    }

    private void applymetlocation(object sender, EventArgs e)
    {
        var metlocation = (ComboItem)metlocationpicker.SelectedItem;
        pk.Met_Location = metlocation.Value;
    }

    private void givebackballs(object sender, EventArgs e)
    {

        pk.Ball = ballpicker.SelectedIndex;
        ballspriteurl = $"{((Ball)pk.Ball).ToString().ToLower()}.png";
        ballimage.Source = ballspriteurl;
    }

    private void applymetdate(object sender, DateChangedEventArgs e)
    {
        pk.MetDate = DateOnly.FromDateTime( metdatepicker.Date);
    }

    private void applymetlevel(object sender, TextChangedEventArgs e)
    {
        if(metleveldisplay.Text.Length > 0)
        {
            pk.Met_Level = int.Parse(metleveldisplay.Text);
        }
    }

    private void applyobediencelevel(object sender, TextChangedEventArgs e)
    {
        if (pk is IObedienceLevel ob)
        {
            if (obedienceleveldisplay.Text.Length > 0)
                ob.Obedience_Level = (byte)int.Parse(obedienceleveldisplay.Text);
        }
    }

    private void applyfateful(object sender, CheckedChangedEventArgs e)
    {
        pk.FatefulEncounter = fatefulcheck.IsChecked;
    }

  

    private void applyeggmetlocation(object sender, EventArgs e)
    {
        var egglocation = (ComboItem)eggmetpicker.SelectedItem;
        pk.Egg_Location = egglocation.Value;
    }

    private void applyeggdate(object sender, DateChangedEventArgs e)
    {
        pk.EggMetDate = DateOnly.FromDateTime(eggdatepicker.Date);
    }

    private void wasegg(object sender, CheckedChangedEventArgs e)
    {
        if(eggcheck.IsChecked)
        {
            pk.Egg_Location = EncounterSuggestion.GetSuggestedEncounterEggLocationEgg(pk, true);
            pk.EggMetDate = DateOnly.FromDayNumber(DateTime.Now.Day);
        }
        else
        {
            pk.Egg_Location = LocationEdits.GetNoneLocation(pk);
            pk.EggMetDate = null;
        }
    }
}

