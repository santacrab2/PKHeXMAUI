using PKHeX.Core;
using static PKHeXMAUI.MainPage;
namespace PKHeXMAUI;

public partial class MoveShopEditor : ContentPage
{
	public MoveShopEditor()
	{
		InitializeComponent();
		MoveShopList = [];
		moveshopcollection.ItemTemplate = new DataTemplate(() =>
		{
			Grid grid = new() { Padding = 10 };
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = 100 });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = 100 });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = 100 });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = 100 });
			Label index = new();
			index.SetBinding(Label.TextProperty, new Binding("index"));
			grid.Add(index, 0, 0);
			Label name = new();
			name.SetBinding(Label.TextProperty, new Binding("name"));
			grid.Add(name, 1, 0);
			CheckBox purchased = new();
			purchased.SetBinding(CheckBox.IsCheckedProperty, new Binding("purchased", BindingMode.TwoWay));
			grid.Add(purchased,2,0);
			CheckBox mastered = new();
			mastered.SetBinding(CheckBox.IsCheckedProperty, new Binding("mastered",BindingMode.TwoWay));
			grid.Add(mastered, 3, 0);
            return grid;
		});
        var names = GameInfo.Strings.Move;
		if (pk is IMoveShop8Mastery Shop)
		{
			var indexes = Shop.Permit.RecordPermitIndexes;
			for (int i = 0; i < indexes.Length; i++)
			{
				var name = names[indexes[i]];
				var item = new MoveShopItem(name, i, Shop);
				MoveShopList.Add(item);
            }
		}
		moveshopcollection.ItemsSource = MoveShopList;
    }

	public static List<MoveShopItem> MoveShopList = [];

    private void GiveAllMoveShop(object sender, EventArgs e)
    {
		if(pk is IMoveShop8Mastery master)
		{
			master.SetMoveShopFlags(pk);
		}
		Navigation.PopModalAsync();
    }

    private void RemoveAllMoveShop(object sender, EventArgs e)
    {
		if(pk is IMoveShop8Mastery master)
		{
			master.ClearMoveShopFlagsMastered();
			master.ClearMoveShopFlags();
		}
		Navigation.PopModalAsync();
    }

    private void SaveMoveShop(object sender, EventArgs e)
    {
		foreach(var item in MoveShopList)
		{
			if(pk is IMoveShop8Mastery master)
			{
				master.SetPurchasedRecordFlag(int.Parse(item.index)-1, item.purchased);
				master.SetMasteredRecordFlag(int.Parse(item.index)-1, item.mastered);
			}
		}
		Navigation.PopModalAsync();
    }

    private void CloseUpShop(object sender, EventArgs e)
    {
		Navigation.PopModalAsync();
    }
}

public class MoveShopItem(string Name, int Index, IMoveShop8Mastery master)
{
    public string index { get; set; } = $"{Index + 1:00}";
    public string name { get; set; } = Name;

    public bool purchased { get; set; } = master.GetPurchasedRecordFlag(Index);
    public bool mastered { get; set; } = master.GetMasteredRecordFlag(Index);
}
