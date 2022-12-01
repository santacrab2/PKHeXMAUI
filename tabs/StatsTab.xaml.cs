using System.Windows.Input;
using PKHeX.Core;
using static pk9reader.MainPage;

namespace pk9reader;

public partial class StatsTab : ContentPage
{
	public StatsTab()
	{
		InitializeComponent();
        ICommand refreshCommand = new Command(() =>
        {
            if (pk.Species != 0)
                applystatsinfo(pk);
            statrefresh.IsRefreshing = false;
        });
        statrefresh.Command = refreshCommand;
        applystatsinfo(pk);
    }

	public void applystatsinfo(PK9 pkm)
	{
        statpic.Source = spriteurl;

	}
}