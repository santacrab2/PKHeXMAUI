using System.Windows.Input;
using PKHeX.Core;
using static PKHeXMAUI.MainPage;
using System.Collections;
namespace PKHeXMAUI;

public partial class AttacksTab : ContentPage
{
    public bool SkipEvent = false;
    public bool FirstLoad = true;
    public comboBox[] moveboxes = [];
    public AttacksTab()
    {
        InitializeComponent();
        moveboxes = [move1, move2, move3, move4, rmove1, rmove2, rmove3, rmove4];
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
    public static List<MoveDisplay> movlist = [];
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
            : $"a_{pkm.Species}{((pkm.Form > 0 && !NoFormSpriteSpecies.Contains(pkm.Species)) ? $"_{pkm.Form}" : "")}.png";
        attackpic.Source = spriteurl;
        movlist = [];
        LegalMoveSource<ComboItem> p = new(new LegalMoveComboSource());
        p.ChangeMoveSource(datasourcefiltered.Moves);
        p.ReloadMoves(new LegalityAnalysis(pkm));
        foreach(var move in p.Display.DataSource)
        {
            var valid = p.Info.CanLearn((ushort)move.Value);
            movlist.Add(new MoveDisplay(move, valid));
        }
        move1.ItemSource = movlist;
        move1.DisplayMemberPath = "Text";
        move2.ItemSource = movlist;
        move2.DisplayMemberPath = "Text";
        move3.ItemSource = movlist;
        move3.DisplayMemberPath = "Text";
        move4.ItemSource = movlist;
        move4.DisplayMemberPath = "Text";
        rmove1.ItemSource = movlist;
        rmove1.DisplayMemberPath = "Text";
        rmove2.ItemSource = movlist;
        rmove2.DisplayMemberPath = "Text";
        rmove3.ItemSource = movlist;
        rmove3.DisplayMemberPath = "Text";
        rmove4.ItemSource = movlist;
        rmove4.DisplayMemberPath = "Text";

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
            AlphaMasteredPicker.ItemSource = movlist;
            AlphaMasteredPicker.DisplayMemberPath = "Text";
            AlphaMasteredPicker.SelectedItem = movlist.Find(z => z.Value == pa8.AlphaMove);
        }
        SkipEvent = false;
    }

    private void applymove1(object sender, EventArgs e)
    {
        if (!SkipEvent)
        {
            if (move1.SelectedIndex >= 0 && move1.SelectedItem != null)
            {
                var valueProperty = move1.SelectedItem.GetType().GetProperty("Value");
                if (valueProperty != null)
                {
                    var value = valueProperty.GetValue(move1.SelectedItem);
                    if (value is int intValue)
                    {
                        pk.Move1 = (ushort)intValue;
                    }
                }
            }
            move1Type.Source = $"type_icon_{MoveInfo.GetType(pk.Move1, pk.Context):00}";
            move1Cat.Source = $"attack_category_{MoveInfo.GetCategory(pk.Move1, pk.Context):00}";
        }
    }
    private void applymove2(object sender, EventArgs e)
    {
        if (!SkipEvent)
        {
            if (move2.SelectedIndex >= 0 && move2.SelectedItem != null)
            {
                var valueProperty = move2.SelectedItem.GetType().GetProperty("Value");
                if (valueProperty != null)
                {
                    var value = valueProperty.GetValue(move2.SelectedItem);
                    if (value is int intValue)
                    {
                        pk.Move2 = (ushort)intValue;
                    }
                }
            }
            move2Type.Source = $"type_icon_{MoveInfo.GetType(pk.Move2, pk.Context):00}";
            move2Cat.Source = $"attack_category_{MoveInfo.GetCategory(pk.Move2, pk.Context):00}";
        }
    }
    private void applymove3(object sender, EventArgs e)
    {
        if (!SkipEvent)
        {
            if (move3.SelectedIndex >= 0 && move3.SelectedItem != null)
            {
                var valueProperty = move3.SelectedItem.GetType().GetProperty("Value");
                if (valueProperty != null)
                {
                    var value = valueProperty.GetValue(move3.SelectedItem);
                    if (value is int intValue)
                    {
                        pk.Move3 = (ushort)intValue;
                    }
                }
            }
            move3Type.Source = $"type_icon_{MoveInfo.GetType(pk.Move3, pk.Context):00}";
            move3Cat.Source = $"attack_category_{MoveInfo.GetCategory(pk.Move3, pk.Context):00}";
        }
    }
    private void applymove4(object sender, EventArgs e)
    {
        if (!SkipEvent)
        {
            if (move4.SelectedIndex >= 0 && move4.SelectedItem != null)
            {
                var valueProperty = move4.SelectedItem.GetType().GetProperty("Value");
                if (valueProperty != null)
                {
                    var value = valueProperty.GetValue(move4.SelectedItem);
                    if (value is int intValue)
                    {
                        pk.Move4 = (ushort)intValue;
                    }
                }
            }
            move4Type.Source = $"type_icon_{MoveInfo.GetType(pk.Move4, pk.Context):00}";
            move4Cat.Source = $"attack_category_{MoveInfo.GetCategory(pk.Move4, pk.Context):00}";
        }
    }
    private void applyrmove1(object sender, EventArgs e)
    {
        if (!SkipEvent)
        {
            if (rmove1.SelectedIndex >= 0 && rmove1.SelectedItem != null)
            {
                var valueProperty = rmove1.SelectedItem.GetType().GetProperty("Value");
                if (valueProperty != null)
                {
                    var value = valueProperty.GetValue(rmove1.SelectedItem);
                    if (value is int intValue)
                    {
                        pk.RelearnMove1 = (ushort)intValue;
                    }
                }
            }
            refreshmoveboxelist();
        }
    }
    private void applyrmove2(object sender, EventArgs e)
    {
        if (!SkipEvent)
        {
            if (rmove2.SelectedIndex >= 0 && rmove2.SelectedItem != null)
            {
                var valueProperty = rmove2.SelectedItem.GetType().GetProperty("Value");
                if (valueProperty != null)
                {
                    var value = valueProperty.GetValue(rmove2.SelectedItem);
                    if (value is int intValue)
                    {
                        pk.RelearnMove2 = (ushort)intValue;
                    }
                }
            }
            refreshmoveboxelist();
        }
    }
    private void applyrmove3(object sender, EventArgs e)
    {
        if (!SkipEvent)
        {
            if (rmove3.SelectedIndex >= 0 && rmove3.SelectedItem != null)
            {
                var valueProperty = rmove3.SelectedItem.GetType().GetProperty("Value");
                if (valueProperty != null)
                {
                    var value = valueProperty.GetValue(rmove3.SelectedItem);
                    if (value is int intValue)
                    {
                        pk.RelearnMove3 = (ushort)intValue;
                    }
                }
            }
            refreshmoveboxelist();
        }
    }
    private void applyrmove4(object sender, EventArgs e)
    {
        if (!SkipEvent)
        {
            if (rmove4.SelectedIndex >= 0 && rmove4.SelectedItem != null)
            {
                var valueProperty = rmove4.SelectedItem.GetType().GetProperty("Value");
                if (valueProperty != null)
                {
                    var value = valueProperty.GetValue(rmove4.SelectedItem);
                    if (value is int intValue)
                    {
                        pk.RelearnMove4 = (ushort)intValue;
                    }
                }
            }
            refreshmoveboxelist();
        }
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
        {
            pk.Move1_PPUps = move1ppups.SelectedIndex;
            applyattackinfo(pk);
        }
    }
    private void applymove2ppups(object sender, EventArgs e)
    {
        if (!SkipEvent)
        {
            pk.Move2_PPUps = move2ppups.SelectedIndex;
            applyattackinfo(pk);
        }
    }
    private void applymove3ppups(object sender, EventArgs e)
    {
        if (!SkipEvent)
        {
            pk.Move3_PPUps = move3ppups.SelectedIndex;
            applyattackinfo(pk);
        }
    }
    private void applymove4ppups(object sender, EventArgs e)
    {
        if (!SkipEvent)
        {
            pk.Move4_PPUps = move4ppups.SelectedIndex;
            applyattackinfo(pk);
        }
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
            if (AlphaMasteredPicker.SelectedIndex >= 0 && AlphaMasteredPicker.SelectedItem != null)
            {
                var valueProperty = AlphaMasteredPicker.SelectedItem.GetType().GetProperty("Value");
                if (valueProperty != null)
                {
                    var value = valueProperty.GetValue(move1.SelectedItem);
                    if (value is int intValue && pk is PA8 pa8)
                    {
                        pa8.AlphaMove = (ushort)intValue;
                    }
                }
            }
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
    public void refreshmoveboxelist()
    {
        movlist = [];
        LegalMoveSource<ComboItem> p = new(new LegalMoveComboSource());
        p.ChangeMoveSource(datasourcefiltered.Moves);
        p.ReloadMoves(new LegalityAnalysis(pk));
        foreach (var move in p.Display.DataSource)
        {
            var valid = p.Info.CanLearn((ushort)move.Value);
            movlist.Add(new MoveDisplay(move, valid));
        }
        foreach (var box in moveboxes)
        {
            box.ItemSource = movlist;
        }
    }
}
public enum MoveCategory
{
    Status,
    Physical,
    Special
}
public class MoveDisplay(ComboItem move, bool valid)
{
    public string Text { get; init; } = move.Text;
    public int Value { get; init; } = move.Value;
    public bool Valid { get; init; } = valid;
}