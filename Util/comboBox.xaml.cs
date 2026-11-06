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
    public static BindableProperty ItemSourceProperty = BindableProperty.Create(nameof(ItemSource), typeof(IEnumerable), typeof(comboBox),new List<object>(), propertyChanged:OnItemsCollectionChanged);
    /// <summary>Bindable property for <see cref="Title"/> </summary>
    public static BindableProperty TitleProperty = BindableProperty.Create(nameof(Title), typeof(string), typeof(comboBox));
    /// <summary>Bindable property for <see cref="Placeholder"/></summary>
    public static BindableProperty PlaceholderProperty = BindableProperty.Create(nameof(Placeholder), typeof(string), typeof(comboBox));
    ///<summary>Bindable property for <see cref="SelectedItem"/></summary>
    public static BindableProperty SelectedItemProperty = BindableProperty.Create(nameof(SelectedItem), typeof(object), typeof(comboBox), null, BindingMode.TwoWay, propertyChanged: SetSelectedItem);
    /// <summary>Bindable property for <see cref="SelectedIndex"/> </summary>
    public static BindableProperty SelectedIndexProperty = BindableProperty.Create(nameof(SelectedIndex), typeof(int), typeof(comboBox),-1,propertyChanged: SetSelectedIndex);
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
    public IList ItemSource { get =>(IList)GetValue(ItemSourceProperty); set =>SetValue(ItemSourceProperty, value); }
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
    public CollectionView picker;
    private bool translate;
    /// <summary>重入保护：IndexChanged 内部会回写 SelectedItem/SelectedIndex，可能再次触发自身，必须拦截避免无限递归与越界。</summary>
    private bool _indexChanging;
    /// <summary>
    /// 为 true 时把布尔/枚举的显示文本翻成中文（设置页属性表格用）。
    /// 只影响显示：候选列表文本与输入框文本都走 <see cref="Display"/>，实际取值仍是原始枚举/布尔。
    /// </summary>
    public bool Translate
    {
        get => translate;
        set
        {
            if (translate == value)
                return;
            translate = value;
            // 候选列表的显示文本要在开关变化后重建，否则过滤/自动补全还按英文比对
            if (ItemSource is not null)
                OnItemsCollectionChanged(this, EventArgs.Empty);
            // 输入框里的已选值也要重画：构造函数里先设了 SelectedItem（英文），随后才打开 Translate
            if (picker.SelectedItem is not null)
                entry.Text = Display(picker.SelectedItem);
        }
    }
    public comboBox()
    {
        InitializeComponent();
        picker = new()
        {
            BackgroundColor = Colors.White
        };
        picker.SelectionChanged += IndexChanged;
        picker.HeightRequest = 50;
        picker.SelectionMode = SelectionMode.Single;
        picker.ItemTemplate = new DataTemplate(() =>
        {
            Grid cell = [];
            Label label = new();
            // 候选列表也必须翻译，之前直接绑原始值，导致枚举/布尔在列表里仍是英文。
            //
            // 这里必须用 Binding.SelfPath（"."）而不是 DisplayMemberPath：
            // MAUI 的 Binding(path, converter) 会先按 path 把源对象解析成一个值，
            // 再把这个「已解析的值」交给 converter。若 path = DisplayMemberPath（例如 "Text"），
            // converter 收到的是字符串本身，而 Display() 又去该字符串上找 "Text" 属性，
            // 必然取不到 → 候选列表整片空白（物种/性格/持有装置/招式全都显示不出来）。
            // 改绑 SelfPath 后 converter 直接拿到整个 item，由 Display() 统一按
            // DisplayMemberPath 取值，显示与选中逻辑都不受影响。
            label.SetBinding(Label.TextProperty, new Binding(Binding.SelfPath, converter: new ComboDisplayConverter(this)));
            label.SetBinding(Label.BackgroundColorProperty, new Binding("Valid", converter: new BoolToColorConverter()));
            label.TextColor = Colors.Black;
            cell.Add(label);
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
        if (picker is null) return;
        // 候选行的文字是「绑定求值那一刻」算出来的。DisplayMemberPath 改动后，
        // 已经生成的行不会自动重算，这里重建一次条目让显示立即刷新。
        picker.ItemsSource = ItemSource;
    }
    private string GetDisplayMember(object item) => Display(item);

    /// <summary>把候选项转成用于界面显示的文本（Translate 为 true 时译成中文）。</summary>
    internal string Display(object? item)
    {
        if (item is null)
            return string.Empty;
        if (DisplayMemberPath is "." or "")
            return Translate ? Loc.Value(item) : item.ToString() ?? "";
        var prop = item.GetType().GetProperty(DisplayMemberPath);
        // 取不到指定属性时退回 ToString()，避免整行空白
        if (prop is null)
            return Translate ? Loc.Value(item) : item.ToString() ?? "";
        return prop.GetValue(item)?.ToString() ?? "";
    }
    /// <summary>
    /// Filter's the items in the ListView based on the text in the entry.
    /// </summary>
    /// <param name="sender">the object calling the event</param>
    /// <param name="e">Event args</param>
    private void entry_textChanged(object sender, TextChangedEventArgs e)
    {
        if (picker.ItemsSource is null) return;
        // 只有用户手动输入时才过滤；程序设置 Text（如切换选中项）不应改动候选列表
        if (!entry.IsFocused) return;
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
        picker.ItemsSource = ItemSource.Cast<object>().Where(z => tempsource.Contains(GetDisplayMember(z))).ToList();
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
        if (_indexChanging)
            return;
        if (picker.SelectedItem is null)
            return;
        if (ItemSource is null)
            return;
        _indexChanging = true;
        try
        {
            SelectedItemText = Display(picker.SelectedItem);

            entry.Text = SelectedItemText;
            SelectedItem = picker.SelectedItem;
            var list = ItemSource.Cast<object>().ToList();
            int idx = list.IndexOf(SelectedItem);
            SelectedIndex = idx; // -1 时 SetSelectedIndex 会直接 return，不会越界
            SelectedIndexChanged?.Invoke(this, e!);
        }
        finally
        {
            _indexChanging = false;
        }
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
        var src = lv.ItemSource;
        if (src is null) return;
        var list = src.Cast<object>().ToList();
        int idx = (int)newValue;
        if (idx < 0 || idx >= list.Count) return; // 越界保护：SelectedIndex 超出候选数量时不索引，避免 IndexOutOfRangeException
        lv.SelectedItem = list[idx];
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
        var item = ItemSource.Cast<object>().FirstOrDefault(z => tempsource.Contains(GetDisplayMember(z)))??"";
        SelectedItemText = Display(item);
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
        // 每次展开前恢复完整列表，避免上次输入过滤后只剩一条
        if (!ReferenceEquals(picker.ItemsSource, ItemSource))
            picker.ItemsSource = ItemSource;
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
{    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is null) return Colors.White;
        return (bool)value ? Colors.Green : Colors.White;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// 候选列表的显示转换：把枚举/布尔按 <see cref="comboBox.Translate"/> 译成中文。
/// 只作用于显示，选中的对象仍是原始值，取值逻辑不受影响。
/// </summary>
internal class ComboDisplayConverter : IValueConverter
{
    private readonly comboBox owner;
    public ComboDisplayConverter(comboBox owner) => this.owner = owner;
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => owner.Display(value);
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}