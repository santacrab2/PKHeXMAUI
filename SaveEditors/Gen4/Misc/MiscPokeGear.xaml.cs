using PKHeX.Core;

namespace PKHeXMAUI;

public partial class MiscPokeGear : ContentPage
{
    private PokegearNumber[] Rolodex = null!;
    private SAV4HGSS SAV = null!;
    public MiscPokeGear(SAV4HGSS sav)
	{
		InitializeComponent();
        SAV = sav;
        Rolodex = SAV.GetPokeGearRoloDex().ToArray();
        CV_Pokegear.ItemTemplate = new DataTemplate(static () =>
            {
                Grid grid = [];
                comboBox cb = new()
                {
                    ItemSource = Enum.GetValues<PokegearNumber>()
                };
                cb.SetBinding(comboBox.SelectedItemProperty, ".", BindingMode.TwoWay);
                grid.Add(cb);
                return grid;
            });
        CV_Pokegear.ItemsSource = Rolodex;

    }
    public void Save() => SAV.SetPokeGearRoloDex(Rolodex);
    private void B_GiveAll_Click(object sender, EventArgs e)
    {
        SAV.PokeGearUnlockAllCallers();
        RefreshList();
    }

    private void B_GiveAllNoTrainers_Click(object sender, EventArgs e)
    {
        SAV.PokeGearUnlockAllCallersNoTrainers();
        RefreshList();
    }

    private void B_DeleteAll_Click(object sender, EventArgs e)
    {
        SAV.PokeGearClearAllCallers();
        RefreshList();
    }
    private void RefreshList()
    {
        Rolodex = SAV.GetPokeGearRoloDex().ToArray();
        CV_Pokegear.ItemsSource = Rolodex;
    }
}