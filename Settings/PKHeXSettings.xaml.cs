using System.Runtime.CompilerServices;
using Syncfusion.Maui.Inputs;
using Syncfusion.Maui.DataSource.Extensions;
using System.Reflection;
using PKHeX.Core;

namespace PKHeXMAUI;

public partial class PKHeXSettings : ContentPage
{
	public static bool skipevent = true;
	public static List<GenericCollection> props = new();
    public PKHeXSettings()
	{
		InitializeComponent();
        foreach (var p in new PSettings().GetType().GetProperties())
            props.Add(new GenericCollection(p));
        foreach (var p in new EncounterSettings().GetType().GetProperties())
            props.Add(new GenericCollection(p));
        PKHeXSettingsCollection.ItemTemplate = new DataTemplate(() =>
		{
            Grid grid = new Grid() { Padding = 10, Margin =10 };
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
	public static bool IgnoreLegalPopup { get =>  Preferences.Default.Get("IgnoreLegalPopup", false);  set => Preferences.Set("IgnoreLegalPopup", IgnoreLegalPopup); }
	public static bool RememberLastSave { get => Preferences.Default.Get("RememberLastSave", true); set => Preferences.Set("RememberLastSave", RememberLastSave); }
	public static bool DisplayLegalBallsOnly { get => Preferences.Default.Get("DisplayLegalBallsOnly", false); set => Preferences.Set("DisplayLegalBallsOnly", DisplayLegalBallsOnly); }
	
}
public class EncounterSettings
{
	public static bool FilterUnavailableSpecies { get => Preferences.Default.Get("FilterUnavailableSpecies", false); set => Preferences.Set("FilterUnavailableSpecies", FilterUnavailableSpecies); }
	public static bool UsePkEditorAsCriteria { get => Preferences.Default.Get("UsePkEditorAsCriteria", false); set => Preferences.Set("UsePkEditorAsCriteria", UsePkEditorAsCriteria); }
}

public class GenericCollection
{
	public PropertyInfo prop { get; set; }
	public List<object> proparray { get; set; } = new();
	public GenericCollection(PropertyInfo p)
	{
		prop = p;
		if(p.PropertyType == typeof(Boolean))
		{
			proparray = new List<object>() { false, true };
		}
		if(p.PropertyType == typeof(GameVersion))
		{

			var gamelist = MainPage.datasourcefiltered.Games;
			foreach(var g in gamelist)
			{
				proparray.Add((GameVersion)g.Value);
			}

        }
	}
}