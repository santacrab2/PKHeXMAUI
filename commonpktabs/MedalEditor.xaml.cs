using static PKHeXMAUI.MainPage;
using PKHeX.Core;




namespace PKHeXMAUI;

public partial class MedalEditor : ContentPage
{
    public static List<RegimenInfo> SuperTrainInfo = new();
    public static List<RegimenInfo> DistSuperTrain = new();
	public MedalEditor()
	{
		InitializeComponent();
        SuperTrainInfo = new();
        if (pk is ISuperTrain pk7)
        {
            SuperTrainInfo.AddRange(GetBooleanRegimenNames(pk7, "SuperTrain"));
            DistSuperTrain.AddRange(GetBooleanRegimenNames(pk7, "DistSuperTrain"));
        }
        rankside.ItemTemplate = new DataTemplate(() =>
        {
            Grid grid = new();
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = 150 });
            CheckBox IsComplete = new() { HorizontalOptions = LayoutOptions.Start, VerticalOptions = LayoutOptions.Center};
            IsComplete.SetBinding(CheckBox.IsCheckedProperty, "CompletedRegimen",BindingMode.TwoWay);
            grid.Add(IsComplete);
            Label RegimenName = new() { HorizontalOptions = LayoutOptions.End, VerticalOptions = LayoutOptions.Center};
            RegimenName.SetBinding(Label.TextProperty, "text");
            RegimenName.TextColor = Colors.White;
            grid.Add(RegimenName);
            return grid;
        });
        rankside.ItemsSource = SuperTrainInfo;
        distsuperside.ItemTemplate = new DataTemplate(() =>
        {
            Grid grid = new();
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = 150 });
            CheckBox IsComplete = new() { HorizontalOptions = LayoutOptions.Start, VerticalOptions = LayoutOptions.Center };
            IsComplete.SetBinding(CheckBox.IsCheckedProperty, "CompletedRegimen", BindingMode.TwoWay);
            grid.Add(IsComplete);
            Label RegimenName = new() { HorizontalOptions = LayoutOptions.End, VerticalOptions = LayoutOptions.Center };
            RegimenName.SetBinding(Label.TextProperty, "text");
            RegimenName.TextColor = Colors.White;
            grid.Add(RegimenName);
            return grid;
        });
        distsuperside.ItemsSource = DistSuperTrain;
    }
    private static IEnumerable<RegimenInfo> GetBooleanRegimenNames(ISuperTrain super, string propertyPrefix)
    {
        var names = ReflectUtil.GetPropertiesStartWithPrefix(super.GetType(), propertyPrefix);
        foreach (var name in names)
        {
            var value = ReflectUtil.GetValue(super, name);
            if (value is bool state)
                yield return new RegimenInfo(name, state);
        }
    }

    private void SaveST(object sender, EventArgs e)
    {
        if (pk is ISuperTrain super) 
        {
            foreach (var reg in SuperTrainInfo)
                ReflectUtil.SetValue(super, reg.text, reg.CompletedRegimen);
            foreach(var reg in DistSuperTrain)
                ReflectUtil.SetValue(super,reg.text, reg.CompletedRegimen);
        }
        Navigation.PopModalAsync();
    }

    private void ApplyAllST(object sender, EventArgs e)
    {
        if (pk is ISuperTrain super) 
        {
            foreach (var reg in SuperTrainInfo)
            {
                reg.CompletedRegimen = true;
                ReflectUtil.SetValue(super, reg.text, reg.CompletedRegimen);
                Navigation.PopModalAsync();
            }
        }
    }

    private void RemoveAllST(object sender, EventArgs e)
    {
        if (pk is ISuperTrain super)
        {
            foreach (var reg in SuperTrainInfo)
            {
                reg.CompletedRegimen = false;
                ReflectUtil.SetValue(super, reg.text, reg.CompletedRegimen);
                Navigation.PopModalAsync();
            }
        }
    }

    private void CloseST(object sender, EventArgs e)
    {
        Navigation.PopModalAsync();
    }
}

public class RegimenInfo
{
    public string text { get; set; }
    public bool CompletedRegimen { get; set; }

    public RegimenInfo(string name, bool completedRegimen)
    {
        text = name;
        CompletedRegimen = completedRegimen;
    }
}