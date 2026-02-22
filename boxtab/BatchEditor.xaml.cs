
using PKHeX.Core;
using System.Reflection;
using static PKHeXMAUI.MainPage;

namespace PKHeXMAUI;

public partial class BatchEditor : ContentPage
{
	public BatchEditor()
	{
		InitializeComponent();
		BatchFormat.Items.Add("Any");
        foreach (Type t in EntityBatchEditor.Instance.Types)
			BatchFormat.Items.Add(t.Name.ToLowerInvariant());
		BatchFormat.Items.Add("All");
		var format = BatchFormat.SelectedIndex = 0;
		BatchProperty.ItemSource = EntityBatchEditor.Instance.Properties[format];
        BatchProperty.SelectedIndex = -1;
		BatchProperty.SelectedIndex = 0;
		BatchEditType.ItemsSource = new object[] { "Set", "==", "!=", ">", ">=", "<", "<=" };
		BatchEditType.SelectedIndex = 0;
    }

    private void AddButton_Clicked(object sender, EventArgs e)
    {
        var prefixes = StringInstruction.Prefixes;
        var prefix = prefixes[BatchEditType.SelectedIndex];
        var property = BatchProperty.SelectedItem;
        const char equals = StringInstruction.SplitInstruction;
        BatchText.Text +=$"{prefix}{property}{equals}\n";
    }
    public bool skipchange = false;
    private void PropertyChang(object sender, EventArgs e)
    {
        if (!skipchange)
        {
            if (BatchProperty.SelectedItem is not null)
            {
                EntityBatchEditor.Instance.TryGetPropertyType((string)BatchProperty.SelectedItem,out var res, BatchFormat.SelectedIndex);
                PropertyTypeLab.Text = res;
                if (EntityBatchEditor.Instance.TryGetHasProperty(MainPage.pk, (string)BatchProperty.SelectedItem, out var pi))
                {
                    GetPropertyDisplayText(pi, MainPage.pk, out var display);
                    PropertyValueLab.Text = display;
                }
                else
                {
                    PropertyValueLab.Text = "";
                }
            }
        }
    }
    private static bool GetPropertyDisplayText(PropertyInfo pi, PKM pk, out string display)
    {
        var type = pi.PropertyType;
        if (type.IsGenericType && typeof(Span<>) == type.GetGenericTypeDefinition())
        {
            display = pi.PropertyType.ToString();
            return false;
        }

        var value = pi.GetValue(pk);
        if (value?.ToString() is not { } x)
        {
            display = "null";
            return false;
        }

        display = x;
        return true;
    }

    private void FormatChanged(object sender, EventArgs e)
    {
        skipchange = true;
        var format = BatchFormat.SelectedIndex;
        BatchProperty.ItemSource = EntityBatchEditor.Instance.Properties[format];
        skipchange = false;
        BatchProperty.SelectedIndex = 0;
    }

    private void CloseButtonClicked(object sender, EventArgs e)
    {
        Navigation.PopModalAsync();
    }
    private async void ApplyBatchChanges(object sender, EventArgs e)
    {
        var sets = StringInstructionSet.GetBatchSets(BatchText.Text);
        if (Array.Exists(sets, s => s.Filters.Any(z => string.IsNullOrWhiteSpace(z.PropertyValue))))
        {
            await DisplayAlertAsync("Batch Editor", "Batch Instructions Format Error", "cancel");
            return;
        }
        if (Array.Exists(sets, z => z.Instructions.Count == 0))
        {
            await DisplayAlertAsync("Batch Editor", "No Batch instructions included", "cancel");
        }
        var emptyVal = sets.SelectMany(s => s.Instructions.Where(z => string.IsNullOrWhiteSpace(z.PropertyValue))).ToArray();
        if (emptyVal.Length > 0)
        {
            await DisplayAlertAsync("Batch Editor", "Another Error Display", "Cancel");
            return;
        }
        foreach(var set in sets)
        {
            EntityBatchEditor.ScreenStrings(set.Filters);
            EntityBatchEditor.ScreenStrings(set.Instructions);
        }
        var data = new List<SlotCache>(MainPage.sav.SlotCount);
        SlotInfoLoader.AddBoxData(MainPage.sav, data);
        if (data.All(z=>z.Entity.Species == 0))
        {
            await DisplayAlertAsync("Batch Editor", "No slots to edit", "cancel");
            return;
        }
        process(data);
        foreach (var slot in data)
            slot.Source.WriteTo(MainPage.sav, slot.Entity, EntityImportSettings.None);
        void process(IList<SlotCache> d)
        {
            foreach (var set in sets)
                ProcessSAV(d, set.Filters, set.Instructions);
        }
       await Navigation.PopModalAsync();
        ((BoxTab)AppShell.Current.CurrentPage).fillbox();
    }
    public PKHeX.Core.EntityBatchProcessor editor = new();
    private void ProcessSAV(IList<SlotCache> data, IReadOnlyList<StringInstruction> Filters, IReadOnlyList<StringInstruction> Instructions)
    {
        if (data.Count == 0)
            return;

        var filterMeta = Filters.Where(f => BatchFilters.FilterMeta.Any(z => z.IsMatch(f.PropertyName))).ToArray();
        if (filterMeta.Length != 0)
            Filters = [.. Filters.Except(filterMeta)];

        var max = data[0].Entity.MaxSpeciesID;

        for (int i = 0; i < data.Count; i++)
        {
            var entry = data[i];
            var pk = data[i].Entity;

            var spec = pk.Species;
            if (spec == 0 || spec > max)
                continue;

            if (entry.Source is SlotInfoBox info && sav.GetBoxSlotFlags(info.Box, info.Slot).IsOverwriteProtected())
                editor.AddSkipped();
            else if (!EntityBatchEditor.IsFilterMatchMeta(filterMeta, entry))
                editor.AddSkipped();
            else
                editor.Process(pk, Filters, Instructions);
        }
    }
}