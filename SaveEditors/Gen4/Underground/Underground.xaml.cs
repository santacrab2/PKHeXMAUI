using PKHeX.Core;

namespace PKHeXMAUI;

public partial class Underground : ContentPage
{
    private readonly SaveFile Origin;
    private readonly SAV4Sinnoh SAV;
    public Underground(SAV4Sinnoh sav)
	{
		InitializeComponent();
        SAV = (SAV4Sinnoh)(Origin = sav).Clone();
        GetUGScores();
    }

    private void GetUGScores()
    {
        LoadValue(NUD_PlayersMet, SAV.UG_PeopleMet);
        LoadValue(NUD_GiftsGiven, SAV.UG_GiftsGiven);
        LoadValue(NUD_GiftsReceived, SAV.UG_GiftsReceived);
        LoadValue(NUD_Spheres, SAV.UG_Spheres);
        LoadValue(NUD_Fossils, SAV.UG_Fossils);
        LoadValue(NUD_TrapPlayers, SAV.UG_TrapPlayers);
        LoadValue(NUD_TrapSelf, SAV.UG_TrapSelf);
        LoadValue(NUD_MyBaseMoved, SAV.UG_MyBaseMoved);
        LoadValue(NUD_FlagsObtained, SAV.UG_FlagsTaken);
        LoadValue(NUD_MyFlagTaken, SAV.UG_FlagsFromMe);
        LoadValue(NUD_MyFlagRecovered, SAV.UG_FlagsRecovered);
        LoadValue(NUD_FlagsCaptured, SAV.UG_FlagsCaptured);
        LoadValue(NUD_HelpedOthers, SAV.UG_HelpedOthers);

        static void LoadValue(NumericUpDown box, uint value)
            => box.Number = Math.Clamp(value, 0, SAV4Sinnoh.UG_MAX);
    }
    private void SetUGScores()
    {
        SAV.UG_PeopleMet = (uint)NUD_PlayersMet.Number;
        SAV.UG_GiftsGiven = (uint)NUD_GiftsGiven.Number;
        SAV.UG_GiftsReceived = (uint)NUD_GiftsReceived.Number;
        SAV.UG_Spheres = (uint)NUD_Spheres.Number;
        SAV.UG_Fossils = (uint)NUD_Fossils.Number;
        SAV.UG_TrapPlayers = (uint)NUD_TrapPlayers.Number;
        SAV.UG_TrapSelf = (uint)NUD_TrapSelf.Number;
        SAV.UG_MyBaseMoved = (uint)NUD_MyBaseMoved.Number;
        SAV.UG_FlagsTaken = (uint)NUD_FlagsObtained.Number;
        SAV.UG_FlagsFromMe = (uint)NUD_MyFlagTaken.Number;
        SAV.UG_FlagsRecovered = (uint)NUD_MyFlagRecovered.Number;
        SAV.UG_FlagsCaptured = (uint)NUD_FlagsCaptured.Number;
        SAV.UG_HelpedOthers = (uint)NUD_HelpedOthers.Number;
    }
}

public partial class UndergroundTab : TabbedPage
{
    public static Underground underground = new((SAV4Sinnoh)MainPage.sav);
    public static UndergroundGoods undergroundGoods = new((SAV4Sinnoh)MainPage.sav);
    public UndergroundTab()
	{
        BarBackgroundColor = Microsoft.Maui.Graphics.Color.FromArgb("303030");
        BarTextColor = Colors.White;
        Children.Add(underground);
        Children.Add(undergroundGoods);
    }
}