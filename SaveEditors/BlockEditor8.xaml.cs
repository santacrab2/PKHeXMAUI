using PKHeX.Core;
using Syncfusion.Maui.Inputs;
namespace PKHeXMAUI;

public partial class BlockEditor8 :ContentPage
{
    private readonly ISCBlockArray SAV;
    private readonly SCBlockMetadata Metadata;
    private readonly ComboItem[] SortedBlockKeys;

    private SCBlock CurrentBlock = null!;
    private string Filter = string.Empty;
    public BlockEditor8(ISCBlockArray sav)
	{
		InitializeComponent();
        SAV = sav;
        Metadata = new SCBlockMetadata(sav.Accessor, [], []);
        SortedBlockKeys = Metadata.GetSortedBlockKeyList().ToArray();
        BlockKey_Picker.ItemsSource = SortedBlockKeys;
        BlockKey_Picker.DisplayMemberPath = "Text";
    }

    private void Update_BlockCV(object sender, Syncfusion.Maui.Inputs.SelectionChangedEventArgs e)
    {
        try
        {
            var key = (uint)((ComboItem)BlockKey_Picker.SelectedItem).Value;
            CurrentBlock = SAV.Accessor.GetBlock(key);
            UpdateBlockSummaryControls();
        }
        catch (Exception)
        {
            BlockStack.Clear();
        }
    }
    private void UpdateBlockSummaryControls()
    {
        BlockStack.Clear();
        BlockEditor_Hex.IsVisible = true;
        var block = CurrentBlock;
        var blockName = Metadata.GetBlockName(block, out var obj);
        BlockEditor_Hex.Text = string.Join(" ", block.Data.Select(z => $"{z:X2}"));
        
        if (obj != null)
        {
            var props = ReflectUtil.GetPropertiesCanWritePublicDeclared(obj.GetType());
            if (props.Count() > 1)
            {
                int row = 0;
                BlockEditor_Hex.IsVisible = false;
                foreach(var prop in props)
                {

                    BlockStack.Add(new Label() { Text = prop},0,row);
                    var BlockEntry = new Entry();
                    BlockEntry.BindingContext = obj;
                    BlockEntry.SetBinding(Entry.TextProperty, prop, BindingMode.TwoWay);
                    BlockStack.Add(BlockEntry,1,row);
                    row++;
                }
            }
        }
    }

    private void CloseBlockEditor8(object sender, EventArgs e)
    {
        Navigation.PopModalAsync();
    }
}