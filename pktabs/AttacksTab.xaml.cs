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
        move1.ItemsSource = Enum.GetValues(typeof(Move));
        move2.ItemsSource = Enum.GetValues(typeof(Move));
        move3.ItemsSource = Enum.GetValues(typeof(Move));
        move4.ItemsSource = Enum.GetValues(typeof(Move));
        rmove1.ItemsSource = Enum.GetValues(typeof(Move));
        rmove2.ItemsSource = Enum.GetValues(typeof(Move));
        rmove3.ItemsSource = Enum.GetValues(typeof(Move));
        rmove4.ItemsSource = Enum.GetValues(typeof(Move));
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

	public void applyattackinfo(PK9 pkm)
	{
		attackpic.Source = spriteurl;
        move1.SelectedIndex = pkm.Move1;
        move2.SelectedIndex = pkm.Move2;
        move3.SelectedIndex = pkm.Move3;
        move4.SelectedIndex = pkm.Move4;
        rmove1.SelectedIndex = pkm.RelearnMove1;
        rmove2.SelectedIndex = pkm.RelearnMove2;
        rmove3.SelectedIndex = pkm.RelearnMove3;
        rmove4.SelectedIndex = pkm.RelearnMove4;
    }

    private void applymove1(object sender, EventArgs e)
    {
        pk.Move1 = (ushort)move1.SelectedIndex;
    }
    private void applymove2(object sender, EventArgs e)
    {
        pk.Move2 = (ushort)move2.SelectedIndex;
    }
    private void applymove3(object sender, EventArgs e)
    {
        pk.Move3 = (ushort)move3.SelectedIndex;
    }
    private void applymove4(object sender, EventArgs e)
    {
        pk.Move4 = (ushort)move4.SelectedIndex;
    }
    private void applyrmove1(object sender, EventArgs e)
    {
        pk.RelearnMove1 = (ushort)rmove1.SelectedIndex;
    }
    private void applyrmove2(object sender, EventArgs e)
    {
        pk.RelearnMove2 = (ushort)rmove2.SelectedIndex;
    }
    private void applyrmove3(object sender, EventArgs e)
    {
        pk.RelearnMove3 = (ushort)rmove3.SelectedIndex;
    }
    private void applyrmove4(object sender, EventArgs e)
    {
        pk.RelearnMove4 = (ushort)rmove4.SelectedIndex;
    }

    private void setsuggmoves(object sender, EventArgs e)
    {
        var m = pk.GetMoveSet(true);
        pk.SetMoves(m);
        pk.HealPP();
    }
}