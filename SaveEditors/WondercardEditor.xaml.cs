using Microsoft.Maui;
using PKHeX.Core;
using System;

namespace PKHeXMAUI;

public partial class WondercardEditor : ContentPage
{
    private readonly SaveFile Origin;
    private readonly SaveFile SAV;
    private readonly IMysteryGiftStorage Cards;
    private readonly IMysteryGiftFlags? Flags;
    private readonly DataMysteryGift[] Album;
    private DataMysteryGift? mg;
    private readonly List<ImageButton> pba = []; // don't mutate this list
    private List<string> Received = [];
    public WondercardEditor(SaveFile sav, DataMysteryGift? g = null)
	{
		InitializeComponent();
        SAV = (Origin = sav).Clone();
        Cards = GetMysteryGiftProvider(SAV);

        Album = LoadMysteryGifts();
        Flags = Cards as IMysteryGiftFlags;
        pba = GetGiftPictureBoxes(SAV.Generation);
        SetGiftBoxes();
        GetReceivedFlags();
        if (Received.Count > 0)
            CV_ReceivedList.SelectedItem = Received[0];

        if (Album[0] is WR7) // giftused is not a valid prop
            B_UnusedAll.IsVisible = B_UsedAll.IsVisible = L_QR.IsVisible = false;
        ClickView(pba[0], EventArgs.Empty);
    }
    private static IMysteryGiftStorage GetMysteryGiftProvider(SaveFile sav)
    {
        if (sav is IMysteryGiftStorageProvider provider)
            return provider.MysteryGiftStorage;
        throw new ArgumentException("Save file does not support Mystery Gifts.", nameof(sav));
    }
    private DataMysteryGift[] LoadMysteryGifts()
    {
        var count = Cards.GiftCountMax;
        var size = SAV is SAV4HGSS ? count + 1 : count;
        var result = new DataMysteryGift[size];
        for (int i = 0; i < count; i++)
            result[i] = Cards.GetMysteryGift(i);
        if (SAV is SAV4HGSS s4)
            result[^1] = s4.LockCapsuleSlot;
        return result;
    }
    private void SetGiftBoxes()
    {
        for (int i = 0; i < Album.Length; i++)
            pba[i].Source = $"a_{Album[i].Species}{((Album[i].Form > 0 && !MainPage.NoFormSpriteSpecies.Contains(Album[i].Species)) ? $"_{Album[i].Form}" : "")}.png";
    }
    private void GetReceivedFlags()
    {
        Received.Clear();
        if (Flags is not { } f)
            return;
        var count = f.MysteryGiftReceivedFlagMax;
        for (int i = 1; i < count; i++)
        {
            if (f.GetMysteryGiftReceivedFlag(i))
                Received.Add(i.ToString("0000"));
        }

        if (Received.Count > 0)
            CV_ReceivedList.SelectedItem = Received[0];
    }
    private List<ImageButton> GetGiftPictureBoxes(byte generation) => generation switch
    {
        4 => PopulateViewGiftsG4(),
        5 or 6 or 7 => PopulateViewGiftsG567(),
        _ => throw new ArgumentOutOfRangeException(nameof(generation), generation, "Game not supported."),
    };
    private List<ImageButton> PopulateViewGiftsG4()
    {
        List<ImageButton> pb = [];
        for(int i = 0; i < 4; i++)
            G_Gifts.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        // Row 1
        Grid f1 = new() { ColumnDefinitions = [new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Auto),] };
        f1.Add(GetLabel($"{nameof(PGT)} 1-6"));
        for (int i = 0; i < 6; i++)
        {
            var p = GetPictureBox(68, 56, $"PGT {i + 1}");
            p.Clicked += ClickView;
            f1.Add(p,i + 1);
            pb.Add(p);
        }
        // Row 2
        Grid f2 = new() { ColumnDefinitions = [new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Auto),] };
        f2.Add(GetLabel($"{nameof(PGT)} 7-8"));
        for (int i = 6; i < 8; i++)
        {
            var p = GetPictureBox(68, 56, $"PGT {i + 1}");
            p.Clicked += ClickView;
            f2.Add(p,i - 5);
            pb.Add(p);
        }
        // Row 3
        Grid f3 = new() { ColumnDefinitions = [new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Auto),] };
        f3.Margin = new (0, 12, 0, 0);
        f3.Add(GetLabel($"{nameof(PCD)} 1-3"));
        for (int i = 8; i < 11; i++)
        {
            var p = GetPictureBox(68, 56, $"PCD {i - 7}");
            p.Clicked += ClickView;
            f3.Add(p, i - 7);
            pb.Add(p);
        }

        G_Gifts.Add(f1);
        G_Gifts.Add(f2,0,1);
        G_Gifts.Add(f3,0,2);

        if (Album.Length == 12) // lock capsule
        {
            // Row 4
            Grid f4 = new() { ColumnDefinitions = [new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Auto)] };
            f4.Add(GetLabel(GameInfo.Strings.Item[533])); // Lock Capsule
            var p = GetPictureBox(68, 56, "PCD Lock Capsule");
            p.Clicked += ClickView;
            f4.Add(p,1);
            pb.Add(p);
            G_Gifts.Add(f4,0,3);
        }
        return pb;

    }
    private void ClickView(object? sender, EventArgs e)
    {
        E_Details.Text = string.Empty;
        int index = pba.IndexOf((ImageButton)sender);
        ViewGiftData(Album[index]);
    }
    private async void ViewGiftData(DataMysteryGift g)
    {
        try
        {
            // only check if the form is visible (not opening)
            if (g.GiftUsed)
            {
                var prompt = await DisplayAlertAsync("Alert", "Mystery Gift is marked as USED and will not be able to be picked up in-game."+ "Do you want to remove the USED flag so that it is UNUSED?","Yes","No");
                if (prompt)
                    g.GiftUsed = false;
            }
            var desc = g.GetDescription();
            for (int i = 0; i < desc.Count(); i++)
            {
                E_Details.Text += desc.ToArray()[i] + Environment.NewLine;
            }
            IMG_ActiveSprite.Source = $"a_{g.Species}{((g.Form > 0 && !MainPage.NoFormSpriteSpecies.Contains(g.Species)) ? $"_{g.Form}" : "")}.png";
            mg = g;
        }
        // Some user input mystery gifts can have out-of-bounds values. Just swallow any exception.
        catch (Exception e)
        {
            E_Details.Text = string.Empty;
            await DisplayAlertAsync("Error", $"An error occurred while trying to view this Mystery Gift's data.{Environment.NewLine}{e.Message}", "OK");
        }
    }
    private static ImageButton GetPictureBox(int width, int height, string name) => new()
    {
        WidthRequest = width + 2,
        HeightRequest = height + 2,
        AutomationId = name,
        BorderColor = Colors.Black,
        BorderWidth = 1,
    };
    private static Label GetLabel(string text) => new()
    {
        Text = text,
    };
    private List<ImageButton> PopulateViewGiftsG567()
    {
        List<ImageButton> pb = [];

        const int cellsPerRow = 6;
        int rows = (int)Math.Ceiling(Album.Length / (decimal)cellsPerRow);
        int countRemaining = Album.Length;
        for (int i = 0; i < rows; i++)
        {
            G_Gifts.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            Grid row = new(){ ColumnDefinitions = [new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Auto)] };
            int count = cellsPerRow >= countRemaining ? countRemaining : cellsPerRow;
            countRemaining -= count;
            int start = (i * cellsPerRow) + 1;
            row.Add(GetLabel($"{start}-{start + count - 1}"));
            for (int j = 0; j < count; j++)
            {
                var p = GetPictureBox(68, 56, $"Row {i} Slot {start + j}");
                row.Add(p,j);
                pb.Add(p);
            }
            G_Gifts.Add(row,0,i);
        }
        return pb;
    }
    private void CV_ReceivedList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {

    }

    private void B_ExportClick(object sender, EventArgs e)
    {

    }

    private void B_ImportClick(object sender, EventArgs e)
    {

    }

    private void B_ModifyAll_Click(object sender, EventArgs e)
    {
        foreach (var g in Album)
            g.GiftUsed = sender == B_UsedAll;
        SetGiftBoxes();
    }

    private void B_Close_Click(object sender, EventArgs e)
    {
        Navigation.PopModalAsync();
    }

    private void B_Save_Click(object sender, EventArgs e)
    {

    }
}