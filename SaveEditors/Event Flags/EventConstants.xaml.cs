using PKHeX.Core;
namespace PKHeXMAUI;

public partial class EventConstants : ContentPage
{

    private List<Tuple<string, List<ComboItem>, ushort, ComboItem>> ConstList = [];
    public EventConstants(IEventFlag37 sav, GameVersion version)
    {
        InitializeComponent();
        var editor = EventFlags.Editor = new EventWorkspace<IEventFlag37, ushort>(sav, version);
        ConstantCollection.ItemTemplate = new DataTemplate(() =>
        {
            var grid = new Grid() { Padding = 10 };
            grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(3, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(2, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Star });
            Label label = new();
            label.SetBinding(Label.TextProperty, new Binding("Item1"));
            Picker combo = new();
            combo.SetBinding(Picker.ItemsSourceProperty, new Binding("Item2"));
            combo.ItemDisplayBinding = new Binding("Text");
            combo.SetBinding(Picker.SelectedItemProperty, new Binding("Item4"));
            combo.SelectedIndexChanged += (sender, e) =>
            {
                var index = ConstList.IndexOf(ConstList.Find(z => z.Item1 == ((Label)grid.Children[0]).Text) ?? new Tuple<string, List<ComboItem>, ushort, ComboItem>("", [], 0, new ComboItem("", 0)));
                ConstList[index] = (ConstList[index].Item1, ConstList[index].Item2, (ushort?)((ComboItem?)((Picker?)sender)?.SelectedItem)?.Value ?? 0, ConstList[index].Item4).ToTuple();
            };
            grid.Add(label);
            grid.Add(combo, 1);
            return grid;
        });
        var labels = editor.Labels.Work.OrderByDescending(z => z.Type).ToList();
        for (int i = 0; i < labels.Count; i++)
        {
            var value = editor.Values[labels[i].Index];
            var map = labels[i].PredefinedValues.Select(z => new ComboItem(z.Name, z.Value)).ToList();
            var valueID = map.Find(z => z.Value == value) ?? map[0];
            ConstList.Add((labels[i].Name, map, value, valueID).ToTuple());
        }
        ConstantCollection.ItemsSource = ConstList;
    }
    public void save()
    {
        EventLabelCollection list = EventFlags.Editor.Labels;
        ushort[] values = EventFlags.Editor.Values;
        var labels = list.Work;
        for (int i = 0; i < labels.Count; i++)
        {
            values[labels[i].Index] = ConstList.Find(z => z.Item1 == labels[i].Name)?.Item3 ?? 0;
        }

        EventFlags.Editor.Save();
    }
}