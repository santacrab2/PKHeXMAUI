using PKHeX.Core;

namespace PKHeXMAUI;

public partial class HoneyTreeEditor : ContentPage
{
    private readonly SAV4Sinnoh Origin;
    private readonly SAV4Sinnoh SAV;
    private readonly byte[] MunchlaxTrees;
    private int entry;
    private bool loading;
    private HoneyTreeValue? Tree;
    public HoneyTreeEditor(SAV4Sinnoh sav)
	{
		InitializeComponent();
        SAV = (SAV4Sinnoh)(Origin = sav).Clone();

        // Get Munchlax tree for this savegame in screen
        MunchlaxTrees = new byte[4];
        HoneyTreeUtil.CalculateMunchlaxTrees(SAV.ID32, MunchlaxTrees);
        var names = new object[] { "Route 205, Floaroma Town side", "Route 205, Eterna City side", "Route 206", "Route 207", "Route 208", "Route 209", "Route 210, Solaceon Town side", "Route 210, Celestic Town side", "Route 211", "Route 212, Hearthome City side", "Route 212, Pastoria City side", "Route 213", "Route 214", "Route 215", "Route 218", "Route 221", "Route 222", "Valley Windworks", "Eterna Forest", "Fuego Ironworks", "Floaroma Meadow" };
        const string sep = "- ";
        CB_TreeList.ItemSource = names;
        L_Tree0.Text = string.Join(Environment.NewLine,
            sep + names[MunchlaxTrees[0]],
            sep + names[MunchlaxTrees[1]],
            sep + names[MunchlaxTrees[2]],
            sep + names[MunchlaxTrees[3]]);

        CB_TreeList.SelectedIndex = 0;
    }
    private ushort TreeSpecies => SAV.GetHoneyTreeSpecies((int)NUD_Group.Number, (int)NUD_Slot.Number);
    private void B_Catchable_Clicked(object sender, EventArgs e) => NUD_Time.Number = 1080;
    private async void ChangeGroupSlot(object sender, EventArgs e)
    {
        var species = TreeSpecies;
        L_Species.Text = GetLabelText(species);

        if (loading)
            return;

        if (species == (int)Species.Munchlax && !MunchlaxTrees.AsSpan().Contains((byte)CB_TreeList.SelectedIndex))
            await DisplayAlertAsync("Warning","Catching Munchlax in this tree will make it illegal for this savegame's TID16/SID16 combination.","ok");
    }
    private static string GetLabelText(ushort species)
    {
        var str = GameInfo.Strings;
        var arr = str.specieslist;
        if (species != (int)Species.Silcoon)
            return arr[species];

        // Silcoon/Cascoon
        var games = str.gamelist;
        return $"{arr[species + 0]} ({games[(int)GameVersion.D]})" + Environment.NewLine +
               $"{arr[species + 2]} ({games[(int)GameVersion.P]})";
    }
    private void B_Cancel_Clicked(object sender, EventArgs e)
    {
        Navigation.PopModalAsync();
    }

    private void ChangeTree(object sender, EventArgs e)
    {
        SaveTree();
        entry = CB_TreeList.SelectedIndex;
        ReadTree();
    }

    private void ReadTree()
    {
        loading = true;
        Tree = SAV.GetHoneyTree(entry);

        NUD_Time.Number = Math.Min(NUD_Time.MaxValue, Tree.Time);
        NUD_Shake.Number = Math.Min(NUD_Shake.MaxValue, Tree.Shake);
        NUD_Group.Number = Math.Min(NUD_Group.MaxValue, Tree.Group);
        NUD_Slot.Number = Math.Min(NUD_Slot.MaxValue, Tree.Slot);

        ChangeGroupSlot(this, EventArgs.Empty);
        loading = false;
    }

    private void SaveTree()
    {
        if (Tree is null)
            return;

        Tree.Time = (uint)NUD_Time.Number;
        Tree.Shake = (int)NUD_Shake.Number;
        Tree.Group = (int)NUD_Group.Number;
        Tree.Slot = (int)NUD_Slot.Number;

        SAV.SetHoneyTree(Tree, entry);
    }

    private void B_Save_Clicked(object sender, EventArgs e)
    {
        SaveTree();
        Origin.CopyChangesFrom(SAV);
        Navigation.PopModalAsync();
    }

}