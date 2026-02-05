using PKHeX.Core;

namespace PKHeXMAUI;

public partial class MiscForest : ContentPage
{
    public SAV5 SAV;
    private EntreeForest Forest = null!;
    private IList<EntreeSlot> AllSlots = null!;
    private List<string> slotlist = [];
    public MiscForest(SAV5 sav)
	{
        SAV = sav;
        InitializeComponent();
        LoadForest();
    }
    private void LoadForest()
    {
        Forest = SAV.EntreeForest;
        Forest.EnsureDecrypted();
        AllSlots = Forest.Slots;
        NUD_Unlocked.Number = (Forest.Unlock38Areas + 2);
        CHK_Area9.IsChecked = Forest.Unlock9thArea;

        var areas = AllSlots.Select(z => z.Area).Distinct()
            .Select(z => new ComboItem(z.ToString(), (int)z)).ToList();

        var filtered = GameInfo.FilteredSources;
        CB_Species.ItemSource = filtered.Species.ToArray();
        CB_Move.ItemSource = filtered.Moves.ToArray();
        CB_Areas.ItemSource = areas;

        CB_Areas.SelectedIndex = 0;
    }
    private void SaveForest()
    {
        Forest.Unlock38Areas = (int)NUD_Unlocked.Number - 2;
        Forest.Unlock9thArea = CHK_Area9.IsChecked;
    }
    private IList<EntreeSlot> CurrentSlots = null!;
    private int currentIndex = -1;

    private void ChangeArea(object sender, EventArgs e)
    {
        var area = CB_Areas.SelectedIndex;
        CurrentSlots = AllSlots.Where(z => (int)z.Area == area).ToArray();
        slotlist.Clear();
        foreach (var z in CurrentSlots.Select(z => GetSpeciesName(z.Species)))
            slotlist.Add(z);
        CV_Slots.SelectedItem = slotlist[currentIndex = 0];
    }

    private void ChangeSlot(object sender, EventArgs e)
    {
        CurrentSlot = null;
        if (slotlist.IndexOf((string)CV_Slots.SelectedItem) >= 0)
            currentIndex = slotlist.IndexOf((string)CV_Slots.SelectedItem);
        var current = CurrentSlots[currentIndex];
        CB_Species.SelectedItem = (int)current.Species;
        SetForms(current);
        SetGenders(current);
        CB_Move.SelectedItem = (int)current.Move;
        CB_Gender.SelectedItem = (int)current.Gender;
        CB_Form.SelectedIndex = CB_Form.Items.Count <= current.Form ? 0 : current.Form;
        NUD_Animation.Number = (current.Animation);
        CurrentSlot = current;
    }
    private EntreeSlot? CurrentSlot;
    public static string GetSpeciesName(ushort species)
    {
        var arr = GameInfo.Strings.Species;
        if (species >= arr.Count)
            return $"Invalid: {species}";
        return arr[species];
    }
    private void SetForms(EntreeSlot slot)
    {
        bool hasForms = PersonalTable.B2W2[slot.Species].HasForms || slot.Species == (int)Species.Mothim;
        L_Form.IsVisible = CB_Form.IsEnabled = CB_Form.IsVisible = hasForms;

        var list = FormConverter.GetFormList(slot.Species, GameInfo.Strings.types, GameInfo.Strings.forms, GameInfo.GenderSymbolUnicode, SAV.Context);
        CB_Form.ItemSource = list;
    }
    private void SetGenders(EntreeSlot slot)
    {
        CB_Gender.ItemSource = GetGenderChoices(slot.Species);
    }
    private static List<ComboItem> GetGenderChoices(ushort species)
    {
        if (species == 0)
            return [new("-", 0)];
        var pi = PersonalTable.B2W2[species];
        var list = new List<ComboItem>();
        if (pi.Genderless)
        {
            list.Add(new ComboItem("Genderless", 2));
            return list;
        }

        if (!pi.OnlyFemale)
            list.Add(new ComboItem("Male", 0));
        if (!pi.OnlyMale)
            list.Add(new ComboItem("Female", 1));
        return list;
    }
}