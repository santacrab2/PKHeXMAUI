using PKHeX.Core;
using System;
using System.Drawing.Imaging;
using System.Drawing;
using System.Runtime.InteropServices;
using SkiaSharp;
using static PKHeXMAUI.PoketchDotMatrix;
using CommunityToolkit.Maui.Converters;
using Microsoft.Maui.Graphics.Platform;
using System.Collections.ObjectModel;
namespace PKHeXMAUI;

public partial class MiscMain : ContentPage
{
    private readonly SAV4 Origin;
    private readonly SAV4 SAV;
    private const int FlyFlagStart = 2480;
    private static ReadOnlySpan<byte> FlyWorkFlagSinnoh => [000, 001, 002, 003, 004, 005, 006, 007, 008, 009, 010, 011, 012, 013, 014, 015, 016, 017, 067, 068];
    private static ReadOnlySpan<byte> LocationIDsSinnoh => [001, 002, 003, 004, 005, 082, 083, 006, 007, 008, 009, 010, 011, 012, 013, 014, 054, 081, 055, 015];
    private static ReadOnlySpan<byte> FlyWorkFlagHGSS => [000, 001, 002, 003, 004, 005, 006, 007, 008, 009, 010, 011, 012, 013, 014, 015, 016, 017, 018, 019, 020, 021, 022, 027, 030, 033, 035];
    private static ReadOnlySpan<byte> LocationIDsHGSS => [138, 139, 140, 141, 142, 143, 144, 145, 146, 147, 148, 126, 127, 128, 129, 130, 131, 132, 133, 134, 135, 136, 137, 229, 227, 221, 225];


    private string[] seals, accessories, backdrops, poketchapps;
    private ObservableCollection<Tuple<string, bool>> FlyDestItems = [];
    private byte[] DotArtistByte = [120];
    public MiscMain(SAV4 sav)
	{
		InitializeComponent();
        SAV = (SAV4)(Origin = sav).Clone();
        GB_Poketch.IsVisible = sav is not SAV4HGSS;
        L_UpgradeMap.IsVisible = CB_UpgradeMap.IsVisible = sav is SAV4HGSS;
        poketchapps = GameInfo.Strings.poketchapps;
        TapGestureRecognizer tap = new();
        tap.Tapped += PB_DotArtist_MouseClick;
        PB_DotArtist.GestureRecognizers.Add(tap);
        ReadMain();
    }
    private void ReadMain()
    {
        NUD_Coin.MaxValue = SAV.MaxCoins;
        NUD_Coin.Number = Math.Clamp(SAV.Coin, 0, SAV.MaxCoins);
        NUD_BP.Number = Math.Clamp(SAV.BP, 0, 9999);

        var locations = SAV is SAV4Sinnoh ? LocationIDsSinnoh : LocationIDsHGSS;
        var flags = SAV is SAV4Sinnoh ? FlyWorkFlagSinnoh : FlyWorkFlagHGSS;

        CV_Flydest.ItemTemplate = new(() =>
        {
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new(GridLength.Star));
            CheckBox cb = new();
            cb.SetBinding(CheckBox.IsCheckedProperty, "Item2",BindingMode.TwoWay);
            Label lb = new();
            lb.SetBinding(Label.TextProperty, "Item1");
            grid.Add(cb);
            grid.Add(lb,1,0);
            return grid;
        });
        for (int i = 0; i < locations.Length; i++)
        {
            var flagIndex = FlyFlagStart + flags[i];
            var state = SAV.GetEventFlag(flagIndex);

            var locationID = locations[i];
            var name = GameInfo.Strings.Gen4.Met0[locationID];
            FlyDestItems.Add(Tuple.Create(name, state));
        }
        CV_Flydest.ItemsSource = FlyDestItems;
        if (SAV is SAV4Sinnoh sinnoh)
        {
            ReadPoketch(sinnoh);
            NUD_UGFlags.Number = Math.Clamp(sinnoh.UG_FlagsCaptured, 0, SAV4Sinnoh.UG_MAX);
            L_PokeathlonPoints.IsVisible = NUD_PokeathlonPoints.IsVisible = false;
        }
        else if (SAV is SAV4HGSS hgss)
        {
            NUD_PokeathlonPoints.Number = hgss.PokeathlonPoints;
            L_UGFlags.IsVisible = NUD_UGFlags.IsVisible = false;
            ReadOnlySpan<string> items = ["Map Johto", "Map Johto+", "Map Johto & Kanto"];
            var index = hgss.MapUnlockState;
            if (index >= MapUnlockState4.Invalid)
                index = MapUnlockState4.JohtoKanto;
            CB_UpgradeMap.ItemSource = items.ToArray();
            CB_UpgradeMap.SelectedIndex = (int)index;
        }

    }
    public void SaveMain()
    {
        SAV.Coin = (uint)NUD_Coin.Number;
        SAV.BP = (ushort)NUD_BP.Number;

        var flags = SAV is SAV4Sinnoh ? FlyWorkFlagSinnoh : FlyWorkFlagHGSS;
        for (int i = 0; i < FlyDestItems.Count; i++)
        {
            var index = FlyFlagStart + flags[i];
            SAV.SetEventFlag(index, FlyDestItems[i].Item2);
        }

        if (SAV is SAV4Sinnoh sinnoh)
        {
            SavePoketch(sinnoh);
            sinnoh.UG_FlagsCaptured = (uint)NUD_UGFlags.Number;
        }
        else if (SAV is SAV4HGSS hgss)
        {
            hgss.PokeathlonPoints = (uint)NUD_PokeathlonPoints.Number;
            hgss.MapUnlockState = (MapUnlockState4)CB_UpgradeMap.SelectedIndex;
        }

    }
    private void SavePoketch(SAV4Sinnoh s)
    {
        int unlockedCount = 0;
        s.CurrentPoketchApp = (sbyte)CB_CurrentApp.SelectedIndex;
        for (int i = 0; i < PoketchItems.Count; i++)
        {
            var b = PoketchItems[i].Item2;
            s.SetPoketchAppUnlocked((PoketchApp)i, b);
            if (b) unlockedCount++;
        }
        s.SetPoketchDotArtistData(DotArtistByte);
        s.PoketchUnlockedCount = (byte)unlockedCount;
    }
    private ObservableCollection<Tuple<string, bool>> PoketchItems = [];
    private void ReadPoketch(SAV4Sinnoh s)
    {
        CB_CurrentApp.ItemSource = poketchapps;

        for (PoketchApp i = 0; i <= PoketchApp.Alarm_Clock; i++)
        {
            var name = poketchapps[(int)i];
            var title = $"{(int)i:00} - {name}";
            var value = s.GetPoketchAppUnlocked(i);
            PoketchItems.Add(Tuple.Create(title, value));
        }
        CV_Poketch.ItemTemplate = new(() =>
        {
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new(GridLength.Star));
            CheckBox cb = new();
            cb.SetBinding(CheckBox.IsCheckedProperty, "Item2",BindingMode.TwoWay);
            Label lb = new();
            lb.SetBinding(Label.TextProperty, "Item1");
            grid.Add(cb);
            grid.Add(lb, 1, 0);
            return grid;
        });
        CV_Poketch.ItemsSource = PoketchItems;
        CB_CurrentApp.SelectedIndex = s.CurrentPoketchApp;

        DotArtistByte = s.GetPoketchDotArtistData();
        SetPictureBoxFromFlags(DotArtistByte);
        
    }
    private void PB_DotArtist_MouseClick(object? sender, TappedEventArgs e)
    {
        var point = (Microsoft.Maui.Graphics.Point)e.GetPosition(PB_DotArtist);
        SetFlagsFromClickPoint((int)point.X, (int)point.Y);
        SetPictureBoxFromFlags(DotArtistByte);
    }
    private void SetFlagsFromClickPoint(int inpX, int inpY)
    {
        inpX = inpX >> 2;
        inpY = inpY >> 2;

        int i = (inpX>>2) + (DotMatrixWidth * (inpY>>2));
        Span<byte> ndab = stackalloc byte[DotMatrixPixelCount / 4];
        DotArtistByte.AsSpan().CopyTo(ndab);

        byte c = (byte)((ndab[i >> 2] >> ((i % 4) << 1)) & 3);
        if (++c >= 4)
            c = 0;

        ndab[i >> 2] &= (byte)~(3 << ((i % 4) << 1));
        ndab[i >> 2] |= (byte)((c & 3) << ((i % 4) << 1));

        ndab.CopyTo(DotArtistByte);
    }
    private void SetPictureBoxFromFlags(ReadOnlySpan<byte> inp)
    {
        if (inp.Length != PoketchDotMatrix.DotMatrixPixelCount / 4)
            return;
        SKBitmap bitmap = GetDotArt(inp);
        var result = bitmap.Encode(SKEncodedImageFormat.Png, 100);
        Microsoft.Maui.Graphics.IImage testimage;
        using (Stream stream = result.AsStream())
        {
            testimage = PlatformImage.FromStream(stream);
        }
        PB_DotArtist.Source = new ByteArrayToImageSourceConverter().ConvertFrom(testimage.AsBytes());
    }
    private void B_GiveAll_Click(object sender, EventArgs e)
    {
        for (int i = 0; i < PoketchItems.Count; i++)
        {
            var item = PoketchItems[i];
            PoketchItems[i] = Tuple.Create(item.Item1, true);
        }
    }
    private void B_AllFlyDest_Click(object sender, EventArgs e)
    {
        for (int i = 0; i < FlyDestItems.Count; i++)
        {
            var item = FlyDestItems[i];
            FlyDestItems[i] = Tuple.Create(item.Item1, true);
        }
    }
}

public static class PoketchDotMatrix
{
    public const int DotMatrixHeight = 20;
    public const int DotMatrixWidth = 24;
    public const int DotMatrixPixelCount = DotMatrixHeight * DotMatrixWidth;

    public static ReadOnlySpan<byte> ColorTable => [248, 168, 88, 8];
    public const int DotMatrixUpscaleFactor = 4;

    public static SKBitmap GetDotArt(ReadOnlySpan<byte> inp)
    {
        int upscaleFactor = DotMatrixUpscaleFactor;
        int width = DotMatrixWidth * upscaleFactor;
        int height = DotMatrixHeight * upscaleFactor;
        byte[] dupbyte = new byte[width * height * 4]; // 4 bytes per pixel (BGRA)

        for (int iy = 0; iy < DotMatrixHeight; iy++)
        {
            for (int ix = 0; ix < DotMatrixWidth; ix++)
            {
                var ib = ix + (DotMatrixWidth * iy);
                var ict = ColorTable[(inp[ib >> 2] >> ((ib % 4) << 1)) & 3];

                for (int izy = 0; izy < upscaleFactor; izy++)
                {
                    for (int izx = 0; izx < upscaleFactor; izx++)
                    {
                        int pixelIndex = ((iy * upscaleFactor + izy) * width + (ix * upscaleFactor + izx)) * 4;
                        dupbyte[pixelIndex] = ict;       // B
                        dupbyte[pixelIndex + 1] = ict;   // G
                        dupbyte[pixelIndex + 2] = ict;   // R
                        dupbyte[pixelIndex + 3] = 255;   // A (opaque)
                    }
                }
            }
        }

        var dabmp = new SKBitmap(width, height, SKColorType.Bgra8888, SKAlphaType.Premul);
        using (SKPixmap map = dabmp.PeekPixels())
        {
            Marshal.Copy(dupbyte, 0, map.GetPixels(), dupbyte.Length);
        }

        return dabmp;
    }
}
public class MiscTab4 : TabbedPage 
{
    public static MiscMain MiscMain = new((SAV4)MainPage.sav);
    public static MiscBattleFrontier4 MBF4 = new((SAV4)MainPage.sav);
    public static MiscPokeWalker MPW;
    public static MiscSeals MSeals = new((SAV4)MainPage.sav);
    public MiscTab4()
    {
        BarBackgroundColor = Microsoft.Maui.Graphics.Color.FromArgb("303030");
        BarTextColor = Colors.White;
        Children.Add(MiscMain);
        Children.Add(MBF4);
        if (MainPage.sav is SAV4HGSS s)
            Children.Add(MPW = new(s));
        Children.Add(MSeals);
        Children.Add(new cancelpage());
        Children.Add(new Misc4Save());
    }
}
public partial class Misc4Save : ContentPage
{
    public Misc4Save()
    {
        this.Title = "Save";
        this.Content = new Label() { Text = "The MAUI Framework has bugs. This is the save page. Navigate to another page, and then select the page you were trying to reach!" };
    }
    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        MiscTab4.MiscMain.SaveMain();
        if (MainPage.sav is SAV4HGSS s)
            MiscTab4.MPW.SaveWalker(s);
        MiscTab4.MSeals.SaveSeals();
        Navigation.PopModalAsync();
    }
}