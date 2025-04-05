using PKHeX.Core;
using System;

namespace PKHeXMAUI;

public partial class MiscPoffins : ContentPage
{
    private PoffinCase4 Case = null!; // initialized on load
    private int CurrentIndex = -1;
    private bool Updating;
    private propertyGrid PG_Poffins = null!;
    private readonly string[] ItemNames = Util.GetStringList("poffin4", "en");
    private readonly List<string> LB_Poffins = [];
    public MiscPoffins(SAV4Sinnoh sav)
	{
		InitializeComponent();
        Case = new PoffinCase4(sav);
        LB_Poffins.Clear();
        for (int i = 0; i < Case.Poffins.Length; i++)
            LB_Poffins.Add(GetPoffinText(i));
        CV_Poffins.ItemsSource = LB_Poffins;
        PG_Poffins = new(Case.Poffins[0]);
        G_Poffins.Add(PG_Poffins, 1, 0);
    }
    private string GetPoffinName(PoffinFlavor4 flavor)
    {
        var index = (uint)flavor;
        if (index >= ItemNames.Length)
            index = 0;
        return ItemNames[index];
    }

    private string GetPoffinText(int index) => $"{index + 1:000} - {GetPoffinName(Case.Poffins[index].Type)}";

    public void Save()
    {
        SaveIndex(CurrentIndex);
        Case.Save();
    }

    private void SaveIndex(int index)
    {
        // do nothing, PropertyGrid handles everything
        if (index < 0)
            return;
        Updating = true;
        LB_Poffins[index] = GetPoffinText(index);
        Updating = false;
    }

    private void LoadIndex(int index)
    {
        if (index < 0)
        {
            CV_Poffins.SelectedItem = LB_Poffins[0];
            return;
        }
        PG_Poffins.CurrentItem = Case.Poffins[index];
    }

    private void LB_Poffins_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (Updating)
            return;
        SaveIndex(CurrentIndex);
        LoadIndex(CurrentIndex = LB_Poffins.IndexOf((string)CV_Poffins.SelectedItem));
    }

    private void B_PoffinAll_Click(object sender, EventArgs e)
    {
        Case.FillCase();
    }

    private void B_PoffinDel_Click(object sender, EventArgs e)
    {
        Case.DeleteAll();
    }

    private void PG_Poffins_PropertyValueChanged(object s, EventArgs e)
    {
        SaveIndex(LB_Poffins.IndexOf((string)CV_Poffins.SelectedItem));
    }
}