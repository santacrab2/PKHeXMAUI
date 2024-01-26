using PKHeX.Core;
using Syncfusion.Maui.Inputs;
namespace PKHeXMAUI
{

    public partial class BlockEditor8 : ContentPage
    {
        private readonly ISCBlockArray SAV;
        private readonly SCBlockMetadata Metadata;
        public static ComboItem[] SortedBlockKeys;

        private SCBlock CurrentBlock = null!;

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
            BlockSummary.IsVisible = true;
            BlockSummary.Text = SCBlockUtil.GetBlockSummary(CurrentBlock);
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
                    foreach (var prop in props)
                    {
                        BlockStack.Add(new Label() { Text = prop }, 0, row);
                        var BlockEntry = new Entry();
                        BlockEntry.BindingContext = obj;
                        BlockEntry.SetBinding(Entry.TextProperty, prop, BindingMode.TwoWay);
                        BlockStack.Add(BlockEntry, 1, row);
                        row++;
                    }
                }
            }
        }

        private void CloseBlockEditor8(object sender, EventArgs e)
        {
            Navigation.PopModalAsync();
        }
        private void ChangeComboBoxFontColor(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            SfComboBox box = (SfComboBox)sender;
            box.TextColor = box.IsDropDownOpen ? Colors.Black : Colors.White;
        }
    }
    public class BlockDataFilter : IComboBoxFilterBehavior
    {
        public List<int> GetMatchingIndexes(SfComboBox source, ComboBoxFilterInfo filterInfo)
        {
            List<int> filteredlist = new List<int>();
            List<ComboItem> SourceList = ((ComboItem[])source.ItemsSource).ToList();
            var text = filterInfo.Text;
            if (text.Length == 8)
            {
                var hex = (int)Util.GetHexValue(text);
                if (hex != 0)
                {
                    // Input is hexadecimal number, select the item
                    filteredlist.Add(BlockEditor8.SortedBlockKeys.ToList().IndexOf(BlockEditor8.SortedBlockKeys.ToList().Find(z => z.Value == hex)));
                    return filteredlist;
                }
            }
            filteredlist.AddRange(from ComboItem item in BlockEditor8.SortedBlockKeys where item.Text.Contains(filterInfo.Text, StringComparison.InvariantCultureIgnoreCase) select BlockEditor8.SortedBlockKeys.ToList().IndexOf(item));
            return filteredlist;
        }
    }
}