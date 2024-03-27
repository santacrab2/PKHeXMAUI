using PKHeX.Core;

namespace PKHeXMAUI;

public partial class TrainerTab9 : TabbedPage
{
	public TrainerTab9()
	{
		InitializeComponent();
        trainertab9.BarBackgroundColor = Color.FromArgb("303030");
        trainertab9.BarTextColor = Colors.White;
        trainertab9.Children.Add(new TrainerEditor9());
        trainertab9.Children.Add(new TrainerEditor9Misc());
        trainertab9.Children.Add(new TrainerImages());
        if (((SAV9SV)MainPage.sav).SaveRevision >= 2)
            trainertab9.Children.Add(new TrainerEditor9Blueberry());
    }
}