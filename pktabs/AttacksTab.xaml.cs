using System;
using System.Windows.Input;
using PKHeX.Core;
using static pk9reader.MainPage;


namespace pk9reader;


public partial class AttacksTab : ContentPage
{
	public AttacksTab()
	{
		InitializeComponent();
        move1ppups.ItemsSource = new List<int>() { 0, 1, 2, 3 };
        move2ppups.ItemsSource = new List<int>() { 0, 1, 2, 3 };
        move3ppups.ItemsSource = new List<int>() { 0, 1, 2, 3 };
        move4ppups.ItemsSource = new List<int>() { 0, 1, 2, 3 };
        ICommand refreshCommand = new Command(() =>
        {
            if (pk.Species != 0)
                applyattackinfo(pk);
            attackrefresh.IsRefreshing = false;
        });
        attackrefresh.Command = refreshCommand;
        if(pk.Species != 0)
            applyattackinfo(pk);
    }
    public static List<Move> movlist = new();
	public void applyattackinfo(PKM pkm)
	{
        if (pkm.IsShiny)
            shinysparklessprite.IsVisible = true;
        else
            shinysparklessprite.IsVisible = false;
        attackpic.Source = spriteurl;
        movlist = new();
       foreach(var mov in datasourcefiltered.Moves)
        {
            LegalMoveInfo p = new();
            p.ReloadMoves(new LegalityAnalysis(pkm));
            if (p.CanLearn((ushort)mov.Value))
            {
                movlist.Add((Move)mov.Value);
            }
        }
        move1.ItemsSource = movlist;
        move2.ItemsSource = movlist;
        move3.ItemsSource = movlist;
        move4.ItemsSource = movlist;
        rmove1.ItemsSource = movlist;
        rmove2.ItemsSource = movlist;
        rmove3.ItemsSource = movlist;
        rmove4.ItemsSource = movlist;
    
        move1.SelectedItem = (Move)pkm.Move1;
        move2.SelectedItem = (Move)pkm.Move2;
        move3.SelectedItem = (Move)pkm.Move3;
        move4.SelectedItem = (Move)pkm.Move4;
        rmove1.SelectedItem = (Move)pkm.RelearnMove1;
        rmove2.SelectedItem = (Move)pkm.RelearnMove2;
        rmove3.SelectedItem = (Move)pkm.RelearnMove3;
        rmove4.SelectedItem = (Move)pkm.RelearnMove4;
        move1pp.Text = pkm.GetMovePP(pkm.Move1, pkm.Move1_PPUps).ToString();
        move2pp.Text = pkm.GetMovePP(pkm.Move2, pkm.Move2_PPUps).ToString();
        move3pp.Text = pkm.GetMovePP(pkm.Move3, pkm.Move3_PPUps).ToString();
        move4pp.Text = pkm.GetMovePP(pkm.Move4, pkm.Move4_PPUps).ToString();
        move1ppups.SelectedIndex = pkm.Move1_PPUps;
        move2ppups.SelectedIndex = pkm.Move2_PPUps;
        move3ppups.SelectedIndex = pkm.Move3_PPUps;
        move4ppups.SelectedIndex = pkm.Move4_PPUps;
    }

    private void applymove1(object sender, EventArgs e)
    {
        pk.Move1 = move1.SelectedIndex > 0? (ushort)(Move)move1.SelectedItem:pk.Move1;
    }
    private void applymove2(object sender, EventArgs e)
    {
        pk.Move2 = move2.SelectedIndex > 0? (ushort)(Move)move2.SelectedItem:pk.Move2;
    }
    private void applymove3(object sender, EventArgs e)
    {
        pk.Move3 = move3.SelectedIndex > 0 ? (ushort)(Move)move3.SelectedItem : pk.Move3;
    }
    private void applymove4(object sender, EventArgs e)
    {
        pk.Move4 = move4.SelectedIndex > 0 ? (ushort)(Move)move4.SelectedItem : pk.Move4;
    }
    private void applyrmove1(object sender, EventArgs e)
    {
        pk.RelearnMove1 = rmove1.SelectedIndex > 0 ? (ushort)(Move)rmove1.SelectedItem : pk.RelearnMove1;
    }
    private void applyrmove2(object sender, EventArgs e)
    {
        pk.RelearnMove2 = rmove2.SelectedIndex > 0 ? (ushort)(Move)rmove2.SelectedItem : pk.RelearnMove2;
    }
    private void applyrmove3(object sender, EventArgs e)
    {
        pk.RelearnMove3 = rmove3.SelectedIndex > 0 ? (ushort)(Move)rmove3.SelectedItem : pk.RelearnMove3;
    }
    private void applyrmove4(object sender, EventArgs e)
    {
        pk.RelearnMove4 = rmove4.SelectedIndex > 0 ? (ushort)(Move)rmove4.SelectedItem : pk.RelearnMove4;
    }

    private void setsuggmoves(object sender, EventArgs e)
    {
        var m = pk.GetMoveSet(true);
        pk.SetMoves(m);
        pk.HealPP();
        move1.SelectedItem = (Move)pk.Move1;
        move2.SelectedItem = (Move)pk.Move2;
        move3.SelectedItem = (Move)pk.Move3;
        move4.SelectedItem = (Move)pk.Move4;
        rmove1.SelectedItem = (Move)pk.RelearnMove1;
        rmove2.SelectedItem = (Move)pk.RelearnMove2;
        rmove3.SelectedItem = (Move)pk.RelearnMove3;
        rmove4.SelectedItem = (Move)pk.RelearnMove4;
    }

    private void applymove1ppups(object sender, EventArgs e)
    {
        pk.Move1_PPUps = move1ppups.SelectedIndex;
    }
    private void applymove2ppups(object sender, EventArgs e)
    {
        pk.Move2_PPUps = move2ppups.SelectedIndex;
    }
    private void applymove3ppups(object sender, EventArgs e)
    {
        pk.Move3_PPUps = move3ppups.SelectedIndex;
    }
    private void applymove4ppups(object sender, EventArgs e)
    {
        pk.Move4_PPUps = move4ppups.SelectedIndex;
    }


}