
using PKHeX.Core;
using System.Text;

namespace PKHeXMAUI;

public partial class TrainerEditor7Misc : ContentPage
{
    public SAV7 SAV = (SAV7)MainPage.sav;
    private static readonly string[] AllStyles = Enum.GetNames<PlayerBattleStyle7>();
    private List<string> BattleStyles = [.. AllStyles];
    private List<string> StampList = Enum.GetNames<Stamp7>().Select(z => z.Replace("_", " ")).ToList();
    public TrainerEditor7Misc()
	{
		InitializeComponent();
        StampCollection.ItemTemplate = new DataTemplate(() =>
        {
            Grid grid = new() { Padding = 10 };
            Label stampText = new();
            stampText.SetBinding(Label.TextProperty, ".");
            grid.Add(stampText);
            return grid;
        });
        ThrowsUnlockedCollection.ItemTemplate = new DataTemplate(() =>
        {
            Grid grid = new() { Padding = 10 };
            Label stampText = new();
            stampText.SetBinding(Label.TextProperty, ".");
            grid.Add(stampText);
            return grid;
        });
        ThrowsLearnedCollection.ItemTemplate = new DataTemplate(() =>
        {
            Grid grid = new() { Padding = 10 };
            Label stampText = new();
            stampText.SetBinding(Label.TextProperty, ".");
            grid.Add(stampText);
            return grid;
        });
        SkinColorPicker.ItemsSource = Enum.GetNames<PlayerSkinColor7>();
        SkinColorPicker.SelectedIndex = SAV.MyStatus.DressUpSkinColor;
        UnlockFashionPicker.ItemsSource = new object[] { "New Game", "All Legal", "Everything" };
        DaysRefreshedEntry.Number = SAV.Misc.DaysFromRefreshed;
        if (SAV is not SAV7USUM)
            BattleStyles.RemoveAt(BattleStyles.Count - 1); // remove Nihilist
        BallThrowPicker.ItemsSource = BattleStyles;
        if ((sbyte)SAV.MyStatus.BallThrowType >= 0 && SAV.MyStatus.BallThrowType < BallThrowPicker.Items.Count)
            BallThrowPicker.SelectedIndex = SAV.MyStatus.BallThrowType;
        ThrowsUnlockedCollection.ItemsSource = BattleStyles;
        BattleStlyeDisplayPicker.ItemsSource = new object[] { "unlocked list", "learned list" };
        BattleStlyeDisplayPicker.SelectedIndex = 0;
        ThrowsUnlockedCollection.IsVisible = BattleStlyeDisplayPicker.IsVisible = SAV is SAV7SM;
        StampCollection.ItemsSource = Enum.GetNames<Stamp7>().Select(z => z.Replace("_", " "));
        TThumbUpEntry.Number = SAV.PokeFinder.ThumbsTotalValue;
        RThumbsupEntry.Number = SAV.PokeFinder.ThumbsHighValue;
        CameraVersionPicker.ItemsSource = new object[] { "1", "2", "3", "4", "5" };
        CameraVersionPicker.SelectedIndex = SAV.PokeFinder.CameraVersion;
        SnapCountEntry.Number = SAV.PokeFinder.SnapCount;
        GyroCheck.IsChecked = SAV.PokeFinder.GyroFlag;
        FPNameEntry.Text = SAV.Festa.FestivalPlazaName;
        MegaUnlockCheck.IsChecked = SAV.MyStatus.MegaUnlocked;
        ZUnlockCheck.IsChecked = SAV.MyStatus.ZMoveUnlocked;
        uint stampBits = SAV.Misc.Stamps;
        StampCollection.SelectedItems = StampList.FindAll(i => (stampBits & 1 << StampList.IndexOf(i)) != 0).Cast<object>().ToList();
        const int unlockStart = 292;
        const int learnedStart = 3479;
        ThrowsUnlockedCollection.SelectedItems = BattleStyles.FindAll(i => SAV.EventWork.GetEventFlag(unlockStart + BattleStyles.IndexOf(i))).Cast<object>().ToList();
        ThrowsLearnedCollection.SelectedItems = BattleStyles.FindAll(i => SAV.EventWork.GetEventFlag(learnedStart + BattleStyles.IndexOf(i))).Cast<object>().ToList();
    }

    private void unlockfashion(object sender, EventArgs e)
    {
        SAV.Fashion.Clear();

        // Write Payload

        switch (UnlockFashionPicker.SelectedIndex)
        {
            case 0: // Base Fashion
                {
                    SAV.Fashion.Reset();
                    break;
                }
            case 1: // Full Legal
                ReadOnlySpan<byte> data1 = SAV is SAV7USUM
                    ? SAV.Gender == 0 ? Encoding.ASCII.GetBytes( new StreamReader( FileSystem.OpenAppPackageFileAsync("fashion_m_uu").Result).ReadToEnd()) : Encoding.ASCII.GetBytes( new StreamReader(FileSystem.OpenAppPackageFileAsync("fashion_f_uu").Result).ReadToEnd())
                    : SAV.Gender == 0 ? Encoding.ASCII.GetBytes( new StreamReader(FileSystem.OpenAppPackageFileAsync("fashion_m_sm").Result).ReadToEnd()) : Encoding.ASCII.GetBytes(new StreamReader(FileSystem.OpenAppPackageFileAsync("fashion_f_sm").Result).ReadToEnd());
                SAV.Fashion.ImportPayload(data1);
                break;
            case 2: // Everything
                ReadOnlySpan<byte> data2 = SAV is SAV7USUM
                    ? SAV.Gender == 0 ? Encoding.ASCII.GetBytes(new StreamReader(FileSystem.OpenAppPackageFileAsync("fashion_m_uu_illegal").Result).ReadToEnd()) : Encoding.ASCII.GetBytes(new StreamReader(FileSystem.OpenAppPackageFileAsync("fashion_f_uu_illegal").Result).ReadToEnd())
                    : SAV.Gender == 0 ? Encoding.ASCII.GetBytes(new StreamReader(FileSystem.OpenAppPackageFileAsync("fashion_m_sm_illegal").Result).ReadToEnd()) : Encoding.ASCII.GetBytes(new StreamReader(FileSystem.OpenAppPackageFileAsync("fashion_f_sm_illegal").Result).ReadToEnd());
                SAV.Fashion.ImportPayload(data2);
                break;
            default:
                return;
        }
    }
    private uint GetBits(CollectionView listbox)
    {
        uint bits = 0;
        for (int i = 0; i < StampList.Count; i++)
        {
            if (listbox.SelectedItems.Contains(StampList[i]))
                bits |= 1u << i;
        }
        return bits;
    }

    public void SaveTE7Mi()
    {
        var skin = SkinColorPicker.SelectedIndex & 1;
        if (SAV.Gender == skin)
            SAV.MyStatus.DressUpSkinColor = SkinColorPicker.SelectedIndex;
        SAV.Misc.DaysFromRefreshed = (int)DaysRefreshedEntry.Number;
        SAV.MyStatus.BallThrowType = (byte)BallThrowPicker.SelectedIndex;
        if(SAV is SAV7SM)
        {
            const int unlockStart = 292;
            const int learnedStart = 3479;
            for (int i = 2; i < BattleStyles.Count; i++)
                SAV.EventWork.SetEventFlag(unlockStart + i, ThrowsUnlockedCollection.SelectedItems.Contains(BattleStyles[i]));
            for (int i = 1; i < BattleStyles.Count; i++)
                SAV.EventWork.SetEventFlag(learnedStart + i, ThrowsLearnedCollection.SelectedItems.Contains(BattleStyles[i]));
        }
        SAV.PokeFinder.ThumbsTotalValue = (uint)TThumbUpEntry.Number;
        SAV.PokeFinder.ThumbsHighValue = (uint)RThumbsupEntry.Number;
        SAV.PokeFinder.SnapCount = (uint)SnapCountEntry.Number;
        SAV.PokeFinder.GyroFlag = GyroCheck.IsChecked;
        SAV.PokeFinder.CameraVersion = (ushort)CameraVersionPicker.SelectedIndex;
        SAV.Festa.FestivalPlazaName = FPNameEntry.Text;
        SAV.Misc.Stamps = GetBits(StampCollection);
        SAV.MyStatus.MegaUnlocked = MegaUnlockCheck.IsChecked;
        SAV.MyStatus.ZMoveUnlocked = ZUnlockCheck.IsChecked;
    }

    private void ChangeThrowCollection(object sender, EventArgs e)
    {
        switch (BattleStlyeDisplayPicker.SelectedIndex)
        {
            case 0: ThrowsUnlockedCollection.IsVisible = true; ThrowsLearnedCollection.IsVisible = false; break;
            case 1: ThrowsUnlockedCollection.IsVisible = false; ThrowsLearnedCollection.IsVisible = true; break;
        }
    }
}