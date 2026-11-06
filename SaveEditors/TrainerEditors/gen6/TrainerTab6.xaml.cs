
using PKHeX.Core;

namespace PKHeXMAUI;

public partial class TrainerTab6 : TabbedPage
{
    public SAV6 SAV = (SAV6)MainPage.sav;
    public static TrainerEditor6? TE6;
    public static TrainerEditor6BadgeMap? TEBM6;
    public static TrainerEditor6Maison? TEM6;
    public static TrainerEditor6Multiplayer? TEMU6;
    public static TrainerEditor6Appearance? TEA6;
	public TrainerTab6()
	{
		InitializeComponent();
        TE6 = new();
        TEBM6 = new();
        TEM6 = new();
        TEMU6 = new();
        TEA6 = new();
        trainertab6.BarBackgroundColor = Color.FromArgb("303030");
        trainertab6.BarTextColor = Colors.White;
        trainertab6.Children.Add(TE6);
        trainertab6.Children.Add(TEBM6);
        if (SAV is not SAV6AODemo)
        {
            trainertab6.Children.Add(TEMU6);
            trainertab6.Children.Add(TEM6);
        }
        if (SAV is SAV6XY)
            trainertab6.Children.Add(TEA6);
        trainertab6.Children.Add(new SavePage6());
        trainertab6.Children.Add(new cancelpage());
    }
}
public partial class SavePage6 : ContentPage
{
    public SavePage6()
    {
        this.Title = L10n.T("common_save");
    }
    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        TrainerTab6.TE6?.SaveTE6();
        TrainerTab6.TEM6?.SaveMaison();
        TrainerTab6.TEBM6?.SaveBadgeMap();
        TrainerTab6.TEMU6?.SaveMultiplayer();
        TrainerTab6.TEA6?.SaveAppearance();
        Navigation.PopModalAsync();
    }
}