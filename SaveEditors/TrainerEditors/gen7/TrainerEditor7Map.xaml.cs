
using PKHeX.Core;
using System.Collections.ObjectModel;

namespace PKHeXMAUI;

public partial class TrainerEditor7Map : ContentPage
{
    public SAV7 SAV = (SAV7)MainPage.sav;
    public ObservableCollection<WhyYouNoTakeGenericsDataTemplate> FlyMapData;
    public ObservableCollection<WhyYouNoTakeGenericsDataTemplate> UnmaskData;
    private int[] FlyDestFlagOfs = null!, MapUnmaskFlagOfs = null!;
     private int SkipFlag => SAV is SAV7USUM ? 4160 : 3200; // FlagMax - 768
    public TrainerEditor7Map()
	{
		InitializeComponent();
        FlyMapData = [];
        UnmaskData = [];
        var sit = SAV.Situation;
        CurrentMapEntry.Number = sit.M;
        RotationEntry.Number = (decimal)(Math.Atan2(SAV.Situation.RZ, SAV.Situation.RW) * 360.0 / Math.PI);

        XCoordEntry.Number = (decimal)(sit.X / 60.0);
        YCoordEntry.Number = (decimal)(sit.Y / 60.0);
        ZCoordEntry.Number = (decimal)(sit.Z / 60.0);
        FlyDestinationCV.ItemTemplate = new DataTemplate(() =>
        {
            Grid grid = new() { Padding = 10 };
            grid.AddColumnDefinition(new ColumnDefinition() { Width = GridLength.Star });
            grid.AddColumnDefinition(new ColumnDefinition() { Width = GridLength.Star });
            CheckBox flycheck = new();
            flycheck.SetBinding(CheckBox.IsCheckedProperty, "check", BindingMode.TwoWay);
            Label flytext = new();
            flytext.SetBinding(Label.TextProperty, "Text");
            grid.Add(flycheck);
            grid.Add(flytext, 1);
            return grid;
        });
        MapUnmaskCV.ItemTemplate = new DataTemplate(() =>
        {
            Grid grid = new() { Padding = 10 };
            grid.AddColumnDefinition(new ColumnDefinition() { Width = GridLength.Star });
            grid.AddColumnDefinition(new ColumnDefinition() { Width = GridLength.Star });
            CheckBox flycheck = new();
            flycheck.SetBinding(CheckBox.IsCheckedProperty, "check", BindingMode.TwoWay);
            Label flytext = new();
            flytext.SetBinding(Label.TextProperty, "Text");
            grid.Add(flycheck);
            grid.Add(flytext, 1);
            return grid;
        });
        GetFlyMapData();
    }

    private void UnlockAllFly(object sender, EventArgs e)
    {
        for (int i = 0; i < FlyMapData.Count; i++)
            FlyMapData[i].check = true;
    }
    private void UnmaskAll(object sender, EventArgs e)
    {
        for (int i = 0; i < UnmaskData.Count; i++)
            UnmaskData[i].check = true;
    }
    public void GetFlyMapData()
    {
        var metLocationList = GameInfo.GetLocationList(GameVersion.US, EntityContext.Gen7, false);
        int[] FlyDestNameIndex = [
            -1,24,34,8,20,38,12,46,40,30,//Melemele
            70,68,78,86,74,104,82,58,90,72,76,92,62,//Akala
            132,136,138,114,118,144,130,154,140,//Ula'ula
            172,184,180,174,176,156,186,//Poni
            188,-1,-1,
            198,202,110,204,//Beach
        ];
        if (SAV.Version is GameVersion.UM or GameVersion.MN)
        {
            FlyDestNameIndex[28] = 142;
            FlyDestNameIndex[36] = 178;
        }
        FlyDestFlagOfs = [
            44,43,45,40,41,49,42,47,46,48,
            50,54,39,57,51,55,59,52,58,53,61,60,56,
            62,66,67,64,65,273,270,37,38,
            69,74,72,71,276,73,70,
            75,332,334,
            331,333,335,336,
        ];
        string[] FlyDestAltName = ["My House", "Photo Club (Hau'oli)", "Photo Club (Konikoni)"];
        for (int i = 0, u = 0, m = FlyDestNameIndex.Length - (SAV is SAV7USUM ? 0 : 6); i < m; i++)
        {
            var dest = FlyDestNameIndex[i];
            var name = dest < 0 ? FlyDestAltName[u++] : metLocationList.FirstOrDefault(v => v.Value == dest)?.Text??"";
            var state = SAV.EventWork.GetEventFlag(SkipFlag + FlyDestFlagOfs[i]);
            FlyMapData.Add(new WhyYouNoTakeGenericsDataTemplate(name, state));
        }
        FlyDestinationCV.ItemsSource = FlyMapData;
        int[] MapUnmaskNameIndex = [
           6,8,24,-1,18,-1,20,22,12,10,14,
            70,50,68,52,74,54,56,58,60,72,62,64,
            132,192,106,108,122,112,114,126,116,118,120,154,
            172,158,160,162,164,166,168,170,
            188,
            198,202,110,204,
        ];
        MapUnmaskFlagOfs = [
            5,76,82,91,79,84,80,81,77,78,83,
            19,10,18,11,21,12,13,14,15,20,16,17,
            33,34,30,31,98,92,93,94,95,96,97,141,
            173,144,145,146,147,148,149,172,
            181,
            409,297,32,296,
        ];
        string[] MapUnmaskAltName = ["Melemele Sea (East)", "Melemele Sea (West)"];
        for (int i = 0, u = 0, m = MapUnmaskNameIndex.Length - (SAV is SAV7USUM ? 0 : 4); i < m; i++)
        {
            var dest = MapUnmaskNameIndex[i];
            var name = dest < 0 ? MapUnmaskAltName[u++] : metLocationList.FirstOrDefault(v => v.Value == dest)?.Text??"";
            var state = SAV.EventWork.GetEventFlag(SkipFlag + MapUnmaskFlagOfs[i]);
            UnmaskData.Add(new WhyYouNoTakeGenericsDataTemplate(name, state));
        }
        MapUnmaskCV.ItemsSource = UnmaskData;
    }

    public void SaveTE7M()
    {
        SAV.Situation.M = (int)CurrentMapEntry.Number;
        SAV.Situation.X = (int)XCoordEntry.Number * 60;
        SAV.Situation.Z = (int)ZCoordEntry.Number * 60;
        SAV.Situation.Y = (int)YCoordEntry.Number * 60;
        var result = (int)((double)RotationEntry.Number * Math.PI / 360.0);
        SAV.Situation.RX = 0;
        SAV.Situation.RZ = (float)Math.Sin(result);
        SAV.Situation.RY = 0;
        SAV.Situation.RW = (float)Math.Cos(result);
        SAV.Situation.UpdateOverworldCoordinates();
    }
}
public class WhyYouNoTakeGenericsDataTemplate(string s, bool b)
{
    public string Text { get; set; } = s;
    public bool check { get; set; } = b;
}