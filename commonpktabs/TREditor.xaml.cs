
using PKHeX.Core;

using static PKHeXMAUI.MainPage;
namespace PKHeXMAUI;

public partial class TREditor : ContentPage
{
	public TREditor()
	{
		InitializeComponent();
        TREditorItem[] lines = [];
        ReadOnlySpan<ushort> indexes = new();
		if(pk is ITechRecord tr)
		{
            var names = GameInfo.Strings.Move;
            indexes = tr.Permit.RecordPermitIndexes;
            lines = new TREditorItem[indexes.Length];
            for (int i = 0; i < lines.Length; i++)
                lines[i] = new TREditorItem($"{i:00} - {names[indexes[i]]}");
        }
		trcollection.ItemTemplate = new DataTemplate(() =>
		{
            Grid grid = new() { Padding = 10 };
            grid.RowDefinitions.Add(new RowDefinition { Height = 24 });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = 350 });
            Label technicalrecordname = new() { HorizontalTextAlignment = TextAlignment.Center };
            technicalrecordname.SetBinding(Label.TextProperty, new Binding("name"));
            grid.Add(technicalrecordname);
			return grid;
        });
        trcollection.ItemsSource = lines;
        var selectedlines = new List<object>();
        for (int i = 0; i < lines.Length; i++)
        {
            if(pk is ITechRecord tr2)
            {
                if (tr2.GetMoveRecordFlag(i))
                    selectedlines.Add(lines[i]);
            }
        }
        trcollection.UpdateSelectedItems(selectedlines);
    }

    private void applytrsandclose(object sender, EventArgs e)
    {
        if(pk is ITechRecord tr)
        {
            var names = GameInfo.Strings.Move;
            var indexes = tr.Permit.RecordPermitIndexes;
            for (int i = 0; i < indexes.Length; i++)
            {
                tr.SetMoveRecordFlag(i, false);
            }
            foreach (var selected in trcollection.SelectedItems)
            {
                var selectedtr = (TREditorItem)selected;
                var selectedtrstring = selectedtr.name.Remove(0, 5);
                for(int i =0;i<indexes.Length;i++)
                {
                    if(selectedtrstring == names[indexes[i]])
                        tr.SetMoveRecordFlag(i, true);
                }
            }
        }
        Navigation.PopModalAsync();
    }

    private void closer(object sender, EventArgs e)
    {
        Navigation.PopModalAsync();
    }

    private void applyalltrsandclose(object sender, EventArgs e)
    {
        if(pk is ITechRecord tr)
        {
            tr.SetRecordFlagsAll();
        }
        Navigation.PopModalAsync();
    }

    private void removealltrsandclose(object sender, EventArgs e)
    {
        if (pk is ITechRecord tr)
        {
            var indexes = tr.Permit.RecordPermitIndexes;
            for (int i = 0; i < indexes.Length; i++)
            {
                tr.SetMoveRecordFlag(i, false);
            }
        }
        Navigation.PopModalAsync();
    }
}

public class TREditorItem(string rec)
{
    public string name { get; set; } = rec;
}