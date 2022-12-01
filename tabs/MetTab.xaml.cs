

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
        ICommand refreshCommand = new Command(() =>
        {
            mettabpic.Source = spriteurl;
            origingamepicker.SelectedIndex = pk.Version;
            battleversionpicker.SelectedIndex = pk.BattleVersion;
            metlocationpicker.SelectedIndex = pk.Met_Location;
            metrefresh.IsRefreshing = false;
        });
        metrefresh.Command = refreshCommand;
        origingamepicker.SelectedIndex = pk.Version;
        battleversionpicker.SelectedIndex=pk.BattleVersion;
        metlocationpicker.SelectedIndex = pk.Met_Location;
    }

 

    private void applyorigingame(object sender, EventArgs e)
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
}

