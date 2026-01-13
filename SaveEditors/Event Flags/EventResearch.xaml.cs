using Microsoft.Maui.Controls;
using PKHeX.Core;

namespace PKHeXMAUI;

public partial class EventResearch : ContentPage
{
    private readonly EventWorkspace<IEventFlag37, ushort> Editor;
    public static Dictionary<string, bool> ValueDict = [];
    private readonly List<Tuple<string, List<ComboItem>, ushort, ComboItem>> ConstList = [];
    public EventResearch(IEventFlag37 sav, GameVersion version)
    {
        InitializeComponent();
        ValueDict = [];
        var editor = Editor = new EventWorkspace<IEventFlag37, ushort>(sav, version);
        for (int i = 0; i < editor.Values.Length; i++)
            CB_Stats.ItemSource.Add(new ComboItem(i.ToString(), i));
        CB_Stats.DisplayMemberPath = "Text";
        NUD_Flag.MaxValue = editor.Flags.Length - 1;
        NUD_Flag.MinValue = 0;
        NUD_Flag.Number = 0;
        CHK_CustomFlag.IsChecked = editor.Flags[0];
        AddFlagList(editor.Labels, editor.Flags);
        AddConstList();
    }
    private void AddFlagList(EventLabelCollection list, bool[] values)
    {
        var labels = list.Flag;
        labels = [.. labels.OrderByDescending(z => z.Type)];
        for (var i = 0; i < labels.Count; i++)
        {
            ValueDict.Add(labels[i].Name, values[labels[i].Index]);
        }
    }
    private void AddConstList()
    {
        var labels = Editor.Labels.Work.OrderByDescending(z => z.Type).ToList();
        for (int i = 0; i < labels.Count; i++)
        {
            var value = Editor.Values[labels[i].Index];
            var map = labels[i].PredefinedValues.Select(z => new ComboItem(z.Name, z.Value)).ToList();
            var valueID = map.Find(z => z.Value == value) ?? map[0];
            ConstList.Add((labels[i].Name, map, value, valueID).ToTuple());
        }
    }
    private void NUD_Flag_ValueChanged(object sender, EventArgs e)
    {
        CHK_CustomFlag.IsChecked = ValueDict.Values.ToArray()[(int)NUD_Flag.Number];
    }

    private void CHK_CustomFlag_CheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        ValueDict.Values.ToArray()[(int)NUD_Flag.Number] = CHK_CustomFlag.IsChecked;
    }

    private void CB_Stats_SelectedIndexChanged(object sender, EventArgs e)
    {
        E_Stat.Text = ConstList[(int)CB_Stats.SelectedIndex].Item3.ToString();
    }

    private void E_Stat_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (ushort.TryParse(E_Stat.Text, out var statshort))
        {
            var index = (int)CB_Stats.SelectedIndex;
            ConstList[index] = (ConstList[index].Item1, ConstList[index].Item2, statshort, ConstList[index].Item4).ToTuple();
        }
    }

    private async void B_LoadOld_Clicked(object sender, EventArgs e)
    {
        var result = await FilePicker.PickAsync();
        if (result is not null)
        {
            L_Old.Text = result.FullPath;
        }
    }

    private async void B_LoadNew_Clicked(object sender, EventArgs e)
    {
        var result = await FilePicker.PickAsync();
        if (result is not null)
        {
            L_New.Text = result.FullPath;
        }
    }
    private async void DiffSaves()
    {
        if (L_Old.Text == "" || L_New.Text == "")
        {
            return;
        }
        var diff = new EventBlockDiff<SAV2, byte>(L_Old.Text, L_New.Text);
        if (diff.Message != EventWorkDiffCompatibility.Valid)
        {
            await DisplayAlertAsync("Invalid", diff.Message.GetMessage(), "Cancel");
            return;
        }

        E_IsSet.Text = string.Join(", ", diff.SetFlags.Select(z => $"{z:0000}"));
        E_UnSet.Text = string.Join(", ", diff.ClearedFlags.Select(z => $"{z:0000}"));

        if (diff.WorkDiff.Count == 0)
        {
            await DisplayAlertAsync("Error", "No Event Constant diff found.", "cancel");
            return;
        }

        var promptCopy = await DisplayAlertAsync("Copy", "Copy Event Constant diff to clipboard?", "Yes", "No");
        if (promptCopy)
            Clipboard.SetTextAsync(string.Join(Environment.NewLine, diff.WorkDiff));
    }

    private void L_Old_Changed(object sender, EventArgs e)
    {
        DiffSaves();
    }
}