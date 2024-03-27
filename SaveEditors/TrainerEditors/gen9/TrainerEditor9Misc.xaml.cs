using PKHeX.Core;
using static PKHeX.Core.SaveBlockAccessor9SV;
namespace PKHeXMAUI;

public partial class TrainerEditor9Misc : ContentPage
{
	public SAV9SV SAV = (SAV9SV)MainPage.sav;
	public TrainerEditor9Misc()
	{
		InitializeComponent();
		TrainerXCoordinateEditor.Text = SAV.X.ToString();
		TrainerYCoordinateEditor.Text = SAV.Y.ToString();
		TrainerZCoordinateEditor.Text = SAV.Z.ToString();
		TrainerRotationEditor.Text = (Math.Atan2(SAV.RZ, SAV.RW) * 360.0 / Math.PI).ToString();


    }

    private void UnlockFlyLocations(object sender, EventArgs e)
    {
       
        var accessor = SAV.Accessor;
        foreach (var hash in FlyHashes)
        {
            if (accessor.TryGetBlock(hash, out var block))
                block.ChangeBooleanType(SCTypeCode.Bool2);
        }
        DisplayAlert("Fly", "All Fly Locations Unlocked", "cancel");
        AllFlyButton.IsEnabled = false;
    }
    private static ReadOnlySpan<uint> FlyHashes =>
   [
       #region Fly Flags
       FSYS_YMAP_FLY_01,
       FSYS_YMAP_FLY_02,
       FSYS_YMAP_FLY_03,
       FSYS_YMAP_FLY_04,
       FSYS_YMAP_FLY_05,
       FSYS_YMAP_FLY_06,
       FSYS_YMAP_FLY_07,
       FSYS_YMAP_FLY_08,
       FSYS_YMAP_FLY_09,
       FSYS_YMAP_FLY_10,
       FSYS_YMAP_FLY_11,
       FSYS_YMAP_FLY_12,
       FSYS_YMAP_FLY_13,
       FSYS_YMAP_FLY_14,
       FSYS_YMAP_FLY_15,
       FSYS_YMAP_FLY_16,
       FSYS_YMAP_FLY_17,
       FSYS_YMAP_FLY_18,
       FSYS_YMAP_FLY_19,
       FSYS_YMAP_FLY_20,
       FSYS_YMAP_FLY_21,
       FSYS_YMAP_FLY_22,
       FSYS_YMAP_FLY_23,
       FSYS_YMAP_FLY_24,
       FSYS_YMAP_FLY_25,
       FSYS_YMAP_FLY_26,
       FSYS_YMAP_FLY_27,
       FSYS_YMAP_FLY_28,
       FSYS_YMAP_FLY_29,
       FSYS_YMAP_FLY_30,
       FSYS_YMAP_FLY_31,
       FSYS_YMAP_FLY_32,
       FSYS_YMAP_FLY_33,
       FSYS_YMAP_FLY_34,
       FSYS_YMAP_FLY_35,
       FSYS_YMAP_FLY_MAGATAMA,
       FSYS_YMAP_FLY_MOKKAN,
       FSYS_YMAP_FLY_TSURUGI,
       FSYS_YMAP_FLY_UTSUWA,
       FSYS_YMAP_POKECEN_02,
       FSYS_YMAP_POKECEN_03,
       FSYS_YMAP_POKECEN_04,
       FSYS_YMAP_POKECEN_05,
       FSYS_YMAP_POKECEN_06,
       FSYS_YMAP_POKECEN_07,
       FSYS_YMAP_POKECEN_08,
       FSYS_YMAP_POKECEN_09,
       FSYS_YMAP_POKECEN_10,
       FSYS_YMAP_POKECEN_11,
       FSYS_YMAP_POKECEN_12,
       FSYS_YMAP_POKECEN_13,
       FSYS_YMAP_POKECEN_14,
       FSYS_YMAP_POKECEN_15,
       FSYS_YMAP_POKECEN_16,
       FSYS_YMAP_POKECEN_17,
       FSYS_YMAP_POKECEN_18,
       FSYS_YMAP_POKECEN_19,
       FSYS_YMAP_POKECEN_20,
       FSYS_YMAP_POKECEN_21,
       FSYS_YMAP_POKECEN_22,
       FSYS_YMAP_POKECEN_23,
       FSYS_YMAP_POKECEN_24,
       FSYS_YMAP_POKECEN_25,
       FSYS_YMAP_POKECEN_26,
       FSYS_YMAP_POKECEN_27,
       FSYS_YMAP_POKECEN_28,
       FSYS_YMAP_POKECEN_29,
       FSYS_YMAP_POKECEN_30,
       FSYS_YMAP_POKECEN_31,
       FSYS_YMAP_POKECEN_32,
       FSYS_YMAP_POKECEN_33,
       FSYS_YMAP_POKECEN_34,
       FSYS_YMAP_POKECEN_35,

       // Treasures of Ruin shrine toggles
       FSYS_YMAP_MAGATAMA,
       FSYS_YMAP_MOKKAN,
       FSYS_YMAP_TSURUGI,
       FSYS_YMAP_UTSUWA,

       // Sudachi 1
       FSYS_YMAP_SU1MAP_CHANGE, // can change map to Kitakami
       FSYS_YMAP_FLY_SU1_AREA10,
       FSYS_YMAP_FLY_SU1_BUSSTOP,
       FSYS_YMAP_FLY_SU1_CENTER01,
       FSYS_YMAP_FLY_SU1_PLAZA,
       FSYS_YMAP_FLY_SU1_SPOT01,
       FSYS_YMAP_FLY_SU1_SPOT02,
       FSYS_YMAP_FLY_SU1_SPOT03,
       FSYS_YMAP_FLY_SU1_SPOT04,
       FSYS_YMAP_FLY_SU1_SPOT05,
       FSYS_YMAP_FLY_SU1_SPOT06,

       // Sudachi 2
       FSYS_YMAP_S2_MAPCHANGE_ENABLE, // can change map to Blueberry Academy
       FSYS_YMAP_FLY_SU2_DRAGON,
       FSYS_YMAP_FLY_SU2_ENTRANCE,
       FSYS_YMAP_FLY_SU2_FAIRY,
       FSYS_YMAP_FLY_SU2_HAGANE,
       FSYS_YMAP_FLY_SU2_HONOO,
       FSYS_YMAP_FLY_SU2_SPOT01,
       FSYS_YMAP_FLY_SU2_SPOT02,
       FSYS_YMAP_FLY_SU2_SPOT03,
       FSYS_YMAP_FLY_SU2_SPOT04,
       FSYS_YMAP_FLY_SU2_SPOT05,
       FSYS_YMAP_FLY_SU2_SPOT06,
       FSYS_YMAP_FLY_SU2_SPOT07,
       FSYS_YMAP_FLY_SU2_SPOT08,
       FSYS_YMAP_FLY_SU2_SPOT09,
       FSYS_YMAP_FLY_SU2_SPOT10,
       FSYS_YMAP_FLY_SU2_SPOT11,
       FSYS_YMAP_POKECEN_SU02,
       #endregion
   ];

    private void CollectStakes(object sender, EventArgs e)
    {
        SAV.CollectAllStakes();
        StakesButton.IsEnabled = false;
        DisplayAlert("Stakes", "All Stakes Collected", "cancel");
    }

    private void UnlockTMRecipes(object sender, EventArgs e)
    {
        SAV.UnlockAllTMRecipes();
        TMRecipeButton.IsEnabled = false;
        DisplayAlert("TMs", "All TM Recipes Unlocked", "cancel");
    }

    private void UnlockBike(object sender, EventArgs e)
    {
        string[] blocks =
         [
             "FSYS_RIDE_DASH_ENABLE",
             "FSYS_RIDE_SWIM_ENABLE",
             "FSYS_RIDE_HIJUMP_ENABLE",
             "FSYS_RIDE_GLIDE_ENABLE",
             "FSYS_RIDE_CLIMB_ENABLE",
         ];

        var accessor = SAV.Accessor;
        foreach (var block in blocks)
            accessor.GetBlock(block).ChangeBooleanType(SCTypeCode.Bool2);
        if (accessor.TryGetBlock("FSYS_RIDE_FLIGHT_ENABLE", out var fly))
            fly.ChangeBooleanType(SCTypeCode.Bool2); // Base & DLC1 saves do not have this block
        BikeButton.IsEnabled = false;
        DisplayAlert("Bike", "All Bike Modes Unlocked", "cancel");
    }

    private void UnlockFashion(object sender, EventArgs e)
    {
        var accessor = SAV.Accessor;
        PlayerFashionUnlock9.UnlockBase(accessor, SAV.Gender);
        FashionButton.IsEnabled = false;
        DisplayAlert("Fashion", "All Fashion Unlocked", "cancel");
    }
}