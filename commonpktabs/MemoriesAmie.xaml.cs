using PKHeX.Core;
using Syncfusion.Maui.Inputs;

using static PKHeXMAUI.MainPage;

namespace PKHeXMAUI;

public partial class MemoriesAmie : TabbedPage
{
    public Label MoreThanAFeeling = new() { Text = "Feeling", IsVisible = false };
    public Label HTMoreThanAFeeling = new() { Text = "Feeling", IsVisible = false };
    public SfComboBox MoreThanAFeelingbox = new() { IsEditable = true, IsVisible = false, BackgroundColor = Colors.Transparent };
    public SfComboBox HTMoreThanAFeelingbox = new() { IsEditable = true, IsVisible = false, BackgroundColor = Colors.Transparent };
    public Label Intense = new() { Text = "Intensity", IsVisible = false };
    public Label HTIntense = new() { Text = "Intensity", IsVisible = false };
    public SfComboBox Intensebox = new() { IsEditable = true, IsVisible = false, BackgroundColor = Colors.Transparent };
    public SfComboBox HTIntenseBox = new() { IsEditable = true, IsVisible = false, BackgroundColor = Colors.Transparent };
    public Label TextVar = new() { IsVisible = false };
    public Label HTTextVar = new() { IsVisible = false };
    public SfComboBox OTTextVarBox = new() { IsEditable = true, IsVisible = false, BackgroundColor = Colors.Transparent };
    public SfComboBox HTTextVarBox = new() { IsEditable = true, IsVisible = false, BackgroundColor = Colors.Transparent };
    public Label MemoryString = new() { IsVisible = false };
    public Label HTMemoryString = new() { IsVisible = false };
    public MemoriesAmie()
    {
        InitializeComponent();
        MemoriesWithOT.Text = $"Memories with {pk.OriginalTrainerName}(OT)";
        MemoryTypePicker.ItemsSource = memorytext.Memory;
        MemoryTypePicker.ItemDisplayBinding = new Binding("Text");

        Friendshipeditor.Text = pk.OriginalTrainerFriendship.ToString();
        HTFriendshipeditor.Text = pk.HandlingTrainerFriendship.ToString();
        MemoriesWithHT.Text = $"Memories with {pk.HandlingTrainerName}(Not OT)";
        HTMemoryTypePicker.ItemsSource = memorytext.Memory;
        HTMemoryTypePicker.ItemDisplayBinding = new Binding("Text");
        if (pk is ITrainerMemories mems)
        {
            MemoryTypePicker.SelectedItem = memorytext.Memory.Find(z => z.Value == mems.OriginalTrainerMemory);
            HTMemoryTypePicker.SelectedItem = memorytext.Memory.Find(x => x.Value == mems.HandlingTrainerMemory);

            MoreThanAFeelingbox.ItemsSource = memorytext.GetMemoryFeelings(pk.Generation).ToArray();
            MoreThanAFeelingbox.SelectionChanged += UpdateMemoryString;
            MoreThanAFeelingbox.PropertyChanged += ChangeComboBoxFontColor;
            HTMoreThanAFeelingbox.ItemsSource = memorytext.GetMemoryFeelings(pk.Generation).ToArray();
            HTMoreThanAFeelingbox.SelectionChanged += UpdateMemoryString;
            HTMoreThanAFeelingbox.PropertyChanged += ChangeComboBoxFontColor;
            MoreThanAFeelingbox.SelectedIndex = mems.OriginalTrainerMemoryFeeling;
            HTMoreThanAFeelingbox.SelectedIndex = mems.HandlingTrainerMemoryFeeling;
            Intensebox.ItemsSource = memorytext.GetMemoryQualities().ToArray();
            Intensebox.SelectionChanged += UpdateMemoryString;
            Intensebox.PropertyChanged += ChangeComboBoxFontColor;
            HTIntenseBox.ItemsSource = memorytext.GetMemoryQualities().ToArray();
            HTIntenseBox.SelectionChanged += UpdateMemoryString;
            HTIntenseBox.PropertyChanged += ChangeComboBoxFontColor;
            Intensebox.SelectedIndex = mems.OriginalTrainerMemoryIntensity;
            HTIntenseBox.SelectedIndex = mems.HandlingTrainerMemoryIntensity;
            var memindex = Memories.GetMemoryArgType(mems.OriginalTrainerMemory, pk.Format);
            var argvals = memorytext.GetArgumentStrings(memindex, pk.Format);
            OTTextVarBox.ItemsSource = argvals;
            OTTextVarBox.SelectionChanged += UpdateMemoryString;
            OTTextVarBox.PropertyChanged += ChangeComboBoxFontColor;
            TextVar.Text = GetMemoryCategory(memindex, pk.Format);
            var HTmemindex = Memories.GetMemoryArgType(mems.HandlingTrainerMemory, pk.Format);
            var HTargvals = memorytext.GetArgumentStrings(HTmemindex, pk.Format);
            HTTextVarBox.ItemsSource = HTargvals;
            HTTextVarBox.SelectionChanged += UpdateMemoryString;
            HTTextVarBox.PropertyChanged += ChangeComboBoxFontColor;
            HTTextVar.Text = GetMemoryCategory(HTmemindex, pk.Format);
            OTTextVarBox.SelectedItem = argvals.Find(z => z.Value == mems.OriginalTrainerMemoryVariable);
            HTTextVarBox.SelectedItem = HTargvals.Find(z => z.Value == mems.HandlingTrainerMemoryVariable);
            OTMemoryStack.Insert(5, TextVar);
            OTMemoryStack.Insert(6, OTTextVarBox);
            OTMemoryStack.Insert(7, Intense);
            OTMemoryStack.Insert(8, Intensebox);
            OTMemoryStack.Insert(9, MoreThanAFeeling);
            OTMemoryStack.Insert(10, MoreThanAFeelingbox);
            OTMemoryStack.Insert(11, MemoryString);
            HTMemoryStack.Insert(5, HTTextVar);
            HTMemoryStack.Insert(6, HTTextVarBox);
            HTMemoryStack.Insert(7, HTIntense);
            HTMemoryStack.Insert(8, HTIntenseBox);
            HTMemoryStack.Insert(9, HTMoreThanAFeeling);
            HTMemoryStack.Insert(10, HTMoreThanAFeelingbox);
            HTMemoryStack.Insert(11, HTMemoryString);
            if (mems.OriginalTrainerMemory > 0)
            {
                MoreThanAFeeling.IsVisible = true;
                MoreThanAFeelingbox.IsVisible = true;
                Intense.IsVisible = true;
                Intensebox.IsVisible = true;
                MemoryString.IsVisible = true;
                OTTextVarBox.IsVisible = true;
                TextVar.IsVisible = true;
                MemoryString.Text = string.Format(memorytext.Memory[mems.OriginalTrainerMemory].Text, pk.Nickname, pk.OriginalTrainerName, (ComboItem)OTTextVarBox.SelectedItem != null ? ((ComboItem)OTTextVarBox.SelectedItem).Text : argvals[0].Text, (string)Intensebox.SelectedItem, (string)MoreThanAFeelingbox.SelectedItem);
            }
            if (mems.HandlingTrainerMemory > 0)
            {
                HTMoreThanAFeeling.IsVisible = true;
                HTMoreThanAFeelingbox.IsVisible = true;
                HTIntense.IsVisible = true;
                HTIntenseBox.IsVisible = true;
                HTMemoryString.IsVisible = true;
                HTTextVar.IsVisible = true;
                HTTextVarBox.IsVisible = true;
                HTMemoryString.Text = string.Format(memorytext.Memory[mems.HandlingTrainerMemory].Text, pk.Nickname, pk.OriginalTrainerName, (ComboItem)HTTextVarBox.SelectedItem != null ? ((ComboItem)HTTextVarBox.SelectedItem).Text : HTargvals[0].Text, (string)HTIntenseBox.SelectedItem, (string)HTMoreThanAFeelingbox.SelectedItem);
            }
        }
        chlabel.Text = pk.CurrentHandler == 0 ? pk.OriginalTrainerName : pk.HandlingTrainerName;
        fullnesseditor.Text = pk.Fullness.ToString();
        EnjoymentEditor.Text = pk.Enjoyment.ToString();
    }
    public static MemoryStrings memorytext = new(GameInfo.Strings);

    private void SaveMemoriesAndClose(object sender, EventArgs e)
    {
        if (byte.TryParse(Friendshipeditor.Text, out var result))
        {
            if (result > 255)
                result = 255;

            pk.OriginalTrainerFriendship = result;
        }
        if (byte.TryParse(HTFriendshipeditor.Text, out var result2))
        {
            if (result2 > 255)
                result2 = 255;
            pk.HandlingTrainerFriendship = result2;
        }

        if (pk is ITrainerMemories mems)
        {
            var selectedmemorytype = (ComboItem)MemoryTypePicker.SelectedItem;
            mems.OriginalTrainerMemory = (byte)selectedmemorytype.Value;
            mems.OriginalTrainerMemoryVariable = (byte)((ComboItem)OTTextVarBox.SelectedItem).Value;
            mems.OriginalTrainerMemoryFeeling = (byte)MoreThanAFeelingbox.SelectedIndex;
            mems.OriginalTrainerMemoryIntensity = (byte)Intensebox.SelectedIndex;

            var selectedhtmemorytype = (ComboItem)HTMemoryTypePicker.SelectedItem;
            mems.HandlingTrainerMemory = (byte)selectedhtmemorytype.Value;
            mems.HandlingTrainerMemoryFeeling = (byte)HTMoreThanAFeelingbox.SelectedIndex;
            mems.HandlingTrainerMemoryIntensity = (byte)HTIntenseBox.SelectedIndex;
            mems.HandlingTrainerMemoryVariable = (byte)((ComboItem)HTTextVarBox.SelectedItem).Value;
        }
        if (byte.TryParse(fullnesseditor.Text, out var result3))
        {
            if (result3 > 255)
                result3 = 255;
            pk.Fullness = result3;
        }
        if (byte.TryParse(EnjoymentEditor.Text, out var result4))
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

    private void ChangeMemoryDisplay(object sender, EventArgs e)
    {
        if (((Picker)sender).SelectedIndex > 0)
        {
            var memindex = Memories.GetMemoryArgType((byte)((Picker)sender).SelectedIndex, pk.Format);
            var argvals = memorytext.GetArgumentStrings(memindex, pk.Format);
            OTTextVarBox.ItemsSource = argvals;
            OTTextVarBox.DisplayMemberPath = "Text";
            TextVar.Text = GetMemoryCategory(memindex, pk.Format);

            MoreThanAFeeling.IsVisible = true;
            MoreThanAFeelingbox.IsVisible = true;

            Intense.IsVisible = true;
            Intensebox.IsVisible = true;

            TextVar.IsVisible = true;
            OTTextVarBox.IsVisible = true;

            MemoryString.IsVisible = true;
            try
            {
                MemoryString.Text = string.Format(memorytext.Memory[MemoryTypePicker.SelectedIndex].Text, pk.Nickname, pk.OriginalTrainerName, (ComboItem)OTTextVarBox.SelectedItem != null ? ((ComboItem)OTTextVarBox.SelectedItem).Text : argvals[0].Text, (string)Intensebox.SelectedItem, (string)MoreThanAFeelingbox.SelectedItem);
            }
            catch (Exception) { }
        }
        else
        {
            MoreThanAFeeling.IsVisible = false;
            MoreThanAFeelingbox.IsVisible = false;
            Intense.IsVisible = false;
            Intensebox.IsVisible = false;
            TextVar.IsVisible = false;
            OTTextVarBox.IsVisible = false;
            MemoryString.IsVisible = false;
        }
    }
    public string GetMemoryCategory(MemoryArgType type, int memoryGen) => type switch
    {
        MemoryArgType.GeneralLocation => "Area:",
        MemoryArgType.SpecificLocation when memoryGen <= 7 => "Location:",
        MemoryArgType.Species => "Species:",
        MemoryArgType.Move => "Move:",
        MemoryArgType.Item => "Item:",
        _ => string.Empty,
    };
    private void UpdateMemoryString(object sender, EventArgs e)
    {
        var memindex = Memories.GetMemoryArgType((byte)MemoryTypePicker.SelectedIndex, pk.Format);
        var argvals = memorytext.GetArgumentStrings(memindex, pk.Format);
        if ((SfComboBox)sender == OTTextVarBox || (SfComboBox)sender == MoreThanAFeelingbox || (SfComboBox)sender == Intensebox)
            MemoryString.Text = string.Format(memorytext.Memory[MemoryTypePicker.SelectedIndex].Text, pk.Nickname, pk.OriginalTrainerName, (ComboItem)OTTextVarBox.SelectedItem != null ? ((ComboItem)OTTextVarBox.SelectedItem).Text : argvals[0].Text, (string)Intensebox.SelectedItem, (string)MoreThanAFeelingbox.SelectedItem);
        if ((SfComboBox)sender == HTTextVarBox || (SfComboBox)sender == HTMoreThanAFeelingbox || (SfComboBox)sender == HTIntenseBox)
        {
            memindex = Memories.GetMemoryArgType((byte)HTMemoryTypePicker.SelectedIndex, pk.Format);
            argvals = memorytext.GetArgumentStrings(memindex, pk.Format);
            HTMemoryString.Text = string.Format(memorytext.Memory[HTMemoryTypePicker.SelectedIndex].Text, pk.Nickname, pk.OriginalTrainerName, (ComboItem)HTTextVarBox.SelectedItem != null ? ((ComboItem)HTTextVarBox.SelectedItem).Text : argvals[0].Text, (string)HTIntenseBox.SelectedItem, (string)HTMoreThanAFeelingbox.SelectedItem);
        }
    }
    private void HTChangeMemoryDisplay(object sender, EventArgs e)
    {
        if (((Picker)sender).SelectedIndex > 0)
        {
            var memindex = Memories.GetMemoryArgType((byte)((Picker)sender).SelectedIndex, pk.Format);
            var argvals = memorytext.GetArgumentStrings(memindex, pk.Format);
            HTTextVarBox.ItemsSource = argvals;
            HTTextVarBox.DisplayMemberPath = "Text";
            HTTextVar.Text = GetMemoryCategory(memindex, pk.Format);

            HTMoreThanAFeeling.IsVisible = true;
            HTMoreThanAFeelingbox.IsVisible = true;

            HTIntense.IsVisible = true;
            HTIntenseBox.IsVisible = true;

            HTTextVar.IsVisible = true;
            HTTextVarBox.IsVisible = true;

            HTMemoryString.IsVisible = true;
            try
            {
                HTMemoryString.Text = string.Format(memorytext.Memory[HTMemoryTypePicker.SelectedIndex].Text, pk.Nickname, pk.OriginalTrainerName, (ComboItem)HTTextVarBox.SelectedItem != null ? ((ComboItem)HTTextVarBox.SelectedItem).Text : argvals[0].Text, (string)HTIntenseBox.SelectedItem, (string)HTMoreThanAFeelingbox.SelectedItem);
            }
            catch (Exception) { }
        }
        else
        {
            HTMoreThanAFeeling.IsVisible = false;
            HTMoreThanAFeelingbox.IsVisible = false;
            HTIntense.IsVisible = false;
            HTIntenseBox.IsVisible = false;
            HTTextVar.IsVisible = false;
            HTTextVarBox.IsVisible = false;
            HTMemoryString.IsVisible = false;
        }
    }
    private void ChangeComboBoxFontColor(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        SfComboBox box = (SfComboBox)sender;
        box.TextColor = box.IsDropDownOpen ? Colors.Black : Colors.White;
    }
}