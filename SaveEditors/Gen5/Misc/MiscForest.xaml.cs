using PKHeX.Core;
using System.Collections.ObjectModel;

namespace PKHeXMAUI;

public partial class MiscForest : ContentPage
{
    public SAV5 SAV;
    private EntreeForest Forest = null!;
    private IList<EntreeSlot> AllSlots = null!;
    private ObservableCollection<string> slotlist = [];
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
        CB_Species.DisplayMemberPath = "Text";
        CB_Move.ItemSource = filtered.Moves.ToArray();
        CB_Move.DisplayMemberPath = "Text";
        CB_Areas.ItemSource = areas;
        CB_Areas.DisplayMemberPath = "Text";
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
        ComboItem area = (ComboItem)CB_Areas.SelectedItem;
        CurrentSlots = [.. AllSlots.Where(z => (int)z.Area == area.Value)];
        slotlist.Clear();
        foreach (var z in CurrentSlots.Select(z => GetSpeciesName(z.Species)))
            slotlist.Add(z);
        CV_Slots.ItemsSource = slotlist;
        CV_Slots.SelectedItem = slotlist[currentIndex = 0];
    }

    private void ChangeSlot(object sender, EventArgs e)
    {
        CurrentSlot = null;
        if (slotlist.IndexOf((string)CV_Slots.SelectedItem) >= 0)
            currentIndex = slotlist.IndexOf((string)CV_Slots.SelectedItem);
        var current = CurrentSlots[currentIndex];
        var filtered = GameInfo.FilteredSources;
        CB_Species.SelectedItem = filtered.Species.Where(z=>z.Value==(int)current.Species).First();
        SetForms(current);
        SetGenders(current);
        CB_Move.SelectedItem = filtered.Moves.Where(z=>z.Value==(int)current.Move).First();
        CB_Gender.SelectedIndex = (int)current.Gender;
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
        CB_Gender.DisplayMemberPath = "Text";
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

    private void B_RandForest_Click(object sender, EventArgs e)
    {
        var source = (SAV is SAV5BW ? Encounters5BW.DreamWorld_BW : Encounters5B2W2.DreamWorld_B2W2).Concat(Encounters5DR.DreamWorld_Common).ToList();
        var rnd = Util.Rand;
        foreach (var s in AllSlots)
        {
            int index = rnd.Next(source.Count);
            var slot = source[index];
            source.Remove(slot);
            s.Species = slot.Species;
            s.Form = slot.Form;
            s.Gender = !((IFixedGender)slot).IsFixedGender ? PersonalTable.B2W2[slot.Species].RandomGender() : slot.Gender;

            ReadOnlySpan<ushort> moves = slot.Moves;
            var count = moves.Length - moves.Count<ushort>(0);
            s.Move = count == 0 ? (ushort)0 : moves[rnd.Next(count)];
        }
        ChangeArea(this, EventArgs.Empty); // refresh
        NUD_Unlocked.Number = 8;
        CHK_Area9.IsChecked = true;
    }
    private void UpdateSlotValue(object sender, EventArgs e)
    {
        if (CurrentSlot is null)
            return;

        if (sender == CB_Species)
        {
            var filtered = GameInfo.FilteredSources;
            CurrentSlot.Species = (ushort)filtered.Species.Where(z=>z.Value == ((ComboItem)CB_Species.SelectedItem).Value).First().Value;
            slotlist[currentIndex] = GetSpeciesName(CurrentSlot.Species);
            SetForms(CurrentSlot);
            SetGenders(CurrentSlot);
        }
        else if (sender == CB_Move)
        {
            CurrentSlot.Move = (ushort)CB_Move.SelectedIndex;
        }
        else if (sender == CB_Gender)
        {
            CurrentSlot.Gender = (byte)CB_Gender.SelectedIndex;
        }
        else if (sender == CB_Form)
        {
            CurrentSlot.Form = (byte)CB_Form.SelectedIndex;
        }
        else if (sender == CHK_Invisible)
        {
            CurrentSlot.Invisible = CHK_Invisible.IsChecked;
        }
        else if (sender == NUD_Animation)
        {
            CurrentSlot.Animation = (int)NUD_Animation.Number;
        }
    }
}