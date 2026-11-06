
using PKHeX.Core;

namespace PKHeXMAUI;

public partial class TrainerTab7 : TabbedPage
{
	public static TrainerEditor7? TE7;
	public static TrainerEditor7Map? TE7M;
	public static TrainerEditor7BattleTree? TE7BT;
	public static TrainerEditor7Misc? TE7Mi;
	public static TrainerEditor7Ultra? TE7U;
	public TrainerTab7()
	{
		InitializeComponent();
		TE7 = new();
		TE7M = new();
		TE7BT = new();
		TE7Mi = new();
		TE7U = new();
        trainertab7.BarBackgroundColor = Color.FromArgb("303030");
        trainertab7.BarTextColor = Colors.White;
        trainertab7.Children.Add(TE7);
		trainertab7.Children.Add(TE7M);
		trainertab7.Children.Add(TE7BT);
		trainertab7.Children.Add(TE7Mi);
		if (MainPage.sav is SAV7USUM)
			trainertab7.Children.Add(TE7U);
		trainertab7.Children.Add(new cancelpage());
		trainertab7.Children.Add(new SavePage7());
	}
}

public partial class SavePage7 : ContentPage
{
    public SavePage7()
    {
        this.Title = L10n.T("common_save");
    }
    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        TrainerTab7.TE7?.SaveTE7();
		TrainerTab7.TE7M?.SaveTE7M();
		TrainerTab7.TE7BT?.SaveTE7B();
		TrainerTab7.TE7Mi?.SaveTE7Mi();
		if (MainPage.sav is SAV7USUM)
			TrainerTab7.TE7U?.SaveTE7U();
        Navigation.PopModalAsync();
    }
}