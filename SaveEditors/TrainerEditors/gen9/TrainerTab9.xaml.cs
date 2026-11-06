
using PKHeX.Core;

namespace PKHeXMAUI;

public partial class TrainerTab9 : TabbedPage
{
    public static TrainerEditor9? TE9;
    public static TrainerEditor9Blueberry? TE9B;
    public static TrainerEditor9Misc? TE9M;
	public TrainerTab9()
	{
		InitializeComponent();
        TE9 = new();
        TE9B = new();
        TE9M = new();
        trainertab9.BarBackgroundColor = Color.FromArgb("303030");
        trainertab9.BarTextColor = Colors.White;
        trainertab9.Children.Add(TE9);
        trainertab9.Children.Add(TE9M);
        trainertab9.Children.Add(new TrainerImages());
        if (((SAV9SV)MainPage.sav).SaveRevision >= 2)
            trainertab9.Children.Add(TE9B);
        trainertab9.Children.Add(new SavePage9());
        trainertab9.Children.Add(new cancelpage());
    }
}

public partial class cancelpage : ContentPage
{
    public cancelpage()
    {
        this.Title = L10n.T("box_cancel");
    }
    protected override void OnNavigatedTo(NavigatedToEventArgs e)
    {
        Navigation.PopModalAsync();
    }
}

public partial class SavePage9 : ContentPage
{
    public SavePage9()
    {
        this.Title = L10n.T("common_save");
    }
    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        TrainerTab9.TE9?.SaveTrainerEditor9();
        TrainerTab9.TE9M?.SaveTEMisc();
        if (((SAV9SV)MainPage.sav).SaveRevision >= 2)
            TrainerTab9.TE9B?.SaveTE9Blueberry();
        Navigation.PopModalAsync();
    }
}