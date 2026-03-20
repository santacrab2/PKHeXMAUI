
using PKHeX.Core;
using System;

namespace PKHeXMAUI;

public partial class MiscPokeblock : ContentPage
{
    private PokeBlock3Case Case = null!; // initialized on load
    ISaveBlock3LargeHoenn SAV;
    public MiscPokeblock(ISaveBlock3LargeHoenn sav)
	{
		InitializeComponent();
        SAV = sav;
        Case = sav.PokeBlocks;
        CV_Pokeblocks.ItemTemplate = new DataTemplate(() =>
        {
            Grid grid = [];
            Label BlockLabel = new();
            BlockLabel.SetBinding(Label.TextProperty, new Binding("Color"));
            grid.Add(BlockLabel);
            return grid;
        });
        CV_Pokeblocks.ItemsSource = Case.Blocks;
        CV_Pokeblocks.SelectedItem = Case.Blocks[0];
        propertyGrid pg_Pokeblock = new(CV_Pokeblocks.SelectedItem);
        CV_Pokeblocks.SelectionChanged += (_, _) => pg_Pokeblock.CurrentItem = CV_Pokeblocks.SelectedItem;
        PokeBlockStack.Add(pg_Pokeblock);
        Button B_GiveAll = new()
        {
            Text = "Give All"
        };
        B_GiveAll.Clicked += B_GiveAll_Clicked;
        Grid buttongrid = [];
        buttongrid.Add(B_GiveAll);
        Button B_delAll = new()
        {
            Text = "Delete All"
        };
        B_delAll.Clicked += B_delAll_Clicked;
        buttongrid.Add(B_delAll, 1);
        PokeBlockStack.Add(buttongrid);
	}

    private void B_delAll_Clicked(object? sender, EventArgs? e)
    {
        Case.DeleteAll();
    }

    private void B_GiveAll_Clicked(object? sender, EventArgs? e)
    {
        Case.MaximizeAll(true);
    }

    public void Save()
    {
        SAV.PokeBlocks = Case;
    }
}