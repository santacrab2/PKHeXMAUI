

using System.Windows.Input;

namespace pk9reader;

public partial class MetTab : ContentPage
{
	public MetTab()
	{
		InitializeComponent();
        mettabpic.Source = MainPage.spriteurl;
        ICommand refreshCommand = new Command(() =>
        {
            mettabpic.Source = MainPage.spriteurl;
            metrefresh.IsRefreshing = false;
        });
        metrefresh.Command = refreshCommand;
    }
	
}