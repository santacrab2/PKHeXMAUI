

using System.ComponentModel;
using System.Drawing.Drawing2D;
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
        metlocationpicker.ItemsSource = GameInfo.Strings.GetLocationNames(9, GameVersion.SV).ToArray();
        eggmetpicker.ItemsSource = GameInfo.Strings.GetLocationNames(9,GameVersion.SV).ToArray();
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
    public void applymetinfo(PK9 pkm)
    {
        mettabpic.Source = spriteurl;
        origingamepicker.SelectedIndex = pkm.Version > -1?pkm.Version:0;
        battleversionpicker.SelectedIndex = pkm.BattleVersion>-1?pkm.BattleVersion:0;
        metlocationpicker.SelectedIndex = pkm.Met_Location>-1?pkm.Met_Location:0;
        ballpicker.SelectedIndex = pkm.Ball>-1?pkm.Ball:0;
        ballspriteurl = $"https://raw.githubusercontent.com/santacrab2/Resources/main/Pokeballs/{(pkm.Ball>-1?(Ball)pkm.Ball:"Poke")}.png";
        ballimage.Source = ballspriteurl;
        var metdate = (DateOnly)pkm.MetDate;
        metdatepicker.Date = pkm.MetDate != null?metdate.ToDateTime(TimeOnly.Parse("10:00 PM")) :DateTime.Now;
        metleveldisplay.Text = pkm.Met_Level>-1?pkm.Met_Level.ToString():"0";
        obedienceleveldisplay.Text = pkm.Obedience_Level>-1?pkm.Obedience_Level.ToString():"0";
        fatefulcheck.IsChecked = pkm.FatefulEncounter;
        eggcheck.IsChecked = pkm.WasEgg;
        var eggmetdate = (DateOnly)pkm.EggMetDate;
        eggdatepicker.Date = pkm.EggMetDate != null?eggmetdate.ToDateTime(TimeOnly.Parse("10:00 PM")):DateTime.UnixEpoch;
        eggmetpicker.SelectedIndex = pkm.Egg_Location > -1 ? pkm.Egg_Location:0;
        
    }

    public void applyorigingame(object sender, EventArgs e)
    {
        pk.Version = origingamepicker.SelectedIndex;

    }

    private void applybattleversion(object sender, EventArgs e)
    {
        pk.BattleVersion = (byte)battleversionpicker.SelectedIndex;
    }

    private void applymetlocation(object sender, EventArgs e)
    {
        pk.Met_Location= (byte)metlocationpicker.SelectedIndex;
    }

    private void givebackballs(object sender, EventArgs e)
    {

        pk.Ball = ballpicker.SelectedIndex;
        ballspriteurl = $"https://raw.githubusercontent.com/santacrab2/Resources/main/Pokeballs/{(Ball)pk.Ball}.png";
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
        if (obedienceleveldisplay.Text.Length > 0)
            pk.Obedience_Level = (byte)int.Parse(obedienceleveldisplay.Text);
    }

    private void applyfateful(object sender, CheckedChangedEventArgs e)
    {
        pk.FatefulEncounter = fatefulcheck.IsChecked;
    }

  

    private void applyeggmetlocation(object sender, EventArgs e)
    {
        pk.Egg_Location = eggmetpicker.SelectedIndex;
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

