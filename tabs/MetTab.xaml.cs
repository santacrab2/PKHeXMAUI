

using System.Windows.Input;
using PKHeX.Core;

namespace pk9reader;

public partial class MetTab : ContentPage
{
	public MetTab()
	{
		InitializeComponent();
        mettabpic.Source = MainPage.spriteurl;
        origingamepicker.ItemsSource = GameInfo.Strings.gamelist;
        ICommand refreshCommand = new Command(() =>
        {
            mettabpic.Source = MainPage.spriteurl;
            metrefresh.IsRefreshing = false;
        });
        metrefresh.Command = refreshCommand;
    }
    
    public void applymetpkinfo (PK9 pkm)
    {
        
    }

    private void applyorigingame(object sender, EventArgs e)
    {
        og.Text = origingamepicker.SelectedIndex.ToString();
    }
}

