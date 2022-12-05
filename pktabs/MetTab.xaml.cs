

using System.Drawing.Drawing2D;
using System.Windows.Input;
using PKHeX.Core;
using static System.Net.WebRequestMethods;
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

    public void applymetinfo(PK9 pkm)
    {
        mettabpic.Source = spriteurl;
        origingamepicker.SelectedIndex = pkm.Version > -1?pkm.Version:0;
        battleversionpicker.SelectedIndex = pkm.BattleVersion>-1?pkm.BattleVersion:0;
        metlocationpicker.SelectedIndex = pkm.Met_Location>-1?pkm.Met_Location:0;
        ballpicker.SelectedIndex = pkm.Ball>-1?pkm.Ball:0;
        ballimage.Source = $"https://raw.githubusercontent.com/santacrab2/Resources/main/Pokeballs/{(pkm.Ball>-1?(Ball)pkm.Ball:"Poke")}.png";
        metdatepicker.Date = pkm.MetDate != null?(DateTime)pkm.MetDate:DateTime.Now;
        metleveldisplay.Text = pkm.Met_Level>-1?pkm.Met_Level.ToString():"0";
        obedienceleveldisplay.Text = pkm.Obedience_Level>-1?pkm.Obedience_Level.ToString():"0";
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
        ballimage.Source = $"https://raw.githubusercontent.com/santacrab2/Resources/main/Pokeballs/{(Ball)pk.Ball}.png";
    }

    private void applymetdate(object sender, DateChangedEventArgs e)
    {
        pk.MetDate = metdatepicker.Date;
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
}

