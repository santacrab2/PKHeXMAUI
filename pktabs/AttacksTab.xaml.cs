using System;
using System.Windows.Input;
using PKHeX.Core;
using Syncfusion.Maui.Inputs;
using Syncfusion.Maui.DataSource.Extensions;
using static PKHeXMAUI.MainPage;

namespace PKHeXMAUI;

public partial class AttacksTab : ContentPage
{
    public bool SkipEvent = false;
    public bool FirstLoad = true;
    public AttacksTab()
    {
        InitializeComponent();
        move1ppups.ItemsSource = new List<int>() { 0, 1, 2, 3 };
        move2ppups.ItemsSource = new List<int>() { 0, 1, 2, 3 };
        move3ppups.ItemsSource = new List<int>() { 0, 1, 2, 3 };
        move4ppups.ItemsSource = new List<int>() { 0, 1, 2, 3 };
        eggsprite.IsVisible = pk.IsEgg;
        if (pk.Species != 0)
            applyattackinfo(pk);
        ICommand refreshCommand = new Command(async () =>
        {
            await applyattackinfo(pk);
            AttackRefresh.IsRefreshing = false;
        });
        AttackRefresh.Command = refreshCommand;
        FirstLoad = false;
    }
    public static List<ComboItem> movlist = [];
    public async Task applyattackinfo(PKM pkm)
    {
        SkipEvent = true;
        eggsprite.IsVisible = pkm.IsEgg;
        if (pkm.HeldItem > 0)
        {
            itemsprite.Source = itemspriteurl;
            itemsprite.IsVisible = true;
        }
        else
        {
            itemsprite.IsVisible = false;
        }

        shinysparklessprite.IsVisible = pkm.IsShiny;
        spriteurl = pkm.Species == 0
            ? "a_egg.png"
            : $"a_{pkm.Species}{((pkm.Form > 0 && !MainPage.NoFormSpriteSpecies.Contains(pkm.Species)) ? $"_{pkm.Form}" : "")}.png";
        attackpic.Source = spriteurl;
        movlist = [];
        foreach (var mov in datasourcefiltered.Moves)
        {
            LegalMoveInfo p = new();
            p.ReloadMoves(new LegalityAnalysis(pkm));
            if (p.CanLearn((ushort)mov.Value))
            {
                movlist.Add(mov);
            }
        }
        move1.ItemsSource = movlist;
        move1.ItemDisplayBinding = new Binding("Text");
        move2.ItemsSource = movlist;
        move2.ItemDisplayBinding = new Binding("Text");
        move3.ItemsSource = movlist;
        move3.ItemDisplayBinding = new Binding("Text");
        move4.ItemsSource = movlist;
        move4.ItemDisplayBinding = new Binding("Text");
        rmove1.ItemsSource = movlist;
        rmove1.ItemDisplayBinding = new Binding("Text");
        rmove2.ItemsSource = movlist;
        rmove2.ItemDisplayBinding = new Binding("Text");
        rmove3.ItemsSource = movlist;
        rmove3.ItemDisplayBinding = new Binding("Text");
        rmove4.ItemsSource = movlist;
        rmove4.ItemDisplayBinding = new Binding("Text");

        move1.SelectedItem = movlist.Find(z => z.Value == pkm.Move1);
        move2.SelectedItem = movlist.Find(z => z.Value == pkm.Move2);
        move3.SelectedItem = movlist.Find(z => z.Value == pkm.Move3);
        move4.SelectedItem = movlist.Find(z => z.Value == pkm.Move4);
        rmove1.SelectedItem = movlist.Find(z => z.Value == pkm.RelearnMove1);
        rmove2.SelectedItem = movlist.Find(z => z.Value == pkm.RelearnMove2);
        rmove3.SelectedItem = movlist.Find(z => z.Value == pkm.RelearnMove3);
        rmove4.SelectedItem = movlist.Find(z => z.Value == pkm.RelearnMove4);
        move1pp.Text = pkm.GetMovePP(pkm.Move1, pkm.Move1_PPUps).ToString();
        move2pp.Text = pkm.GetMovePP(pkm.Move2, pkm.Move2_PPUps).ToString();
        move3pp.Text = pkm.GetMovePP(pkm.Move3, pkm.Move3_PPUps).ToString();
        move4pp.Text = pkm.GetMovePP(pkm.Move4, pkm.Move4_PPUps).ToString();
        move1ppups.SelectedIndex = pkm.Move1_PPUps;
        move2ppups.SelectedIndex = pkm.Move2_PPUps;
        move3ppups.SelectedIndex = pkm.Move3_PPUps;
        move4ppups.SelectedIndex = pkm.Move4_PPUps;
        move1Type.Source = $"type_icon_{MoveInfo.GetType(pkm.Move1, pkm.Context):00}";
        move2Type.Source = $"type_icon_{MoveInfo.GetType(pkm.Move2, pkm.Context):00}";
        move3Type.Source = $"type_icon_{MoveInfo.GetType(pkm.Move3, pkm.Context):00}";
        move4Type.Source = $"type_icon_{MoveInfo.GetType(pkm.Move4, pkm.Context):00}";
        move1Cat.Source = $"attack_category_{MoveInfo.GetCategory(pkm.Move1, pkm.Context):00}";
        move2Cat.Source = $"attack_category_{MoveInfo.GetCategory(pkm.Move2, pkm.Context):00}";
        move3Cat.Source = $"attack_category_{MoveInfo.GetCategory(pkm.Move3, pkm.Context):00}";
        move4Cat.Source = $"attack_category_{MoveInfo.GetCategory(pkm.Move4, pkm.Context):00}";
        if (pk is IMoveShop8Mastery)
            moveshopbutton.IsVisible = true;
        if (pk is PA8 pa8)
        {
            AlphaMasteredLabel.IsVisible = true;
            AlphaMasteredPicker.IsVisible = true;
            AlphaMasteredPicker.ItemsSource = movlist;
            AlphaMasteredPicker.ItemDisplayBinding = new Binding("Text");
            AlphaMasteredPicker.SelectedItem = movlist.Find(z => z.Value == pa8.AlphaMove);
        }
        SkipEvent = false;
    }

    private void applymove1(object sender, EventArgs e)
    {
        if (!SkipEvent)
        {
            pk.Move1 = move1.SelectedIndex >= 0 ? (ushort)((ComboItem)move1.SelectedItem).Value : pk.Move1;
            move1Type.Source = $"type_icon_{MoveInfo.GetType(pk.Move1, pk.Context):00}";
            move1Cat.Source = $"attack_category_{MoveInfo.GetCategory(pk.Move1, pk.Context):00}";
        }
    }
    private void applymove2(object sender, EventArgs e)
    {
        if (!SkipEvent)
        {
            pk.Move2 = move2.SelectedIndex >= 0 ? (ushort)((ComboItem)move2.SelectedItem).Value : pk.Move2;
            move2Type.Source = $"type_icon_{MoveInfo.GetType(pk.Move2, pk.Context):00}";
            move2Cat.Source = $"attack_category_{MoveInfo.GetCategory(pk.Move2, pk.Context):00}";
        }
    }
    private void applymove3(object sender, EventArgs e)
    {
        if (!SkipEvent)
        {
            pk.Move3 = move3.SelectedIndex >= 0 ? (ushort)((ComboItem)move3.SelectedItem).Value : pk.Move3;
            move3Type.Source = $"type_icon_{MoveInfo.GetType(pk.Move3, pk.Context):00}";
            move3Cat.Source = $"attack_category_{MoveInfo.GetCategory(pk.Move3, pk.Context):00}";
        }
    }
    private void applymove4(object sender, EventArgs e)
    {
        if (!SkipEvent)
        {
            pk.Move4 = move4.SelectedIndex >= 0 ? (ushort)((ComboItem)move4.SelectedItem).Value : pk.Move4;
            move4Type.Source = $"type_icon_{MoveInfo.GetType(pk.Move4, pk.Context):00}";
            move4Cat.Source = $"attack_category_{MoveInfo.GetCategory(pk.Move4, pk.Context):00}";
        }
    }
    private void applyrmove1(object sender, EventArgs e)
    {
        if (!SkipEvent)
            pk.RelearnMove1 = rmove1.SelectedIndex >= 0 ? (ushort)((ComboItem)rmove1.SelectedItem).Value : pk.RelearnMove1;
    }
    private void applyrmove2(object sender, EventArgs e)
    {
        if (!SkipEvent)
            pk.RelearnMove2 = rmove2.SelectedIndex >= 0 ? (ushort)((ComboItem)rmove2.SelectedItem).Value : pk.RelearnMove2;
    }
    private void applyrmove3(object sender, EventArgs e)
    {
        if (!SkipEvent)
            pk.RelearnMove3 = rmove3.SelectedIndex >= 0 ? (ushort)((ComboItem)rmove3.SelectedItem).Value : pk.RelearnMove3;
    }
    private void applyrmove4(object sender, EventArgs e)
    {
        if (!SkipEvent)
            pk.RelearnMove4 = rmove4.SelectedIndex >= 0 ? (ushort)((ComboItem)rmove4.SelectedItem).Value : pk.RelearnMove4;
    }

    private async void setsuggmoves(object sender, EventArgs e)
    {
        var m = new ushort[4];
        pk.GetMoveSet(m, true);
        pk.SetMoves(m);
        pk.HealPP();
        await applyattackinfo(pk);
    }

    private void applymove1ppups(object sender, EventArgs e)
    {
        if (!SkipEvent)
            pk.Move1_PPUps = move1ppups.SelectedIndex;
    }
    private void applymove2ppups(object sender, EventArgs e)
    {
        if (!SkipEvent)
            pk.Move2_PPUps = move2ppups.SelectedIndex;
    }
    private void applymove3ppups(object sender, EventArgs e)
    {
        if (!SkipEvent)
            pk.Move3_PPUps = move3ppups.SelectedIndex;
    }
    private void applymove4ppups(object sender, EventArgs e)
    {
        if (!SkipEvent)
            pk.Move4_PPUps = move4ppups.SelectedIndex;
    }

    private void openTReditor(object sender, EventArgs e)
    {
        Navigation.PushModalAsync(new TREditor());
    }

    private void refreshmoves(object sender, EventArgs e)
    {
        if (pk.Species != 0)
            applyattackinfo(pk);
    }

    private void openMoveShopEditor(object sender, EventArgs e)
    {
        Navigation.PushModalAsync(new MoveShopEditor());
    }

    private void applyAlphaMasteredMove(object sender, EventArgs e)
    {
        if (!SkipEvent)
        {
            var selectedmove = (ComboItem)AlphaMasteredPicker.SelectedItem;
            if (pk is PA8 pa8)
                pa8.AlphaMove = (ushort)selectedmove.Value;
        }
    }

    private void applyPP1(object sender, TextChangedEventArgs e)
    {
        pk.Move1_PP = int.Parse(move1pp.Text);
    }
    private void applyPP2(object sender, TextChangedEventArgs e)
    {
        pk.Move2_PP = int.Parse(move2pp.Text);
    }
    private void applyPP3(object sender, TextChangedEventArgs e)
    {
        pk.Move3_PP = int.Parse(move3pp.Text);
    }
    private void applyPP4(object sender, TextChangedEventArgs e)
    {
        pk.Move4_PP = int.Parse(move4pp.Text);
    }

    private async void DisplayMoveInfo1(object sender, EventArgs e)
    {
        var value = pk.Move1;
        var details = $"Category: {(MoveCategory)MoveInfo.GetCategory((ushort)value, EntityContext.Gen9)}\nPower: {MoveInfo.GetPower((ushort)value, EntityContext.Gen9)}\nAccuracy: {MoveInfo.GetAccuracy((ushort)value, EntityContext.Gen9)}\n";
        await DisplayAlert($"{(Move)value}", details, "cancel");
    }
    private async void DisplayMoveInfo2(object sender, EventArgs e)
    {
        var value = pk.Move2;
        var details = $"Category: {(MoveCategory)MoveInfo.GetCategory((ushort)value, EntityContext.Gen9)}\nPower: {MoveInfo.GetPower((ushort)value, EntityContext.Gen9)}\nAccuracy: {MoveInfo.GetAccuracy((ushort)value, EntityContext.Gen9)}\n";
        await DisplayAlert($"{(Move)value}", details, "cancel");
    }
    private async void DisplayMoveInfo3(object sender, EventArgs e)
    {
        var value = pk.Move3;
        var details = $"Category: {(MoveCategory)MoveInfo.GetCategory((ushort)value, EntityContext.Gen9)}\nPower: {MoveInfo.GetPower((ushort)value, EntityContext.Gen9)}\nAccuracy: {MoveInfo.GetAccuracy((ushort)value, EntityContext.Gen9)}\n";
        await DisplayAlert($"{(Move)value}", details, "cancel");
    }
    private async void DisplayMoveInfo4(object sender, EventArgs e)
    {
        var value = pk.Move4;
        var details = $"Category: {(MoveCategory)MoveInfo.GetCategory((ushort)value, EntityContext.Gen9)}\nPower: {MoveInfo.GetPower((ushort)value, EntityContext.Gen9)}\nAccuracy: {MoveInfo.GetAccuracy((ushort)value, EntityContext.Gen9)}\n";
        await DisplayAlert($"{(Move)value}", details, "cancel");
    }
}
public enum MoveCategory
{
    Status,
    Physical,
    Special
}