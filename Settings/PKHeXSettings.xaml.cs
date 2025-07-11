
using System.Reflection;
using PKHeX.Core;
using PKHeX.Core.AutoMod;

using System.Collections.ObjectModel;

using System.Text.Json;
using System.Runtime.Serialization.Json;
using CommunityToolkit.Maui.Storage;
using CommunityToolkit.Maui.Core.Extensions;

namespace PKHeXMAUI;

public partial class PKHeXSettings : ContentPage
{
	public static bool skipevent = true;
	public static List<GenericCollection> props = [];
    public PKHeXSettings()
	{
		InitializeComponent();

        var prop = new propertyGrid(new PSettings());
        Stack_PKHeXSettings.Children.Add(prop);
        Permissions.RequestAsync<Permissions.StorageWrite>();
        var noSelectVersions = new[] { GameVersion.GO, GameVersion.Any };
        SaveVersionPicker.ItemsSource = GameInfo.VersionDataSource.Where(z => !noSelectVersions.Contains((GameVersion)z.Value)).ToList();
        SaveVersionPicker.ItemDisplayBinding = new Binding("Text");
        skipevent = true;
        var newVersion = MainPage.sav.Version;
        if (newVersion == GameVersion.HGSS)
            newVersion = MainPage.sav.Version.GetSingleVersion();
        SaveVersionPicker.SelectedItem = GameInfo.VersionDataSource.FirstOrDefault(z => (GameVersion)z.Value == newVersion);
        skipevent = false;
    }
    private void applynewsave(object sender, EventArgs e)
    {
        if (!skipevent)
        {
            var selected = (ComboItem)SaveVersionPicker.SelectedItem;
            Preferences.Set("SaveFile", selected.Value);
            var blanksav = SaveUtil.GetBlankSAV((GameVersion)selected.Value, "PKHeX");

            App.Current!.Windows[0].Page = new AppShell(blanksav);
        }
    }
    public string LastBox = "";
}
public class PSettings
{
    public static StartPage StartupPage { get => (StartPage)Preferences.Get("StartupPage", 0); }
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
		if(p.PropertyType == typeof(bool))
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
            foreach (var s in Enum.GetValues<Severity>())
                proparray.Add(s);
        }
        if(p.PropertyType == typeof(ObservableCollection<MoveType>))
        {
            foreach (var l in Enum.GetValues<MoveType>())
                proparray.Add(l);
        }
        if (p.PropertyType == typeof(int))
        {
            proparray = ["PK Editor", "Box", "Encounters", "LiveHex", "Save Editors"];
        }
	}
}
public class GenericCollectionSelector : DataTemplateSelector
{
    public static CollectionView Selected = new() { CanReorderItems = true, SelectionMode = SelectionMode.Single};
    public static CollectionView Options = new() { CanReorderItems = true, SelectionMode = SelectionMode.Single };
    public static ObservableCollection<MoveType> SelectedSource = [];
    public static ObservableCollection<MoveType> MoveTypeOptionsSource = Enum.GetValues<MoveType>().ToArray().ToObservableCollection<MoveType>();
    public DataTemplate ComboTemplate = new(() =>
    {
        Grid grid = new() { Padding = 10, Margin = 10 };
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = 175 });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = 175 });

        Label Label_settings = new();
        Label_settings.SetBinding(Label.TextProperty, "prop.Name");
        grid.Add(Label_settings);
        comboBox SettingBool = new() { HorizontalOptions = LayoutOptions.Center };
        SettingBool.SetBinding(comboBox.PlaceholderProperty, "prop.Name");
        SettingBool.SetBinding(comboBox.ItemSourceProperty, "proparray");
        SettingBool.Loaded += GetSettingBool;
        SettingBool.SelectedIndexChanged += ApplySettingBool;
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
    public DataTemplate RandomTypesSettingTemplate = new(() =>
    {
        Grid grid = new() { Padding = 10, Margin = 10 };
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = 175 });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = 175 });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        Selected.ItemTemplate = new DataTemplate(() =>
        {
            Grid grid = new() { Padding = 10, Margin = 10 };
            Label type = new();
            type.SetBinding(Label.TextProperty, ".");
            TapGestureRecognizer Taping = new();
            Taping.Tapped += ((ALMSettings)AppShell.Current!.CurrentPage).RemoveTap;
            grid.GestureRecognizers.Add(Taping);
            grid.Add(type);
            return grid;
        });

        Options.ItemTemplate = new DataTemplate(() =>
        {
            Grid grid = new() { Padding = 10, Margin = 10 };
            Label type = new();
            type.SetBinding(Label.TextProperty,".");
            TapGestureRecognizer NotAddedTap = new();
            NotAddedTap.Tapped += ((ALMSettings)AppShell.Current!.CurrentPage).TapTapTap;
            NotAddedTap.SetBinding(TapGestureRecognizer.CommandParameterProperty, ".");
            grid.GestureRecognizers.Add(NotAddedTap);
            grid.Add(type);
            return grid;
        });
        Options.SetBinding(CollectionView.ItemsSourceProperty, ".");
        Options.BindingContext = MoveTypeOptionsSource;
        Selected.SetBinding(CollectionView.ItemsSourceProperty, ".");
        Selected.BindingContext = SelectedSource;
        var proplabel = new Label();
        proplabel.SetBinding(Label.TextProperty, "prop.Name");
        grid.Add(proplabel);
        grid.Add(new Label() { Text = "Options" }, 1, 0);
        grid.Add(Selected,0,1);
        grid.Add(Options,1,1);
        return grid;
    });
    protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
    {
        if (((GenericCollection)item).prop.PropertyType == typeof(string))
            return StringTemplate;
        if (((GenericCollection)item).prop.PropertyType == typeof(ObservableCollection<MoveType>))
            return RandomTypesSettingTemplate;
        else
            return ComboTemplate;
    }
    public static void ApplySettingBool(object? sender, EventArgs? e)
    {
        if (sender is comboBox box)
        {
                if (box.SelectedItem is Boolean)
                    Preferences.Set(box.Placeholder, (bool)box.SelectedItem);
                else if (box.SelectedItem is GameVersion version)
                    Preferences.Set(box.Placeholder, (int)version);
                else if (box.SelectedItem is Severity severity)
                    Preferences.Set(box.Placeholder, (int)severity);
                else if (box.SelectedItem is string)
                    Preferences.Set(box.Placeholder, box.SelectedIndex);
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
    public static void GetSettingBool(object? sender, EventArgs? e)
    {
        try
        {
            if (sender is comboBox box)
            {
                if (box.Placeholder != LastBox && box.ItemSource is not null)
                {
                    if (((List<object>)box.ItemSource)[0] is Boolean)
                        box.SelectedItem = Preferences.Get(box.Placeholder, false);
                    else if (((List<object>)box.ItemSource)[0] is GameVersion)
                        box.SelectedItem = (GameVersion)Preferences.Get(box.Placeholder, 0);
                    else if (((List<object>)box.ItemSource)[0] is Severity)
                        box.SelectedItem = (Severity)Preferences.Get(box.Placeholder, 0);
                    else if (((List<object>)box.ItemSource)[0] is string)
                        box.SelectedIndex = Preferences.Get(box.Placeholder, 0);

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
                        if(editor.Placeholder == "TrainerFolderPath")
                        {
                            var tap = new TapGestureRecognizer() { NumberOfTapsRequired = 1 };
                            tap.Tapped +=async (s, e) =>
                            {
                                var path = await FolderPicker.PickAsync(CancellationToken.None);
                                if (!path.IsSuccessful) return;
                                if (s is null) return;
                                ((Editor)s).Text = $"{path.Folder.Path}/";
                            };
                            editor.GestureRecognizers.Add(tap);
                        }
                    }
                }
            }
        }
        catch (Exception) { }
    }
}
public enum StartPage
{
    PKEditor,
    Box,
    Encounters,
    LiveHex,
    SaveEditors
}