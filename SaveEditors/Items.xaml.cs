using static PKHeXMAUI.MainPage;
using PKHeX.Core;
using Syncfusion.Maui.Inputs;
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
            header.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Star });
            Label headerItem = new() { Text = "Item" };
            header.Add(headerItem,1);
            Label headerCount = new() { Text = "Count", HorizontalOptions = LayoutOptions.End };
            header.Add(headerCount, 2);
            Label headerFav = new() { Text = "Fav", HorizontalOptions = LayoutOptions.End };
            header.Add(headerFav,3);
            Label headerNew = new() { Text = "New", HorizontalOptions = LayoutOptions.Center};
            header.Add(headerNew,4);
            var ItemCollection = new CollectionView
            {
                WidthRequest = 400,
                HeightRequest = 500,
                ItemTemplate = new DataTemplate(() =>
            {
                Grid grid = [];
                grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Star });
                grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(2, GridUnitType.Star) });
                grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Star });
                grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Star });
                grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Star });
                SfComboBox itemname = new() { Placeholder = "(None)", IsEditable = true, TextSearchMode = ComboBoxTextSearchMode.StartsWith, BackgroundColor = Colors.Transparent, MaxDropDownHeight = 500 };
                itemname.PropertyChanged += ChangeComboBoxFontColor;
                itemname.SelectionChanged += ChangeItemSprite;
                var pouchstrings = GetStringsForPouch(pouch.GetAllItems());
                itemname.ItemsSource = pouchstrings;
                itemname.SetBinding(SfComboBox.SelectedItemProperty, "name", mode: BindingMode.TwoWay);
                itemname.IsClearButtonVisible = false;
                grid.Add(itemname, 1);
                Image itemsp = new() { HorizontalOptions = LayoutOptions.Start, HeightRequest = 25, WidthRequest = 25 };
                itemsp.SetBinding(Image.SourceProperty, "itemsprite");
                grid.Add(itemsp);
                Editor itemCount = new();
                itemCount.SetBinding(Editor.TextProperty, "count", mode: BindingMode.TwoWay);
                grid.Add(itemCount, 2);
                CheckBox ItemFavCheck = new();
                ItemFavCheck.SetBinding(CheckBox.IsCheckedProperty, "isfav", mode: BindingMode.TwoWay);
                grid.Add(ItemFavCheck, 3);
                CheckBox ItemNewCheck = new();
                ItemNewCheck.SetBinding(CheckBox.IsCheckedProperty, "isnew", BindingMode.TwoWay);
                grid.Add(ItemNewCheck, 4);
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
        if (Remote.Connected)
        {
            if(Remote.Injector is LPBDSP)
            {
                try
                {
                    Remote.Injector.WriteBlockFromString(Remote, "Items", ((SAV8BS)Origin).Items.Data, ((SAV8BS)Origin).Items);
                }
                catch (Exception) { }
            }
            else
            {
                try
                {
                    Remote.Injector.WriteBlocksFromSAV(Remote, "Items", Origin);
                }
                catch (Exception) { }
            }
        }
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
    private void ChangeItemSprite(object sender, EventArgs e)
    {
        var pindex = Array.IndexOf([.. ItemsMain.Children], ItemsMain.CurrentPage) - 1;
        var CurrentSource = SourceList[pindex];
        itemInfo? CurrentItem = CurrentSource.Find(z => z.name == (string)((Syncfusion.Maui.Inputs.SelectionChangedEventArgs)e).CurrentSelection[0]);
        if (CurrentItem is not null)
        {
            var lump = HeldItemLumpUtil.GetIsLump(CurrentItem.InvItem.Index, sav.Context);
            CurrentItem.itemsprite = sav is SAV9SV ? lump is HeldItemLumpImage.TechnicalMachine ? "aitem_tm.png" : lump is HeldItemLumpImage.TechnicalRecord ? "aitem_tr.png" : $"aitem_{Array.IndexOf(itemlist, CurrentItem.name)}.png" : lump is HeldItemLumpImage.TechnicalMachine ? "bitem_tm.png" : lump is HeldItemLumpImage.TechnicalRecord ? "bitem_tr.png" : $"bitem_{Array.IndexOf(itemlist, CurrentItem.name)}.png";
            if (itemInfo.Pouch_Material_SV.Contains((ushort)CurrentItem.InvItem.Index))
                CurrentItem.itemsprite = "aitem_material.png";
            if (CurrentItem.InvItem.Index >= 2522 && CurrentItem.InvItem.Index <= 2546)
                CurrentItem.itemsprite = "aitem_snack.png";
            if (itemInfo.Pouch_Picnic.Contains((ushort)CurrentItem.InvItem.Index))
                CurrentItem.itemsprite = "aitem_picnic.png";
            SourceList[pindex] = CurrentSource;
        }
    }
}
public class itemInfo
{
    public string count { get; set; }
    public string name { get; set; }
    public bool isfav { get; set; }
    public bool isnew { get; set; }
    public string itemsprite { get; set; }
    public bool isfreespace { get; set; }
    public uint isfreespaceindex { get; set; }
    public InventoryItem InvItem { get; set; }
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
        var lump = HeldItemLumpUtil.GetIsLump(item.Index, sav.Context);
        itemsprite = sav is SAV9SV ? lump is HeldItemLumpImage.TechnicalMachine ? "aitem_tm.png" : lump is HeldItemLumpImage.TechnicalRecord ? "aitem_tr.png" : $"aitem_{item.Index}.png" : lump is HeldItemLumpImage.TechnicalMachine ? "bitem_tm.png" : lump is HeldItemLumpImage.TechnicalRecord ? "bitem_tr.png" : $"bitem_{item.Index}.png";
        if (Pouch_Material_SV.Contains((ushort)item.Index))
            itemsprite = "aitem_material.png";
        if (item.Index >= 2522 && item.Index <= 2546)
            itemsprite = "aitem_snack.png";
        if (Pouch_Picnic.Contains((ushort)item.Index))
            itemsprite = "aitem_picnic.png";
        InvItem = item;
    }
     public static List<ushort> Pouch_Material_SV =
    [
        1956, 1957, 1958, 1959, 1960, 1961, 1962, 1963, 1964, 1965,
        1966, 1967, 1968, 1969, 1970, 1971, 1972, 1973, 1974, 1975,
        1976, 1977, 1978, 1979, 1980, 1981, 1982, 1983, 1984, 1985,
        1986, 1987, 1988, 1989, 1990, 1991, 1992, 1993, 1994, 1995,
        1996, 1997, 1998, 1999, 2000, 2001, 2002, 2003, 2004, 2005,
        2006, 2007, 2008, 2009, 2010, 2011, 2012, 2013, 2014, 2015,
        2016, 2017, 2018, 2019, 2020, 2021, 2022, 2023, 2024, 2025,
        2026, 2027, 2028, 2029, 2030, 2031, 2032, 2033, 2034, 2035,
        2036, 2037, 2038, 2039, 2040, 2041, 2042, 2043, 2044, 2045,
        2046, 2047, 2048, 2049, 2050, 2051, 2052, 2053, 2054, 2055,
        2056, 2057, 2058, 2059, 2060, 2061, 2062, 2063, 2064, 2065,
        2066, 2067, 2068, 2069, 2070, 2071, 2072, 2073, 2074, 2075,
        2076, 2077, 2078, 2079, 2080, 2081, 2082, 2083, 2084, 2085,
        2086, 2087, 2088, 2089, 2090, 2091, 2092, 2093, 2094, 2095,
        2096, 2097, 2098, 2099, 2103, 2104, 2105, 2106, 2107, 2108,
        2109, 2110, 2111, 2112, 2113, 2114, 2115, 2116, 2117, 2118,
        2119, 2120, 2121, 2122, 2123, 2126, 2127, 2128, 2129, 2130,
        2131, 2132, 2133, 2134, 2135, 2136, 2137, 2156, 2157, 2158,
        2159, 2438, 2439, 2440, 2441, 2442, 2443, 2444, 2445, 2446,
        2447, 2448, 2449, 2450, 2451, 2452, 2453, 2454, 2455, 2456,
        2457, 2458, 2459, 2460, 2461, 2462, 2463, 2464, 2465, 2466,
        2467, 2468, 2469, 2470, 2471, 2472, 2473, 2474, 2475, 2476,
        2477, 2478, 2484, 2485, 2486, 2487, 2488, 2489, 2490, 2491,
        2492, 2493, 2494, 2495, 2496, 2497, 2498, 2499, 2500, 2501,
        2502, 2503, 2504, 2505, 2506, 2507, 2508, 2509, 2510, 2511,
        2512, 2513, 2514, 2515, 2516, 2517, 2518, 2519, 2520, 2521,
    ];
    public static List<ushort> Pouch_Picnic =
    [
        2311,
        2313, 2314, 2315, 2316, 2317, 2318, 2319, 2320, 2321, 2322,
        2323, 2324, 2325, 2326, 2327, 2329, 2330, 2331, 2332, 2333,
        2334, 2335, 2336, 2337, 2338, 2339, 2340, 2341, 2342, 2348,
        2349, 2350, 2351, 2352, 2353, 2354, 2355, 2356, 2357, 2358,
        2359, 2360, 2361, 2362, 2363, 2364, 2365, 2366, 2367, 2368,
        2369, 2370, 2371, 2372, 2373, 2374, 2375, 2376, 2377, 2378,
        2379, 2380, 2381, 2382, 2383, 2384, 2385, 2386, 2387, 2388,
        2389, 2390, 2391, 2392, 2393, 2394, 2395, 2396, 2397, 2398,
        2399, 2400, 2417, 2418, 2419, 2420, 2421, 2422, 2423, 2424,
        2425, 2426, 2427, 2428, 2429, 2430, 2431, 2432, 2433, 2434,
        2435, 2436, 2437, 2548, 2551, 2552,

    ];
}