using CommunityToolkit.Maui.Storage;
using PKHeX.Core;
using Syncfusion.Maui.Inputs;
using System.Security.Cryptography;
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

        private async void ExportBlocksFolder(object sender, EventArgs e)
        {
            var FolderResults = await FolderPicker.PickAsync(CancellationToken.None);
            var path = FolderResults.Folder.Path;
            var blocks = SAV.AllBlocks;
            ExportAllBlocks(blocks, path);
        }
        private static void ExportAllBlocks(IEnumerable<SCBlock> blocks, string path)
        {
            foreach (var b in blocks.Where(z => z.Data.Length != 0))
            {
                var fn = $"{SCBlockUtil.GetBlockFileNameWithoutExtension(b)}.bin";
                var fileName = Path.Combine(path, fn);
                File.WriteAllBytes(fileName, b.Data);
            }
        }

        private async void ImportBlocksFolder(object sender, EventArgs e)
        {
            var FolderResults = await FolderPicker.PickAsync(CancellationToken.None);
            if (FolderResults.IsSuccessful) 
            {
                var failed = SCBlockUtil.ImportBlocksFromFolder(FolderResults.Folder.Path,SAV);
                if(failed.Count != 0)
                {
                    var msg = string.Join(Environment.NewLine, failed);
                    await DisplayAlert("Failed", $"Failed to import: {msg}", "cancel");
                }

            }
        }

        private void ExportCurrentBlock_Clicked(object sender, EventArgs e) => ExportSelectBlock(CurrentBlock);

        private async void ExportSelectBlock(SCBlock block)
        {
            var name = SCBlockUtil.GetBlockFileNameWithoutExtension(block);
            using var BlockStreams = new MemoryStream(block.Data);
            var result = await FileSaver.SaveAsync($"{name}.bin", BlockStreams, CancellationToken.None);
            if (result.IsSuccessful)
                await DisplayAlert("Success", $"Block File saved at {result.FilePath}", "cancel");
            else
                await DisplayAlert("Failure", $"Block File did not save due to {result.Exception.Message}", "cancel");
        }

        private void ImportCurrentBlock_Clicked(object sender, EventArgs e) => ImportSelectBlock(CurrentBlock);
        private async void ImportSelectBlock(SCBlock blockTarget)
        {
            var Pickedfile = await FilePicker.PickAsync();
            if (Pickedfile is null)
                return;
            var key = blockTarget.Key;
            var data = blockTarget.Data;
            var path = Pickedfile.FileName;
            var file = new FileInfo(path);
            if(file.Length != data.Length)
            {
                await DisplayAlert("Error", string.Format(MessageStrings.MsgFileSize, $"0x{file.Length:X8}"), "cancel");
                return;
            }
           var newdata = File.ReadAllBytes(path);
            blockTarget.ChangeData(newdata);
        }
    }
    public class BlockDataFilter : IComboBoxFilterBehavior
    {
        public List<int> GetMatchingIndexes(SfComboBox source, ComboBoxFilterInfo filterInfo)
        {
            List<int> filteredlist = [];
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