using PKHeX.Core;
using Syncfusion.Maui.Inputs;

namespace PKHeXMAUI;

public partial class ALMSettings : ContentPage
{
    public static List<GenericCollection> props = new();
    public ALMSettings()
	{
		InitializeComponent();
        foreach (var p in new PluginSettings().GetType().GetProperties())
            props.Add(new GenericCollection(p));
        
        ALMSettingsCollection.ItemTemplate = new DataTemplate(() =>
        {
            Grid grid = new Grid() { Padding = 10, Margin = 10 };
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


        ALMSettingsCollection.ItemsSource = props;
    }
    public string LastBox = "";
    public void GetSettingBool(object sender, EventArgs e)
    {
        var box = (SfComboBox)sender;
        if (box.Placeholder != LastBox)
        {
            if (box.Placeholder != "PrioritizeGameVersion")
                box.SelectedItem = Preferences.Get(box.Placeholder,false);
            else
                box.SelectedItem = (GameVersion)Preferences.Get(box.Placeholder, 0);
            LastBox = box.Placeholder;
        }
    }

    public void ApplySettingBool(object sender, EventArgs e)
    {
        var box = (SfComboBox)sender;
        if(box.SelectedItem is Boolean)
            Preferences.Set(box.Placeholder, (bool)box.SelectedItem);
        else
            Preferences.Set(box.Placeholder, (int)(GameVersion)box.SelectedItem);

    }
}

public class PluginSettings
{
   
    public static bool PrioritizeGame { get => Preferences.Default.Get("PrioritizeGame", false);  }
    public static GameVersion PrioritizeGameVersion { get => (GameVersion)Preferences.Default.Get("PrioritizeGameVersion", 50);  }
    public static bool SetAllLegalRibbons { get => Preferences.Default.Get("SetAllLegalRibbons", false);  }
    public static bool SetBattleVersion { get => Preferences.Default.Get("SetBattleVersion", false);  }
    public static bool SetBallByColor { get => Preferences.Default.Get("SetBallByColor", false);  }
    public static bool EnableMemesForIllegalSets { get => Preferences.Default.Get("EnableMemesForIllegalSets", false);  }
    public static bool LivingDexAllForms { get => Preferences.Get("LivingDexAllForms", false); }
    public static bool LivingDexNativeOnly { get => Preferences.Get("LivingDexNativeOnly", false); }
    public static bool LivingDexSetAlpha { get => Preferences.Get("LivingDexSetAlpha", false); }
    public static bool LivingDexSetShiny { get => Preferences.Get("LivingDexSetShiny", false); }

}