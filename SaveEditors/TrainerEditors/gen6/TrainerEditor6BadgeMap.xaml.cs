
using PKHeX.Core;

namespace PKHeXMAUI;

public partial class TrainerEditor6BadgeMap : ContentPage
{
    private readonly CheckBox[] cba;
    public SAV6 SAV = (SAV6)MainPage.sav;
    public TrainerEditor6BadgeMap()
	{
		InitializeComponent();
        cba = [Badge1, Badge2, Badge3, Badge4, Badge5, Badge6, Badge7, Badge8];
        int badgeval = SAV.Badges;
        for (int i = 0; i < 8; i++)
            cba[i].IsChecked = (badgeval & (1 << i)) != 0;
        var sit = SAV.Situation;
        CurrentMapEntry.Number = sit.M;
        RotationEntry.Number = sit.R;
        try
        {
            XCoordEntry.Number = (decimal)(sit.X / 18.0);
            YCoordEntry.Number = (decimal)(sit.Y / 18.0);
            ZCoordEntry.Number = (decimal)(sit.Z / 18.0);
        }
        catch (Exception) { MapGrid.IsVisible = false; }
    }
    public void SaveBadgeMap()
    {
        var sit = SAV.Situation;
        sit.M = (int)CurrentMapEntry.Number;
        sit.X = (int)XCoordEntry.Number * 18;
        sit.Y = (int)YCoordEntry.Number * 18;
        sit.Z = (int)ZCoordEntry.Number * 18;
        sit.R = (int)RotationEntry.Number;
        int badgeval = 0;
        for (int i = 0; i < 8; i++)
            badgeval |= (cba[i].IsChecked ? 1 : 0) << i;
        SAV.Badges = badgeval;
    }
}