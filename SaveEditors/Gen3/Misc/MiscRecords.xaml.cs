
using PKHeX.Core;

namespace PKHeXMAUI;

public partial class MiscRecords : ContentPage
{
	public bool setting = false;
	public static List<ComboItem> items = [];
	public MiscRecords(SAV3 sav)
	{
		InitializeComponent();
		setting = true;
		items = [.. Record3.GetItems(sav)];
        ComboRecord.ItemSource = items;
        ComboRecord.DisplayMemberPath = "Text";
        ComboRecord.SelectedIndexChanged += (_, _) =>
		{
			if (ComboRecord.SelectedItem is null) return;
			var index = ((ComboItem)ComboRecord.SelectedItem).Value;
			EntryRecordValue.Number = sav.GetRecord(index);
			EntryFameH.IsVisible = EntryFameM.IsVisible = EntryFameS.IsVisible = index == 1;
        };
		ComboRecord.SelectedIndex = 1;
		ComboRecord.SelectedIndex = 0;
		EntryRecordValue.ValueChanged += (_, _) =>
		{
			if (ComboRecord.SelectedItem is null || setting) return;
			var index = ((ComboItem)ComboRecord.SelectedItem).Value;
			var value = (uint)EntryRecordValue.Number;
			sav.SetRecord(index, value);
			if (index == 1)
			{
				setting = true;
				EntryFameH.Number = (value >> 16);
				EntryFameM.Number = (value >> 8);
				EntryFameS.Number = value;
				setting = false;
			}
		};
		EntryFameH.ValueChanged += (_, _) => ChangeFame();
		EntryFameM.ValueChanged += (_, _) => ChangeFame();
		EntryFameS.ValueChanged += (_, _) => ChangeFame();
        void ChangeFame() => sav.SetRecord(1, (uint)(EntryRecordValue.Number = GetFameTime()));
        setting = false;
    }
    public uint GetFameTime()
    {
        var hrs = Math.Min(9999, (uint)EntryFameH.Number);
        var min = Math.Min(59, (uint)EntryFameM.Number);
        var sec = Math.Min(59, (uint)EntryFameS.Number);

        return ((hrs << 16) | (min << 8) | sec);
    }
}