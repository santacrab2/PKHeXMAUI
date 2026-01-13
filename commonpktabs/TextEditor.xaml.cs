using PKHeX.Core;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Globalization;
using static PKHeXMAUI.MainPage;

namespace PKHeXMAUI;

public partial class TextEditor : ContentPage
{
    private readonly IStringConverter Converter;
    public ObservableCollection<trashclass> TrashList = [];
    public string FinalString { get; private set; }
    public byte[] FinalBytes { get; private set; }

    private readonly byte[] Raw;
    private bool editing;
    public TextEditor(string TB_NN, IStringConverter sav, byte generation) : this(TB_NN, [], sav, generation) { }
    public TextEditor(string TB_NN, Span<byte> raw, IStringConverter converter, byte generation)
	{
		InitializeComponent();
        Converter = converter;

        FinalString = TB_NN;

        editing = true;
        if (raw.Length != 0)
        {
            Raw = FinalBytes = raw.ToArray();
            AddTrashEditing(raw.Length);
        }
        else
        {
            Raw = FinalBytes = [];
        }
        var f = new Microsoft.Maui.Graphics.Font("sans-serif", 11);
        AddCharEditing(f, generation);
        StringEntry.Text = TB_NN;
        LanguagePicker.ItemsSource = Enum.GetValues<LanguageID>();
        LanguagePicker.SelectedIndex = MainPage.sav.Language;
    }
    private void AddTrashEditing(int count)
    {
        for(var i = 0;i<count;i++)
            TrashList.Add(new trashclass(i, Raw[i]));
        StringHexCV.ItemTemplate = new DataTemplate(() =>
        {
            Grid grid = new() { Padding = 10 };
            grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Star });
            grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Star });
            Label HexLabel = new() { VerticalOptions=LayoutOptions.Center,HorizontalOptions=LayoutOptions.Center };
            HexLabel.SetBinding(Label.TextProperty, new Binding("Index",stringFormat:"${0}"));
            grid.Add(HexLabel);
            Entry HexValue = new();
            HexValue.SetBinding(Entry.TextProperty, new Binding("Data", BindingMode.TwoWay));
            grid.Add(HexValue,1);
            return grid;
        });
        StringHexCV.ItemsLayout= new GridItemsLayout(4, ItemsLayoutOrientation.Vertical);
        StringHexCV.ItemsSource = TrashList;
        SpeciesCombo.ItemSource = (System.Collections.IList)GameInfo.Strings.Species;
        SpeciesCombo.SelectedItem = GameInfo.Sources.SpeciesDataSource.FirstOrDefault(z => z.Value == MainPage.pk.Species) ?? new("(None)", 0);
        GenerationPicker.ItemsSource = Enumerable.Range(1, 9).ToArray();
        GenerationPicker.SelectedIndex = MainPage.sav.Generation - 1;
    }
    private void AddCharEditing(Microsoft.Maui.Graphics.Font f, byte generation)
    {
        var chars = GetChars(generation);
        if (chars.Count == 0)
            return;
        SpecialStringCV.ItemTemplate = new DataTemplate(() =>
        {
            Grid grid = new() { Padding = 10 };
            Label special = new() { VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.Center,FontFamily=f.ToString() };
            special.SetBinding(Label.TextProperty, new Binding(".",converter:new CharConverter()));
            TapGestureRecognizer tap = new();
            tap.Tapped += (_, _) => { if (StringEntry.Text.Length < StringEntry.MaxLength) StringEntry.Text+=special.Text; };
            special.GestureRecognizers.Add(tap);
            grid.Add(special);
            return grid;
        });
        SpecialStringCV.ItemsLayout = new GridItemsLayout(8, ItemsLayoutOrientation.Vertical);
        SpecialStringCV.ItemsSource = chars;
    }
    private void B_Save_Click(object sender, EventArgs e)
    {
        FinalString = StringEntry.Text;
        foreach (var trash in TrashList)
            Raw[trash.Index] = trash.Data;

        FinalBytes = Raw;

        Navigation.PopModalAsync();
        EditingTrash = false;
    }
    private void B_ApplyTrash_Click(object sender, EventArgs e)
    {
        string text = GetTrashString();
        ReadOnlySpan<byte> data = SetString(text);
        ReadOnlySpan<byte> current = SetString(StringEntry.Text);
        if (data.Length <= current.Length)
        {
            DisplayAlertAsync("Trash byte layer is hidden by current text.",
                $"Current Bytes: {current.Length}" + Environment.NewLine + $"Layer Bytes: {data.Length}","cancel");
            return;
        }
        if (data.Length > TrashList.Count)
        {
            DisplayAlertAsync("Trash byte layer is too long to apply.","Too Long.", "cancel");
            return;
        }
        for (int i = current.Length; i < data.Length; i++)
        {
            TrashList[i].Data = data[i];
        }
    }
    public void SetOrientation()
    {
        #if ANDROID
        Microsoft.Maui.ApplicationModel.Platform.CurrentActivity!.RequestedOrientation =    Android.Content.PM.ScreenOrientation.Locked;
        Microsoft.Maui.ApplicationModel.Platform.CurrentActivity.RequestedOrientation = Android.Content.PM.ScreenOrientation.Landscape;
        #endif
    }
    public void RestSetOrientation()
    {
        #if ANDROID
        Microsoft.Maui.ApplicationModel.Platform.CurrentActivity!.RequestedOrientation =    Android.Content.PM.ScreenOrientation.User;
        #endif
    }

    protected override void OnAppearing()
    {
        SetOrientation();
    }

    protected override void OnDisappearing()
    {
        RestSetOrientation();
    }

    private void CloseTextEditor(object sender, EventArgs e)
    {
        Navigation.PopModalAsync();
    }
    private string GetTrashString()
    {
        var species = (ushort?)((ComboItem?)SpeciesCombo.SelectedItem)?.Value??0;
        var language = (LanguageID)LanguagePicker.SelectedItem;
        var gen = (byte)(int)GenerationPicker.SelectedItem;
        string text = SpeciesName.GetSpeciesNameGeneration(species, (int)language, gen);

        if (string.IsNullOrEmpty(text)) // no result
            text = ((ComboItem?)SpeciesCombo.SelectedItem)?.Text??"";
        return text;
    }
    private byte[] SetString(ReadOnlySpan<char> text)
    {
        Span<byte> temp = stackalloc byte[Raw.Length];
        var written = Converter.SetString(temp, text, text.Length, StringConverterOption.None);
        return temp[..written].ToArray();
    }
    private void B_ClearTrash_Click(object sender, EventArgs e)
    {
        byte[] current = SetString(StringEntry.Text);
        for (int i = current.Length; i < TrashList.Count; i++)
        {
            TrashList[i].Data = 0;
        }
    }
    private string GetString() => Converter.GetString(Raw);
    private void UpdateString(Entry tb)
    {
        if (editing)
            return;
        editing = true;
        // build bytes
        ReadOnlySpan<byte> data = SetString(tb.Text);
        if (data.Length > Raw.Length)
            data = data[..Raw.Length];
        data.CopyTo(Raw);
        for (int i = 0; i < Raw.Length; i++)
        {
            TrashList[i].Data = Raw[i];
        }
        editing = false;
    }
    private static ObservableCollection<char> GetChars(byte generation) => generation switch
    {
        5 => SpecialCharsGen5,
        6 => SpecialCharsGen67,
        7 => SpecialCharsGen67,
        >= 8 => SpecialCharsGen8,
        _ => [], // Undocumented
    };

    // Unicode codepoints for special characters, incorrectly starting at 0x2460 instead of 0xE0xx.
    private static ObservableCollection<char> SpecialCharsGen5 =>
    [
   (char)     0x2460, // Full Neutral (Happy in Gen7)
  (char)      0x2461, // Full Happy (Angry in Gen7)
  (char)      0x2462, // Full Sad
  (char)      0x2463, // Full Angry (Neutral in Gen7)
  (char)      0x2464, // Full Right-up arrow
  (char)      0x2465, // Full Right-down arrow
  (char)      0x2466, // Full Zz
  (char)      0x2467, // ×
  (char)      0x2468, // ÷
        // Skip 69-6B, can't be entered.
  (char)      0x246C, // …
  (char)      0x246D, // ♂
  (char)      0x246E, // ♀
  (char)      0x246F, // ♠
  (char)      0x2470, // ♣
  (char)      0x2471, // ♥
  (char)      0x2472, // ♦
  (char)      0x2473, // ★
  (char)      0x2474, // ◎
  (char)      0x2475, // ○
  (char)      0x2476, // □
  (char)      0x2477, // △
  (char)      0x2478, // ◇
  (char)      0x2479, // ♪
  (char)      0x247A, // ☀
  (char)      0x247B, // ☁
  (char)      0x247C, // ☂
  (char)      0x247D, // ☃
  (char)      0x247E, // Half Neutral
  (char)      0x247F, // Half Happy
        (char)0x2480, // Half Sad
  (char)      0x2481, // Half Angry
  (char)      0x2482, // Half Right-up arrow
  (char)      0x2483, // Half Right-down arrow 
  (char)      0x2484, // Half Zz
    ];

    private static ObservableCollection<char> SpecialCharsGen67 =>
    [
        (char)0xE081, // Full Neutral (Happy in Gen7)
        (char)0xE082, // Full Happy (Angry in Gen7)
        (char)0xE083, // Full Sad
        (char)0xE084, // Full Angry (Neutral in Gen7)
       (char)0xE085, // Full Right-up arrow
        (char)0xE086, // Full Right-down arrow
        (char)0xE087, // Full Zz
        (char)0xE088, // ×
        (char)0xE089, // ÷
        // Skip 8A-8C, can't be entered.
        (char)0xE08D, // …
       (char) 0xE08E, // ♂
        (char)0xE08F, // ♀
        (char)0xE090, // ♠
       (char) 0xE091, // ♣
      (char)  0xE092, // ♥
    (char)   0xE093, // ♦
   (char)     0xE094, // ★
  (char)      0xE095, // ◎
      (char)  0xE096, // ○
    (char)    0xE097, // □
      (char)  0xE098, // △
      (char)  0xE099, // ◇
       (char) 0xE09A, // ♪
       (char) 0xE09B, // ☀
      (char)  0xE09C, // ☁
     (char)   0xE09D, // ☂
    (char)    0xE09E, // ☃
   (char)     0xE09F, // Half Neutral
     (char)   0xE0A0, // Half Happy
  (char)      0xE0A1, // Half Sad
       (char) 0xE0A2, // Half Angry
     (char)   0xE0A3, // Half Right-up arrow
  (char)      0xE0A4, // Half Right-down arrow 
       (char) 0xE0A5, // Half Zz
    ];

    private static ObservableCollection<char> SpecialCharsGen8 =>
    [
        '…', // '\uE08D' -> '\u2026'
        '♂', // '\uE08E' -> '\u2642'
        '♀', // '\uE08F' -> '\u2640'
        '♠', // '\uE090' -> '\u2660'
        '♣', // '\uE091' -> '\u2663'
        '♥', // '\uE092' -> '\u2665'
        '♦', // '\uE093' -> '\u2666'
        '★', // '\uE094' -> '\u2605'
        '◎', // '\uE095' -> '\u25CE'
        '○', // '\uE096' -> '\u25CB'
        '□', // '\uE097' -> '\u25A1'
        '△', // '\uE098' -> '\u25B3'
        '◇', // '\uE099' -> '\u25C7'
        '♪', // '\uE09A' -> '\u266A'
        '☀', // '\uE09B' -> '\u2600'
        '☁', // '\uE09C' -> '\u2601'
        '☂', // '\uE09D' -> '\u2602'
        '☃', // '\uE09E' -> '\u2603'
    ];
}
public class trashclass(int index, byte data)
{
    public int Index { get; set; } = index;
    public byte Data { get; set; } = data;
}
public class CharConverter : IValueConverter
{
    object? IValueConverter.Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is null)
            return value;
        try
        {
            var newchar = ((char)value).ToString();
            return newchar;
        }
        catch (Exception)
        {
            return value;
        }
    }

    object? IValueConverter.ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return ushort.Parse(value?.ToString()??"0");
    }
}