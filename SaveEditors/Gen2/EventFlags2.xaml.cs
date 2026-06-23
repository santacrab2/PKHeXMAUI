
using static PKHeXMAUI.MainPage;
using PKHeX.Core;
namespace PKHeXMAUI;

public partial class EventFlags2 : ContentPage
{
    public EventWorkspace<SAV2, byte> Editor;
    public Dictionary<string,bool> ValueDict = [];
    public EventFlags2()
	{
		InitializeComponent();
        ValueDict = [];
		var editor = Editor = new EventWorkspace<SAV2, byte>((SAV2)sav, sav.Version);
        FlagCollection.ItemTemplate = new DataTemplate(() =>
        {
            var grid = new Grid() { Padding = 10 };
            grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Star });
            grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(3,GridUnitType.Star) });
            var check = new CheckBox() { VerticalOptions = LayoutOptions.Center };
            check.SetBinding(CheckBox.IsCheckedProperty, new Binding("Value"));
            grid.Add(check);
            var label = new Label();
            label.SetBinding(Label.TextProperty, new Binding("Key"));
            grid.Add(label, 1);

            var tap2 = new TapGestureRecognizer
            {
                CommandParameter = grid,
                Command = new Command(() => tapp(grid))
            };
            check.GestureRecognizers.Add(tap2);
            return grid;
        });
        AddFlagList(editor.Labels, editor.Flags);
    }
    private void AddFlagList(EventLabelCollection list, bool[] values)
    {
        var labels = list.Flag;
        for (var i = 0; i < labels.Count; i++)
        {
            ValueDict.Add(labels[i].Name, values[labels[i].Index]);
        }
        FlagCollection.ItemsSource = ValueDict;
    }

    public void tapp(object? g)
    {
        Grid gr = (Grid?)g??[];
        var chs = ((CheckBox)gr.Children[0]).IsChecked;
        ((CheckBox)gr.Children[0]).IsChecked = !chs;
        ValueDict[((Label)gr.Children[1]).Text] = !chs;
    }
    public void save()
    {
        EventLabelCollection list = Editor.Labels;
        var labels = list.Flag;
        var sav2 = (SAV2)sav;

        for (int i = 0; i < labels.Count; i++)
        {
            var flagIndex = labels[i].Index;
            var newValue = ValueDict[labels[i].Name];
            sav2.SetEventFlag(flagIndex, newValue);
        }
    }
}

public partial class EventFlags2Tab : TabbedPage
{
	public static EventFlags2? EF2;
    public static EventConstants2? EC2;
    public static EventResearch2? ER2;
	public EventFlags2Tab()
	{
        this.BarBackgroundColor = Color.FromArgb("303030");
        this.BarTextColor = Colors.White;
        EF2 = new();
        EC2 = new();
        ER2 = new((SAV2)sav,sav.Version);
        this.Children.Add(EF2);
        this.Children.Add(EC2);
        this.Children.Add(ER2);
        this.Children.Add(new EventEditor2Save());
        this.Children.Add(new cancelpage());
    }
}
public partial class EventEditor2Save : ContentPage
{
    public EventEditor2Save()
    {
        this.Title = "Save";
    }

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        EventFlags2Tab.EF2?.save();
        EventFlags2Tab.EC2?.save();
        Navigation.PopModalAsync();
    }
}