
using static PKHeXMAUI.MainPage;
using PKHeX.Core;
namespace PKHeXMAUI;

public partial class EventFlags : ContentPage
{
    private readonly EventWorkspace<IEventFlag37, ushort> Editor;
    public static Dictionary<string, bool> ValueDict = [];
    public EventFlags(IEventFlag37 sav, GameVersion version)
    {
        InitializeComponent();
        ValueDict = [];
        var editor = Editor = new EventWorkspace<IEventFlag37, ushort>(sav, version);
        FlagCollection.ItemTemplate = new DataTemplate(() =>
        {
            var grid = new Grid() { Padding = 10 };
            grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Star });
            grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(3, GridUnitType.Star) });
            var check = new CheckBox() { VerticalOptions = LayoutOptions.Center };
            check.SetBinding(CheckBox.IsCheckedProperty, new Binding("Value"));
            grid.Add(check);
            var label = new Label();
            label.SetBinding(Label.TextProperty, new Binding("Key"));
            grid.Add(label, 1);
            var tap = new TapGestureRecognizer();
            tap.Tapped += tapp;
            grid.GestureRecognizers.Add(tap);
            var tap2 = new TapGestureRecognizer
            {
                CommandParameter = grid,
                Command = new Command(() => tapp(grid, (TappedEventArgs)EventArgs.Empty))
            };
            check.GestureRecognizers.Add(tap2);
            return grid;
        });
        AddFlagList(editor.Labels, editor.Flags);
    }
    private void AddFlagList(EventLabelCollection list, bool[] values)
    {
        var labels = list.Flag;
        labels = [.. labels.OrderByDescending(z => z.Type)];
        for (var i = 0; i < labels.Count; i++)
        {
            ValueDict.Add(labels[i].Name, values[labels[i].Index]);
        }
        FlagCollection.ItemsSource = ValueDict;
    }
#nullable enable
    public void tapp(object? g, TappedEventArgs? e)
    {
        Grid gr = (Grid?)g ?? [];
        var chs = ((CheckBox)gr.Children[0]).IsChecked;
        ((CheckBox)gr.Children[0]).IsChecked = !chs;
        ValueDict[((Label)gr.Children[1]).Text] = !chs;
    }
    public void save()
    {
        EventLabelCollection list = Editor.Labels;
        bool[] values = Editor.Flags;
        var labels = list.Flag;
        for (int i = 0; i < labels.Count; i++)
        {
            values[labels[i].Index] = ValueDict[labels[i].Name];
        }

        Editor.Save();
    }
}

public partial class EventFlagsTab : TabbedPage
{
    public static EventFlags? EF2;
    public static EventConstants? EC2;
    public EventFlagsTab(IEventFlag37 g37, GameVersion version)
    {
        this.BarBackgroundColor = Color.FromArgb("303030");
        this.BarTextColor = Colors.White;
        EF2 = new(g37, version);
        EC2 = new(g37, version);
        this.Children.Add(EF2);
        this.Children.Add(EC2);
        this.Children.Add(new EventEditorSave());
        this.Children.Add(new cancelpage());
    }
}
public partial class EventEditorSave : ContentPage
{
    public EventEditorSave()
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