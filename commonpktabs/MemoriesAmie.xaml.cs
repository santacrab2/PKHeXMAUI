using PKHeX.Core;
using static PKHeXMAUI.MainPage;

namespace PKHeXMAUI;

public partial class MemoriesAmie : TabbedPage
{
	public MemoriesAmie()
	{

		InitializeComponent();
		MemoriesWithOT.Text = $"Memories with {pk.OT_Name}(OT)";
		MemoryTypePicker.ItemsSource = memorytext.Memory;
		MemoryTypePicker.ItemDisplayBinding = new Binding("Text");
	
		Friendshipeditor.Text = pk.OT_Friendship.ToString();
		HTFriendshipeditor.Text = pk.HT_Friendship.ToString();
		MemoriesWithHT.Text = $"Memories with {pk.HT_Name}(Not OT)";
		HTMemoryTypePicker.ItemsSource = memorytext.Memory;
		HTMemoryTypePicker.ItemDisplayBinding = new Binding("Text");
        if (pk is ITrainerMemories mems)
        {
            MemoryTypePicker.SelectedItem = memorytext.Memory.Where(z => z.Value == mems.OT_Memory).FirstOrDefault();
			HTMemoryTypePicker.SelectedItem = memorytext.Memory.Where(x => x.Value == mems.HT_Memory).FirstOrDefault();
        }
		if (pk.CurrentHandler == 0)
			chlabel.Text = pk.OT_Name;	
		else
			chlabel.Text = pk.HT_Name;
		fullnesseditor.Text = pk.Fullness.ToString();
		EnjoymentEditor.Text = pk.Enjoyment.ToString();
    }
	public static MemoryStrings memorytext = new MemoryStrings(GameInfo.Strings);

    private void SaveMemoriesAndClose(object sender, EventArgs e)
    {
		if (int.TryParse(Friendshipeditor.Text, out var result))
		{

			if (result > 255)
				result = 255;
			
			pk.OT_Friendship = result;
		}
		if (int.TryParse(HTFriendshipeditor.Text, out var result2))
		{
			if (result2 > 255)
				result2 = 255;
			pk.HT_Friendship = result2;
		}
			
		if(pk is ITrainerMemories mems)
		{
            var selectedmemorytype = (ComboItem)MemoryTypePicker.SelectedItem;
			mems.OT_Memory = (byte)selectedmemorytype.Value;
			var selectedhtmemorytype = (ComboItem)HTMemoryTypePicker.SelectedItem;
			mems.HT_Memory = (byte)selectedhtmemorytype.Value;
        }
		if(byte.TryParse(fullnesseditor.Text,out var result3))
		{
			if (result3 > 255)
				result3 = 255;
			pk.Fullness = result3;
		}
		if(byte.TryParse(EnjoymentEditor.Text,out var result4))
		{
			if (result4 > 255)
				result4 = 255;
			pk.Enjoyment = result4;
		}
		Navigation.PopModalAsync();
    }

    private void CloseMemories(object sender, EventArgs e)
    {
		Navigation.PopModalAsync();
    }
}