using Microsoft.Maui.Controls;
using static PKHeXMAUI.MainPage;
using PKHeX.Core;
using Syncfusion.Maui.Inputs;
using Syncfusion.Maui.DataSource.Extensions;
using System.Windows.Input;
using PKHeX.Core.Injection;

namespace PKHeXMAUI;

public partial class Items : TabbedPage
{
    public static string[] itemlist = [.. GameInfo.Strings.GetItemStrings(sav.Context, sav.Version)];
    public List<List<itemInfo>> SourceList = [];
    private readonly IReadOnlyList<InventoryPouch> pouches;
    private readonly SaveFile Origin;
    private readonly SaveFile SAV;
    public static int currentcount = 995;
    public Items()
	{
        
        InitializeComponent();
        if (Remote.Connected)
        {
            var success = Remote.Injector.ReadBlockFromString(Remote, sav, "Items", out var data);
            if(success)
            {
                switch (sav)
                {
                    case SAV9SV s: data.ToArray()[0].CopyTo(s.Items.Data, 0); break;
                    case SAV8LA la: data.ToArray()[0].CopyTo(la.Items.Data, 0); break;
                    case SAV8BS bs: data.ToArray()[0].CopyTo(bs.Items.Data, 0); break;
                    case SAV8SWSH sw: data.ToArray()[0].CopyTo(sw.Items.Data, 0); break;
                    case SAV7 s7: data.ToArray()[0].CopyTo(s7.Items.Data, 0); break;
                    case SAV6 s6: data.ToArray()[0].CopyTo(s6.Items.Data, 0); break;
                }
            }
            else
            {
                DisplayAlert("Error", "No Data Found, I guess", "okay...");
            }
        }
        SAV = (Origin = sav).Clone();
        pouches = sav.Inventory;

        foreach (var pouch in pouches)
        {
            var content = new ContentPage() { Title = pouch.Type.ToString() };
            Grid header = [];
            header.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Star });
            header.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Star });
            header.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Star });
            header.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Star });
            Label headerItem = new() { Text = "Item" };
            header.Add(headerItem);
            Label headerCount = new() { Text = "Count", HorizontalOptions = LayoutOptions.End };
            header.Add(headerCount, 1);
            Label headerFav = new() { Text = "Fav", HorizontalOptions = LayoutOptions.End };
            header.Add(headerFav,2);
            Label headerNew = new() { Text = "New", HorizontalOptions = LayoutOptions.Center};
            header.Add(headerNew,3);
            var ItemCollection = new CollectionView
            {
                WidthRequest = 400,
                HeightRequest = 500,
                ItemTemplate = new DataTemplate(() =>
            {
                Grid grid = [];
                grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(2, GridUnitType.Star) });
                grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Star });
                grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Star });
                grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Star });
                SfComboBox itemname = new() { Placeholder = "(None)", IsEditable = true, TextSearchMode = ComboBoxTextSearchMode.StartsWith, BackgroundColor = Colors.Transparent, MaxDropDownHeight = 500 };
                itemname.PropertyChanged += ChangeComboBoxFontColor;
                var pouchstrings = GetStringsForPouch(pouch.GetAllItems());
                itemname.ItemsSource = pouchstrings;
                itemname.SetBinding(SfComboBox.SelectedItemProperty, "name", mode: BindingMode.TwoWay);
                itemname.IsClearButtonVisible = false;
                grid.Add(itemname);
                Editor itemCount = new();
                itemCount.SetBinding(Editor.TextProperty, "count", mode: BindingMode.TwoWay);
                grid.Add(itemCount, 1);
                CheckBox ItemFavCheck = new();
                ItemFavCheck.SetBinding(CheckBox.IsCheckedProperty, "isfav", mode: BindingMode.TwoWay);
                grid.Add(ItemFavCheck, 2);
                CheckBox ItemNewCheck = new();
                ItemNewCheck.SetBinding(CheckBox.IsCheckedProperty, "isnew", BindingMode.TwoWay);
                grid.Add(ItemNewCheck, 3);
                return grid;
            })
            };
            var infolist = new List<itemInfo>();
            foreach(var item in pouch.Items)
            {
                infolist.Add(new itemInfo(item));
            }
            SourceList.Add(infolist);
            ItemCollection.ItemsSource = infolist;
            Button GiveAll = new() { Text = "Give All" };
            GiveAll.Clicked += GiveAll_Clicked;
            ToolTipProperties.SetText(GiveAll, "Gives you every item for this bag at the Count above even if you don't have it");
            Button ModifyAll = new() { Text = "Modify All" };
            ModifyAll.Clicked += ModifyAll_Clicked;
            ToolTipProperties.SetText(ModifyAll, "Gives you the Count above for any item you already have");
            Editor GiveCount = new() { Text = "995" };
            GiveCount.TextChanged += SetCount;
            Button ClearAll = new() { Text = "Clear All" };
            ToolTipProperties.SetText(ClearAll, "Clears the current bag");
            ClearAll.Clicked += ClearAll_Clicked;
            var itemrefresh = new RefreshView();
            var itemscroll = new ScrollView
            {
                Content = new StackLayout()
                {
                    Children =
                {
                    header, ItemCollection, GiveCount,GiveAll,ModifyAll,ClearAll
                }
                }
            };
            itemrefresh.Content = itemscroll;
            ICommand refreshview = new Command(async() =>
            {
                var pindex = Array.IndexOf([.. ItemsMain.Children], ItemsMain.CurrentPage) - 1;
                ItemCollection.ItemsSource = SourceList[pindex];
                itemrefresh.IsRefreshing = false;
            });
            itemrefresh.Command = refreshview;
            content.Content = itemrefresh;
            ItemsMain.Children.Add(content);
        }
        ItemsMain.CurrentPageChanged += SetCount;
    }
    private void ClearAll_Clicked(object sender, EventArgs e)
    {
        var pindex = Array.IndexOf([.. ItemsMain.Children], ItemsMain.CurrentPage) - 1;
        var list = SourceList[pindex];
        list.Clear();
        var pouchlist = pouches[pindex];
        pouchlist.RemoveAll();
        foreach (var item in pouchlist.Items)
        {
            list.Add(new itemInfo(item));
        }
        SourceList[pindex] = list;
    }
    private void SetCount(object sender, EventArgs e)
    {
        if (sender is Editor ed)
        {
            var parsed = int.TryParse(ed.Text, out var count);
            if (parsed)
                currentcount = count;
        }
        else
        {
            currentcount = 995;
        }
    }
    private void GiveAll_Clicked(object sender, EventArgs e)
    {
        var pindex = Array.IndexOf([.. ItemsMain.Children], ItemsMain.CurrentPage)-1;
        var list = SourceList[pindex];
        list.Clear();
        var pouchlist = pouches[pindex];
        var allitems = pouchlist.GetAllItems();
        pouchlist.GiveAllItems(sav, allitems, currentcount);
        foreach(var item in pouchlist.Items)
        {
            list.Add(new itemInfo(item));
        }
        SourceList[pindex] = list;
    }
    private void ModifyAll_Clicked(object sender, EventArgs e)
    {
        var pindex = Array.IndexOf([.. ItemsMain.Children], ItemsMain.CurrentPage) - 1;
        var list = SourceList[pindex];
        list.Clear();
        var pouchlist = pouches[pindex];
        pouchlist.ModifyAllCount(sav,currentcount);
        foreach (var item in pouchlist.Items)
        {
            list.Add(new itemInfo(item));
        }
        SourceList[pindex] = list;
    }

    private void ChangeComboBoxFontColor(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        SfComboBox box = (SfComboBox)sender;
        box.TextColor = box.IsDropDownOpen ? Colors.Black : Colors.White;
    }
    private string[] GetStringsForPouch(ReadOnlySpan<ushort> items, bool sort = true)
    {
        string[] res = new string[items.Length + 1];
        for (int i = 0; i < res.Length - 1; i++)
            res[i] = itemlist[items[i]];
        res[items.Length] = itemlist[0];
        if (sort)
            Array.Sort(res);
        return res;
    }

    private async void SaveItemsClicked(object sender, EventArgs e)
    {
        int i = 0;
        saveitems.Text = "saving...";
        Task.Delay(100);
       foreach(var pouch in pouches)
        {
            await setbag(pouch,i);
            i++;
        }
        SAV.Inventory = pouches;
        Origin.CopyChangesFrom(SAV);

        await Navigation.PopModalAsync();
    }
    private async Task setbag(InventoryPouch pouch,int sourceindex)
    {
        int ctr = 0;
        var list = SourceList[sourceindex];
            foreach (var it in list)
            {
                var itemindex = Array.IndexOf(itemlist, it.name);
                var validct = int.TryParse(it.count, out var itemct);
                if (itemindex <= 0) // Compression of Empty Slots
                    continue;
                if (!validct)
                    continue;

                var item = pouch.GetEmpty(itemindex, itemct);
                if (item is IItemFavorite f)
                    f.IsFavorite = it.isfav;
                if (item is IItemNewFlag n)
                    n.IsNew = it.isnew;
                if (item is IItemFreeSpace fs)
                    fs.IsFreeSpace = it.isfreespace;
                if (item is IItemFreeSpaceIndex fi)
                    fi.FreeSpaceIndex = it.isfreespaceindex;
                pouch.Items[ctr] = item;
                ctr++;
            }

            for (int i = ctr; i < pouch.Items.Length; i++)
                pouch.Items[i] = pouch.GetEmpty(); // Empty Slots at the end
    }

    private void CloseItems(object sender, EventArgs e)
    {
        Navigation.PopModalAsync();
    }
}
public class itemInfo
{
    public string count { get; set; }
    public string name { get; set; }
    public bool isfav { get; set; }
    public bool isnew { get; set; }
    public bool isfreespace { get; set; }
    public uint isfreespaceindex { get; set; }
    public itemInfo(InventoryItem item)
    {
        count = item.Count.ToString();
        name = Items.itemlist[item.Index];
        if (item is IItemFavorite f)
            isfav = f.IsFavorite;
        if (item is IItemNewFlag n)
            isnew = n.IsNew;
        if (item is IItemFreeSpace fs)
            isfreespace = fs.IsFreeSpace;
        if (item is IItemFreeSpaceIndex fi)
            isfreespaceindex = fi.FreeSpaceIndex;
    }
}