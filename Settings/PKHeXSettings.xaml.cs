using System.Runtime.CompilerServices;
using Syncfusion.Maui.Inputs;
using Syncfusion.Maui.DataSource.Extensions;
using System.Reflection;
using PKHeX.Core;
using PKHeX.Core.AutoMod;

namespace PKHeXMAUI;

public partial class PKHeXSettings : ContentPage
{
	public static bool skipevent = true;
	public static List<GenericCollection> props = [];
    public PKHeXSettings()
	{
		InitializeComponent();
        foreach (var p in new PSettings().GetType().GetProperties())
            props.Add(new GenericCollection(p));
        foreach (var p in new EncounterSettings().GetType().GetProperties())
            props.Add(new GenericCollection(p));
        PKHeXSettingsCollection.ItemTemplate = new DataTemplate(() =>
		{
            Grid grid = new() { Padding = 10, Margin =10 };
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = 175 });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = 175 });

			Label Label_settings = new();
            Label_settings.SetBinding(Label.TextProperty, "prop.Name");
			grid.Add(Label_settings);
			SfComboBox SettingBool = new() { HorizontalOptions = LayoutOptions.Center };
			SettingBool.SetBinding(SfComboBox.PlaceholderProperty,"prop.Name");
			SettingBool.SetBinding(SfComboBox.ItemsSourceProperty,"proparray");
			SettingBool.PropertyChanged += GetSettingBool;
            SettingBool.SelectionChanged += ApplySettingBool;
			grid.Add(SettingBool, 1);

			return grid;
        });

		PKHeXSettingsCollection.ItemsSource = props;
    }
	public string LastBox = "";
    public void GetSettingBool(object sender, EventArgs e)
	{
        var box = (SfComboBox)sender;
        if (box.Placeholder != LastBox)
		{
			box.SelectedItem = Preferences.Default.Get(box.Placeholder, false);
			LastBox = box.Placeholder;
		}
	}

    public void ApplySettingBool(object sender, EventArgs e)
	{
        var box = (SfComboBox)sender;
		Preferences.Set(box.Placeholder, (bool)box.SelectedItem);
	}
}
public class PSettings
{
	public static bool IgnoreLegalPopup { get => Preferences.Get("IgnoreLegalPopup",false); }
	public static bool RememberLastSave { get => Preferences.Default.Get("RememberLastSave", true);  }
	public static bool DisplayLegalBallsOnly { get => Preferences.Default.Get("DisplayLegalBallsOnly", false);  }
	public static bool AllowIncompatibleConversion { get => Preferences.Default.Get("AllowIncompatibleConversion", false); }
    public static bool SetUpdatePKM { get => Preferences.Get("SetUpdatePKM", true); }
}
public class EncounterSettings
{
	public static bool FilterUnavailableSpecies { get => Preferences.Default.Get("FilterUnavailableSpecies", false); }
	public static bool UsePkEditorAsCriteria { get => Preferences.Default.Get("UsePkEditorAsCriteria", false); }
}

public class GenericCollection
{
	public PropertyInfo prop { get; set; }
	public List<object> proparray { get; set; } = [];
	public GenericCollection(PropertyInfo p)
	{
		prop = p;
		if(p.PropertyType == typeof(Boolean))
		{
			proparray = [false, true];
		}
		if(p.PropertyType == typeof(GameVersion))
		{
			var gamelist = MainPage.datasourcefiltered.Games;
			foreach(var g in gamelist)
			{
				proparray.Add((GameVersion)g.Value);
			}
        }
        if(p.PropertyType == typeof(Severity))
        {
            foreach (var s in Enum.GetValues(typeof(Severity)))
                proparray.Add((Severity)s);
        }
	}
}
public class GenericCollectionSelector : DataTemplateSelector
{
    public DataTemplate ComboTemplate = new(() =>
    {
        Grid grid = new() { Padding = 10, Margin = 10 };
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = 175 });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = 175 });

        Label Label_settings = new();
        Label_settings.SetBinding(Label.TextProperty, "prop.Name");
        grid.Add(Label_settings);
        SfComboBox SettingBool = new() { HorizontalOptions = LayoutOptions.Center };
        SettingBool.SetBinding(SfComboBox.PlaceholderProperty, "prop.Name");
        SettingBool.SetBinding(SfComboBox.ItemsSourceProperty, "proparray");
        SettingBool.PropertyChanged += GetSettingBool;
        SettingBool.SelectionChanged += ApplySettingBool;
        grid.Add(SettingBool, 1);

        return grid;
    });

    public DataTemplate StringTemplate = new(() =>
    {
        Grid grid = new() { Padding = 10, Margin = 10 };
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = 175 });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = 175 });

        Label Label_settings = new();
        Label_settings.SetBinding(Label.TextProperty, "prop.Name");
        grid.Add(Label_settings);
        Editor SettingString = new() { HorizontalOptions = LayoutOptions.Center };
        SettingString.SetBinding(Editor.PlaceholderProperty, "prop.Name");
        SettingString.PropertyChanged += GetSettingBool;
        SettingString.TextChanged += ApplySettingBool;
        grid.Add(SettingString, 1);
        return grid;
    });

    protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
    {
        return ((GenericCollection)item).prop.PropertyType == typeof(string) ? StringTemplate : ComboTemplate;
    }
    public static void ApplySettingBool(object sender, EventArgs e)
    {
        if (sender is SfComboBox box)
        {
                if (box.SelectedItem is Boolean)
                    Preferences.Set(box.Placeholder, (bool)box.SelectedItem);
                else if (box.SelectedItem is GameVersion)
                    Preferences.Set(box.Placeholder, (int)(GameVersion)box.SelectedItem);
                else if (box.SelectedItem is Severity)
                    Preferences.Set(box.Placeholder, (int)(Severity)box.SelectedItem);
        }
        if (sender is Editor editor)
        {
            Preferences.Set(editor.Placeholder, editor.Text);
            switch (editor.Placeholder)
            {
                case "DefaultOT": TrainerSettings.DefaultOT = editor.Text.Trim(); break;
                case "DefaultTID":
                    var isTIDdigits = ushort.TryParse(editor.Text, out var TID);
                    if (isTIDdigits)
                        TrainerSettings.DefaultTID16 = TID; break;
                case "DefaultSID":
                    var isSIDdigits = ushort.TryParse(editor.Text, out var SID);
                    if (isSIDdigits)
                        TrainerSettings.DefaultSID16 = SID; break;
            }
            TrainerSettings.Clear();
            TrainerSettings.Register(TrainerSettings.DefaultFallback((GameVersion)MainPage.sav.Version, (LanguageID)MainPage.sav.Language));
        }
        MainPage.SetSettings();
    }
    public static string LastBox = "";
    public static void GetSettingBool(object sender, EventArgs e)
    {
        if (sender is SfComboBox box)
        {
            if (box.Placeholder != LastBox && box.ItemsSource is not null)
            {
                if (((List<object>)box.ItemsSource)[0] is Boolean)
                    box.SelectedItem = Preferences.Get(box.Placeholder, false);
                else if (((List<object>)box.ItemsSource)[0] is GameVersion)
                    box.SelectedItem = (GameVersion)Preferences.Get(box.Placeholder, 0);
                else if(((List<object>)box.ItemsSource)[0] is Severity)
                    box.SelectedItem = (Severity)Preferences.Get(box.Placeholder,0);

                LastBox = box.Placeholder;
            }
        }
        if (sender is Editor editor)
        {
            if (editor.Placeholder != null)
            {
                if (editor.Placeholder != LastBox)
                {
                    editor.Text = Preferences.Get(editor.Placeholder, "12345");
                    LastBox = editor.Placeholder;
                    var description = editor.Placeholder switch
                    {
                        "DefaultOT" => "The OT Name that will be set when you press Legalize",
                        "DefaultTID" => "5 digit Trainer ID (Not the 7 digit display)",
                        "DefaultSID" => "5 digit Trainer Secret ID (not the 4 digit display)",
                        _ => ""
                    };
                    ToolTipProperties.SetText(editor, description);
                }
            }
        }
    }
}