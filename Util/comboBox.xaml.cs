#if ANDROID
using Android.Widget;
#endif
using Microsoft.Maui.Platform;
using PKHeX.Core;
using System.Collections;
using System.Globalization;
using static System.Net.Mime.MediaTypeNames;

namespace PKHeXMAUI;
/// <summary>
/// Custom Combo Box control that allows setting a generic ItemSource.
/// </summary>
public partial class comboBox : Microsoft.Maui.Controls.ContentView
{
    ///<summary>Bindable property for <see cref="DisplayMemberPath"/></summary>
    public static BindableProperty DisplayMemberPathProperty = BindableProperty.Create(nameof(DisplayMemberPath), typeof(string), typeof(comboBox),".",propertyChanged:OnItemsCollectionChanged);
    /// <summary>Bindable property for <see cref="ItemSource"/> </summary>
    public static BindableProperty ItemSourceProperty = BindableProperty.Create(nameof(ItemSource), typeof(IEnumerable), typeof(comboBox), propertyChanged:OnItemsCollectionChanged);
    /// <summary>Bindable property for <see cref="Title"/> </summary>
    public static BindableProperty TitleProperty = BindableProperty.Create(nameof(Title), typeof(string), typeof(comboBox));
    /// <summary>Bindable property for <see cref="Placeholder"/></summary>
    public static BindableProperty PlaceholderProperty = BindableProperty.Create(nameof(Placeholder), typeof(string), typeof(comboBox));
    ///<summary>Bindable property for <see cref="SelectedItem"/></summary>
    public static BindableProperty SelectedItemProperty = BindableProperty.Create(nameof(SelectedItem), typeof(object), typeof(comboBox), null, BindingMode.TwoWay, propertyChanged: SetSelectedItem);
    /// <summary>Bindable property for <see cref="SelectedIndex"/> </summary>
    public static BindableProperty SelectedIndexProperty = BindableProperty.Create(nameof(SelectedIndex), typeof(int), typeof(comboBox),propertyChanged: SetSelectedIndex);
    /// <summary>Bindable property for <see cref="SelectedIndex"/> </summary>
    public event EventHandler? SelectedIndexChanged;
    public event EventHandler? TextChanged;
   /// <summary>Gets or sets the Binding Path for displaying the object. Default is ".". This is a bindable Property. </summary>
    public string DisplayMemberPath { get => (string)GetValue(DisplayMemberPathProperty); set { SetValue(DisplayMemberPathProperty, value); } }
    /// <summary>Gets or sets the Title of the comboBox. This is a bindable property. Default is null.</summary>
	public string Title { get => (string)GetValue(TitleProperty);set=> SetValue(TitleProperty, value); }
    /// <summary>
    /// the string representation of the items in <see cref="ItemSource"/>
    /// </summary>
    public IList<string> Items = [];
    /// <summary>
    /// Gets or sets the ItemSource Property of the comboBox. Default is null. This is a bindable property.
    /// </summary>
    public IEnumerable ItemSource { get =>(IEnumerable)GetValue(ItemSourceProperty); set =>SetValue(ItemSourceProperty, value); }
    /// <summary>
    /// Gets or sets the Placeholder property of the comboBox. Default is null. This is a bindable property.
    /// </summary>
    public string Placeholder { get => (string)GetValue(PlaceholderProperty); set => SetValue(PlaceholderProperty, value); }
    /// <summary>
    /// Gets or sets the index of the Selected Item in the comboBox. Default is null. This is a bindable property.
    /// </summary>
	public int SelectedIndex { get=>(int)GetValue(SelectedIndexProperty); set =>  SetValue(SelectedIndexProperty, value); }
    /// <summary>
    /// Gets or sets the selected item in the comboBox. Default is null. This is a bindable property.
    /// </summary>
	public object? SelectedItem { get => GetValue(SelectedItemProperty); set { picker.SelectedItem = value; SetValue(SelectedItemProperty, value); } }
    public Microsoft.Maui.Controls.ListView picker;
    public comboBox()
    {
        InitializeComponent();
        picker = new()
        {
            BackgroundColor = Colors.White
        };
        picker.ItemSelected += IndexChanged;
        picker.HeightRequest = 50;
        picker.SelectionMode = ListViewSelectionMode.Single;
        picker.SetBinding(Microsoft.Maui.Controls.ListView.ItemsSourceProperty, new Binding("ItemSource", source: ThisView));
        picker.ItemTemplate = new DataTemplate(() =>
        {
            ViewCell cell = new();
            Label label = new();
            label.SetBinding(Label.TextProperty, new Binding(DisplayMemberPath));
            label.TextColor = Colors.Black;
            picker.ItemTemplate = new DataTemplate(() =>
            {
                ViewCell cell = new();
                Label label = new();
                label.SetBinding(Label.TextProperty, new Binding(DisplayMemberPath));
                label.SetBinding(Label.BackgroundColorProperty, new Binding("Valid", converter: new BoolToColorConverter()));
                label.TextColor = Colors.Black;
                cell.View = label;
                return cell;
            });
            cell.View = label;
            return cell;
        });
#if ANDROID
        entry.Unfocused += (s, e) => popupWindow.Dismiss();
#endif
        picker.ZIndex = 1;
    }
    static void OnItemsCollectionChanged(BindableObject bindable, object oldValue, object newValue)
    {
        ((comboBox)bindable).OnItemsCollectionChanged(bindable,EventArgs.Empty);
    }
    public void OnItemsCollectionChanged(object sender, EventArgs e)
    {
        if (ItemSource is null) return;
        Items.Clear();
        foreach (var item in ItemSource)
        {
            Items.Add(GetDisplayMember(item));
        }
    }
    private string GetDisplayMember(object item)
    {
        if (DisplayMemberPath == ".")
        {
            if (item is not null)
            {
                return item.ToString()??"";
            }

            return string.Empty;
        }
        var result = (item.GetType().GetProperty(DisplayMemberPath)?.GetValue(item))?.ToString();
        return result ?? "";
    }
    /// <summary>
    /// Filter's the items in the ListView based on the text in the entry.
    /// </summary>
    /// <param name="sender">the object calling the event</param>
    /// <param name="e">Event args</param>
    private void entry_textChanged(object sender, TextChangedEventArgs e)
    {
        if (picker.ItemsSource is null) return;
#if ANDROID
        if (popupWindow?.IsShowing == false) ShowDropdown();
#endif
        if (entry.Text.Length == 8)
        {
            List<ComboItem> filteredlist = [];
            var hex = (int)Util.GetHexValue(entry.Text);
            if (hex != 0)
            {
                // Input is hexadecimal number, select the item
                filteredlist = [BlockEditor8.SortedBlockKeys.ToList().Find(z => z.Value == hex) ?? new ComboItem("Error", 0)];
                picker.ItemsSource = filteredlist;
                return;
            }
        }
        IList tempsource = Items.Where(z=>z.StartsWith(entry.Text,StringComparison.OrdinalIgnoreCase)).ToList();
        picker.ItemsSource = ItemSource.Cast<object>().Where(z => tempsource.Contains(z.GetType().GetProperty(DisplayMemberPath) is null?z.ToString():z.GetType().GetProperty(DisplayMemberPath)?.GetValue(z)?.ToString())).ToList();
        TextChanged?.Invoke(this, e);
    }
    private string SelectedItemText = "";
    /// <summary>
    /// Changes the displayed text in the entry control, calls the bindable SelectedIndexChanged Event
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void IndexChanged(object? sender, EventArgs? e)
    {
        if (picker.SelectedItem.GetType().GetProperty(DisplayMemberPath) is null)
            SelectedItemText = picker.SelectedItem.ToString()??"";
        else
            SelectedItemText = picker.SelectedItem.GetType().GetProperty(DisplayMemberPath)?.GetValue(picker.SelectedItem)?.ToString()??"";

        entry.Text = SelectedItemText;
        SelectedItem = picker.SelectedItem;
        SelectedIndex = ItemSource.Cast<object>().ToList().IndexOf(SelectedItem);
        SelectedIndexChanged?.Invoke(this, e!);
#if ANDROID
        popupWindow?.Dismiss();
#endif
    }
    public void ForceSelection(object? sender, EventArgs? e) => picker.SelectedItem = SelectedItem;
    static void SetSelectedItem(BindableObject bindable, object oldValue, object newValue)
    {
        ((comboBox)bindable).SetSelectedItem(newValue);
    }
    public void SetSelectedItem(object value) => picker.SelectedItem = value;
    static void SetSelectedIndex(BindableObject bindable, object oldValue, object newValue)
    {
        if ((int)newValue < 0) return;
        var lv = (comboBox)bindable;
        lv.SelectedItem = lv.ItemSource.Cast<object>().ToList()[(int)newValue];
    }
    /// <summary>
    /// Hides the dropdown List for the comboBox
    /// </summary>
    public void HideList()
    {
        #if ANDROID
        popupWindow.Dismiss();
      #endif
    }
    /// <summary>
    /// Shows the dropdown List for the comboBox
    /// </summary>
    public void ShowList()
    {
        ShowDropdown();
    }
    private void ShowList(object sender, FocusEventArgs e)
    {
        ShowDropdown();
    }
    private void ClearText(object sender, EventArgs e)
    {
        entry.Text = string.Empty;
        if (!entry.IsFocused)
            entry.Focus();
    }
    private void AutoCompleteText(object sender, EventArgs e)
    {
        IList tempsource = Items.Where(z => z.Contains(entry.Text, StringComparison.CurrentCultureIgnoreCase)).ToList();
        var item = ItemSource.Cast<object>().FirstOrDefault(z => tempsource.Contains(z.GetType().GetProperty(DisplayMemberPath) is null ? z.ToString() : z.GetType().GetProperty(DisplayMemberPath)?.GetValue(z)?.ToString() ?? ""))??"";
        if (item.GetType().GetProperty(DisplayMemberPath) is null)
            SelectedItemText = picker.SelectedItem?.ToString() ?? "";
        else
            SelectedItemText = item.GetType().GetProperty(DisplayMemberPath)?.GetValue(item)?.ToString() ?? "";
        entry.Text = SelectedItemText;
        picker.SelectedItem = item;
    }
#if ANDROID
    private PopupWindow popupWindow = new();
#endif
    /// <summary>
    /// Shows the dropdown Listview. Currently for Android Only. To-Do: All Other Platforms.
    /// </summary>
    private void ShowDropdown()
    {
#if ANDROID
        if (this.Handler?.MauiContext == null) { return; }
        popupWindow?.Dismiss();
        var contentView = picker.ToPlatform(this.Handler.MauiContext);

        popupWindow = new(contentView, (int)(this.Width * 2), 300)
        {
            OutsideTouchable = true
        };

        var parentView = this.entry.ToPlatform(this.Handler.MauiContext);
        popupWindow.ShowAsDropDown(parentView);
#endif
    }
}

internal class BoolToColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return (bool)value ? Colors.Green : Colors.White;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}