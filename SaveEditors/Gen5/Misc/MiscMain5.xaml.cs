using PKHeX.Core;
using System.Collections.ObjectModel;
using static System.Buffers.Binary.BinaryPrimitives;

namespace PKHeXMAUI;

public partial class MiscMain5 : ContentPage
{
    private int ofsFly;
    private readonly SAV5 SAV;
    private int[] FlyDestC = null!;
    private ObservableCollection<Tuple<string, bool>> FlyDestItems = [];
    private ObservableCollection<Tuple<string, bool>> UnlockedKeysItems = [];
    private comboBox[] cbr = null!;
    public MiscMain5(SAV5 sav)
	{
		InitializeComponent();
        SAV = sav;
        CV_Flydest.ItemTemplate = new(() =>
        {
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new(GridLength.Star));
            CheckBox cb = new();
            cb.SetBinding(CheckBox.IsCheckedProperty, "Item2", BindingMode.TwoWay);
            Label lb = new();
            lb.SetBinding(Label.TextProperty, "Item1");
            grid.Add(cb);
            grid.Add(lb, 1, 0);
            return grid;
        });
        CV_Keys.ItemTemplate = new(() =>
        {
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new(GridLength.Star));
            CheckBox cb = new();
            cb.SetBinding(CheckBox.IsCheckedProperty, "Item2", BindingMode.TwoWay);
            Label lb = new();
            lb.SetBinding(Label.TextProperty, "Item1");
            grid.Add(cb);
            grid.Add(lb, 1, 0);
            return grid;
        });
        ReadMain();
        ReadRecord();
    }
    private void ReadRecord()
    {
        var record = SAV.Records;
        NUD_Record16.MaxValue = Record5.Record16 - 1;
        NUD_Record32.MaxValue = Record5.Record32 - 1;
        NUD_Record16V.Number = record.GetRecord16(0);
        NUD_Record32V.Number = record.GetRecord32(0);
        NUD_Record16V.ValueChanged += (_, _) => record.SetRecord16((int)NUD_Record16.Number, (ushort)NUD_Record16V.Number);
        NUD_Record32V.ValueChanged += (_, _) => record.SetRecord32((int)NUD_Record32.Number, (uint)NUD_Record32V.Number);
        NUD_Record16.ValueChanged += (_, _) => NUD_Record16V.Number = record.GetRecord16((int)NUD_Record16.Number);
        NUD_Record32.ValueChanged += (_, _) => NUD_Record32V.Number = record.GetRecord32((int)NUD_Record32.Number);
    }
    public void SaveRecord() => SAV.Records.EndAccess();
    private void ReadMain()
    {
        string[]? FlyDestA;
        switch (SAV.Version)
        {
            case GameVersion.B or GameVersion.W or GameVersion.BW:
                ofsFly = 0x204B2;
                FlyDestA = [
                    "Nuvema Town", "Accumula Town", "Striaton City", "Nacrene City",
                    "Castelia City", "Nimbasa City", "Driftveil City", "Mistralton City",
                    "Icirrus City", "Opelucid City", "Victory Road", "Pokemon League",
                    "Lacunosa Town", "Undella Town", "Black City/White Forest", "(Unity Tower)",
                ];
                FlyDestC = [
                    0, 1, 2, 3,
                    4, 5, 6, 7,
                    8, 9, 15, 11,
                    10, 13, 12, 14,
                ];
                break;
            case GameVersion.B2 or GameVersion.W2 or GameVersion.B2W2:
                ofsFly = 0x20392;
                FlyDestA = [
                    "Aspertia City", "Floccesy Town", "Virbank City",
                    "Nuvema Town", "Accumula Town", "Striaton City", "Nacrene City",
                    "Castelia City", "Nimbasa City", "Driftveil City", "Mistralton City",
                    "Icirrus City", "Opelucid City",
                    "Lacunosa Town", "Undella Town", "Black City/White Forest",
                    "Lentimas Town", "Humilau City", "Victory Road", "Pokemon League",
                    "Pokestar Studios", "Join Avenue", "PWT", "(Unity Tower)",
                ];
                FlyDestC = [
                    24, 27, 25,
                    8, 9, 10, 11,
                    12, 13, 14, 15,
                    16, 17,
                    18, 21, 20,
                    28, 26, 66, 19,
                    5, 6, 7, 22,
                ];
                break;

            default: throw new ArgumentOutOfRangeException(nameof(SAV.Version));
        }
        uint valFly = ReadUInt32LittleEndian(SAV.Data[ofsFly..]);
        for (int i = 0; i < FlyDestA.Length; i++)
        {
            bool isSet;
            if (FlyDestC[i] < 32)
                isSet = (valFly & (1u << FlyDestC[i])) != 0;
            else
                isSet = (SAV.Data[ofsFly + (FlyDestC[i] >> 3)] & (1 << (FlyDestC[i] & 7))) != 0;
            FlyDestItems.Add(new Tuple<string, bool>(FlyDestA[i], isSet));
        }
        CV_Flydest.ItemsSource = FlyDestItems;
        if (SAV is SAV5BW bw)
        {
            cbr = [CB_Roamer642, CB_Roamer641];
            for (int i = 0; i < cbr.Length; i++)
            {
                byte c = bw.Encount.GetRoamerState(i);
                var states = GetStates();
                if (states.All(z => z.Value != c))
                    states.Add(new ComboItem($"Unknown (0x{c:X2})", c));
                cbr[i].Items.Clear();
                cbr[i].ItemSource = states.Where(v => v.Value >= 2 || v.Value == c).ToList();
                cbr[i].SelectedItem = (int)c;
            }
            {
                var current = bw.EventWork.GetWorkRoamer();
                var states = GetRoamStatusStates();
                if (states.All(z => z.Value != current))
                    states.Add(new ComboItem($"Unknown (0x{current:X2})", current));
                CB_RoamStatus.Items.Clear();
                CB_RoamStatus.ItemSource = states;
                CB_RoamStatus.SelectedItem = (int)current;
            }
            CHK_LibertyPass.IsChecked = bw.Misc.IsLibertyTicketActivated;
        }
        else if (SAV is SAV5B2W2 b2w2)
        {
            GB_Roamer.IsVisible = CHK_LibertyPass.IsVisible = L_LibertyPass.IsVisible = false;
            var keys = b2w2.Keys;
            // KeySystem
            string[] KeySystemA =
            [
                "Obtain EasyKey", "Obtain ChallengeKey", "Obtain CityKey", "Obtain IronKey", "Obtain IcebergKey",
                "Unlock EasyMode", "Unlock ChallengeMode", "Unlock City", "Unlock IronChamber", "Unlock IcebergChamber",
            ];
            for (int i = 0; i < 5; i++)
            {
                UnlockedKeysItems.Add(new Tuple<string, bool>(KeySystemA[i], keys.GetIsKeyObtained((KeyType5)i)));
                UnlockedKeysItems.Add(new Tuple<string, bool>(KeySystemA[i + 5], keys.GetIsKeyUnlocked((KeyType5)i)));
            }

            CV_Keys.ItemsSource = UnlockedKeysItems;
        }
        else { GB_KeySystem.IsVisible = GB_Roamer.IsVisible = CHK_LibertyPass.IsVisible = false; }
    }
    private static List<ComboItem> GetStates() =>
    [
        new("Not roamed", 0),
        new("Roaming", 1),
        new("Defeated", 2),
        new("Captured", 3),
    ];
    private static List<ComboItem> GetRoamStatusStates() =>
    [
        new("Not happened", 0),
        new("Go to route 7", 1),
        new("Event finished", 3),
    ];
    private void B_AllFlyDest_Click(object sender, EventArgs e)
    {
        for (int i = 0; i < FlyDestItems.Count; i++)
            FlyDestItems[i] = new Tuple<string,bool>(FlyDestItems[i].Item1, true);
    }
    private void B_AllKeys_Click(object sender, EventArgs e)
    {
        for (int i = 0; i < UnlockedKeysItems.Count; i++)
            UnlockedKeysItems[i] = new Tuple<string,bool>(UnlockedKeysItems[i].Item1, true);
    }
    public void SaveMain()
    {
        uint valFly = ReadUInt32LittleEndian(SAV.Data[ofsFly..]);
        for (int i = 0; i < FlyDestItems.Count; i++)
        {
            if (FlyDestC[i] < 32)
            {
                if (FlyDestItems[i].Item2)
                    valFly |= 1u << FlyDestC[i];
                else
                    valFly &= ~(1u << FlyDestC[i]);
            }
            else
            {
                var ofs = ofsFly + (FlyDestC[i] >> 3);
                SAV.Data[ofs] = (byte)((SAV.Data[ofs] & ~(1 << (FlyDestC[i] & 7))) | ((FlyDestItems[i].Item2 ? 1 : 0) << (FlyDestC[i] & 7)));
            }
        }
        WriteUInt32LittleEndian(SAV.Data[ofsFly..], valFly);

        if (SAV is SAV5BW bw)
        {
            // Roamer
            var encount = bw.Encount;
            for (int i = 0; i < cbr.Length; i++)
            {
                int c = bw.Encount.GetRoamerState(i);
                var d = (byte)cbr[i].SelectedIndex;

                if (c == d)
                    continue;
                encount.SetRoamerState(i, d);
                if (c != 1)
                    continue;
                var roamer = i == 0 ? encount.Roamer1 : encount.Roamer2;
                roamer.Clear();
                encount.SetRoamerState2C(i, 0);
            }

            // RoamStatus
            {
                var desired = (ushort)CB_RoamStatus.SelectedIndex;
                bw.EventWork.SetWorkRoamer(desired);
            }

            // LibertyPass
            if (CHK_LibertyPass.IsChecked != bw.Misc.IsLibertyTicketActivated)
                bw.Misc.IsLibertyTicketActivated = CHK_LibertyPass.IsChecked;
        }
        else if (SAV is SAV5B2W2 b2w2)
        {
            // KeySystem
            var keys = b2w2.Keys;
            for (int i = 0; i < 5; i++)
            {
                var index = i * 2;
                var obtain = UnlockedKeysItems[index].Item2;
                if (obtain != keys.GetIsKeyObtained((KeyType5)i))
                    keys.SetIsKeyObtained((KeyType5)i, obtain);

                var unlock = UnlockedKeysItems[index + 1].Item2;
                if (unlock != keys.GetIsKeyUnlocked((KeyType5)i))
                    keys.SetIsKeyUnlocked((KeyType5)i, unlock);
            }
        }
    }
}

public partial class MiscTab5 : TabbedPage
{
    public static MiscMain5 miscMain = new((SAV5)MainPage.sav);
    public static MiscEntree miscEntree = new((SAV5)MainPage.sav);
    public static MiscForest miscForest = new((SAV5)MainPage.sav);
    public static MiscSubway miscSubway = new((SAV5)MainPage.sav);
    public static MiscCityForest miscCityForest = new((SAV5)MainPage.sav);
    public static MiscMusical miscMusical = new((SAV5)MainPage.sav);
    public static MiscMedals miscMedals = new((SAV5)MainPage.sav);
    public MiscTab5()
    {
        BarBackgroundColor = Color.FromArgb("303030");
        BarTextColor = Colors.White;
        Children.Add(miscMain);
        Children.Add(miscEntree);
        Children.Add(miscForest);
        Children.Add(miscSubway);
        if (MainPage.sav is SAV5BW)
            Children.Add(miscCityForest);
        if (MainPage.sav is SAV5B2W2)
            Children.Add(miscMedals);
        Children.Add(miscMusical);
        Children.Add(new cancelpage());
        Children.Add(new Misc5Save());
    }
}
public partial class Misc5Save : ContentPage
{
    public Misc5Save()
    {
        this.Title = "Save";
        this.Content = new Label() { Text = "The MAUI Framework has bugs. This is the save page. Navigate to another page, and then select the page you were trying to reach!" };
    }
    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        MiscTab5.miscMain.SaveRecord();
        MiscTab5.miscMain.SaveMain();
        MiscTab5.miscEntree.SaveEntralink();
        MiscTab5.miscForest.SaveForest();
        MiscTab5.miscSubway.SaveSubway();
        Navigation.PopModalAsync();
    }
}