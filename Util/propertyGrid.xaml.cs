using CommunityToolkit.Maui.Views;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;

namespace PKHeXMAUI;

public partial class propertyGrid : ContentView
{
    public BindableProperty CurrentItemProperty = BindableProperty.Create(nameof(CurrentItem), typeof(object), typeof(propertyGrid),propertyChanged: OnPChanged);
    public static Dictionary<string, List<PropertyInfo>>? Categories;
    public object CurrentItem { get=>GetValue(CurrentItemProperty); set=>SetValue(CurrentItemProperty,value); }
    private bool init = true;
    public propertyGrid(object item)
    {
        InitializeComponent();
        Categories = [];
        CurrentItem = item;
        PGrid.Clear();
        var props = item.GetType().GetProperties().OrderBy(z=>z.Name);
        foreach (var prop in props)
        {
            CategoryAttribute att = prop.GetCustomAttribute<CategoryAttribute>() ?? new CategoryAttribute("Misc");
            if (Categories.TryGetValue(att.Category, out List<PropertyInfo>? value))
                value.Add(prop);
            else
                Categories.Add(att.Category,[prop]);
        }
        int p = 0;
        foreach (var cat in Categories)
        {
            Label label = new()
            {
                Text = cat.Key+ "▽"
            };
            Expander expander = new() { Header = label };
            var stack = new Grid();
            for (int i = 0; i < cat.Value.Count; i++)
            {
                var pi = cat.Value[i];
                Label plabel = new() { Text = pi.Name, VerticalOptions = LayoutOptions.Center };
                stack.Add(plabel, 0, i);
                if (pi.PropertyType.IsEnum)
                {
                    stack.Add(GetEnumCombo(pi), 1, i);
                }
                else if (pi.PropertyType.IsClass)
                {

                    stack.Add(GetClassExpander(pi), 1, i);
                }
                else if (pi.PropertyType.IsSZArray)
                {
                    stack.Add(GetArrayExpander(pi), 1, i);
                }
                else
                {
                   
                    stack.Add(GetPropertyEntry(pi), 1, i);
                    
                }
            }
            expander.Content = stack;
            PGrid.RowDefinitions.Add(new() { Height = GridLength.Star });
            PGrid.Add(expander,row:p);
            p++;
        }
        init = false;
    }
    static void OnPChanged(BindableObject bindable,object oldValue, object newValue)
    {
        ((propertyGrid)bindable).PChanged();
    }
    public void PChanged()
    {
        if (init) return;
        Categories = [];
        PGrid.Clear();
        var props = CurrentItem.GetType().GetProperties().OrderBy(z=>z.Name);
        foreach (var prop in props)
        {
            CategoryAttribute att = prop.GetCustomAttribute<CategoryAttribute>() ?? new CategoryAttribute("Misc");
            if (Categories.TryGetValue(att.Category, out List<PropertyInfo>? value))
                value.Add(prop);
            else
                Categories.Add(att.Category, [prop]);
        }
        int p = 0;
        foreach (var cat in Categories)
        {
            Label label = new()
            {
                Text = cat.Key + "▽"
            };
            Expander expander = new() { Header = label };
            var stack = new Grid();
            for (int i = 0; i < cat.Value.Count; i++)
            {
                var pi = cat.Value[i];
                Label plabel = new() { Text = pi.Name, VerticalOptions = LayoutOptions.Center };
                stack.Add(plabel, 0, i);
                if (pi.PropertyType.IsEnum)
                {
                    stack.Add(GetEnumCombo(pi), 1, i);
                }
                else if (pi.PropertyType.IsClass)
                {
                    Label L_intern = new() { Text = pi.PropertyType.ToString() + "▽" };
                    Expander E_intern = new() { Header = L_intern, Margin = 20 };
                    Grid G_stack = [];
                    int o = 0;

                    foreach (var pr in pi.PropertyType.GetProperties().OrderBy(z=>z.Name))
                    {
                        try
                        {
                            if (pi.PropertyType.GetCustomAttribute<TypeConverterAttribute>() == null) { L_intern.Text = pi.PropertyType.ToString(); break; };
                            Label L_prop = new() { Text = pr.Name };
                            G_stack.Add(L_prop, 0, o);
                            if (pr.PropertyType.IsEnum)
                            {

                                G_stack.Add(GetEnumCombo(pi, pr), 1, o);
                            }
                            else
                            {

                                G_stack.Add(GetPropertyEntry(pi, pr), 1, o);

                            }
                            o++;
                        }
                        catch (Exception) { continue; }
                    }
                    E_intern.Content = G_stack;
                    stack.Add(E_intern, 1, i);
                }
                else
                {

                    stack.Add(GetPropertyEntry(pi), 1, i);

                }
            }
            expander.Content = stack;
            PGrid.Add(expander,0, p);
            p++;
        }
    }
    public comboBox GetEnumCombo(PropertyInfo CurrentProperty)
    {
        comboBox cb = new()
        {
            ItemSource = Enum.GetValues(CurrentProperty.PropertyType),
            SelectedItem = CurrentProperty?.GetValue(CurrentItem) ?? new()
        };
        cb.SelectedIndexChanged += (_, _) => CurrentProperty?.SetValue(CurrentItem, cb.SelectedItem); 
        return cb;
    }
    public comboBox GetEnumCombo(PropertyInfo upperProperty, PropertyInfo LowerProperty)
    {
        comboBox cb = new()
        {
            ItemSource = Enum.GetValues(LowerProperty.PropertyType),
            SelectedItem = LowerProperty?.GetValue(upperProperty?.GetValue(CurrentItem)) ?? null
        };
        cb.SelectedIndexChanged += (_, _) => LowerProperty?.SetValue(upperProperty?.GetValue(CurrentItem), cb.SelectedItem);
        return cb;
    }
    public comboBox GetEnumCombo(object Value, PropertyInfo CurrentProperty)
    {
        comboBox cb = new()
        {
            ItemSource = Enum.GetValues(CurrentProperty.PropertyType),
            SelectedItem = CurrentProperty?.GetValue(Value) ?? new()
        };
        cb.SelectedIndexChanged += (_, _) => CurrentProperty?.SetValue(Value, cb.SelectedItem); 
        return cb;

    }
    public comboBox GetEnumCombo(object value)
    {
        comboBox cb = new()
        {
            ItemSource = Enum.GetValues(value.GetType()),
            SelectedItem = value
        };
        cb.SelectedIndexChanged += (_, _) => value = cb.SelectedItem;
        return cb;
    }
    public Entry GetPropertyEntry(PropertyInfo CurrentProperty)
    {
        Entry pentry = new();
        try
        {
            pentry.Text = CurrentProperty.GetValue(CurrentItem)?.ToString();
        }
        catch (Exception) { pentry.Text = "Method is not Supported"; }
        pentry.TextChanged += (_, _) =>
        {
            try
            {
                if (string.IsNullOrEmpty(pentry.Text)) return;
                var value = Convert.ChangeType(pentry.Text, CurrentProperty.PropertyType);
                CurrentProperty.SetValue(CurrentItem, value);
            }
            catch (Exception) { }
        };
        return pentry;
    }
    public Entry GetPropertyEntry(PropertyInfo UpperProperty, PropertyInfo LowerProperty)
    {
        Entry pentry = new();
        try
        {
            pentry.Text = LowerProperty.GetValue(UpperProperty?.GetValue(CurrentItem))?.ToString();
        }
        catch (Exception) { pentry.Text = "Method is not Supported"; }
        pentry.TextChanged += (_, _) =>
        {
            try
            {
                if (string.IsNullOrEmpty(pentry.Text)) return;
                var value = Convert.ChangeType(pentry.Text, LowerProperty.PropertyType);
                LowerProperty.SetValue(UpperProperty?.GetValue(CurrentItem), value);
            }
            catch (Exception) { }
        };
        return pentry;
    }
    public Entry GetPropertyEntry(object? Value, PropertyInfo CurrentProperty)
    {
        Entry pentry = new();
        try
        {
            pentry.Text = CurrentProperty.GetValue(Value)?.ToString();
        }
        catch (Exception) { pentry.Text = "Method is not Supported"; }
        pentry.TextChanged += (_, _) =>
        {
            try
            {
                if (string.IsNullOrEmpty(pentry.Text)) return;
                var value = Convert.ChangeType(pentry.Text, CurrentProperty.PropertyType);
                CurrentProperty.SetValue(Value, value);
            }
            catch (Exception) { }
        };
        return pentry;
    }
    public Expander GetClassExpander(PropertyInfo CurrentProperty)
    {
        Label L_intern = new() { Text = CurrentProperty.PropertyType.ToString() + "▽" };
        Expander E_intern = new() { Header = L_intern };
        Grid G_stack = [];
        int o = 0;
        foreach (var pr in CurrentProperty.PropertyType.GetProperties().OrderBy(z=>z.Name))
        {
            
                if (CurrentProperty.PropertyType.GetCustomAttribute<TypeConverterAttribute>() == null) { L_intern.Text = CurrentProperty.PropertyType.ToString(); break; };
                Label L_prop = new() { Text = pr.Name };
                G_stack.Add(L_prop, 0, o);
                if (pr.PropertyType.IsEnum)
                {

                    G_stack.Add(GetEnumCombo(CurrentProperty, pr), 1, o);
                }
                else if (pr.PropertyType.IsSZArray)
                {
                    G_stack.Add(GetArrayExpander(CurrentProperty?.GetValue(CurrentItem), pr), 1, o);
                }
                else
                {

                    G_stack.Add(GetPropertyEntry(CurrentProperty, pr), 1, o);

                }
                o++;
            
  
        }
        E_intern.Content = G_stack;
        return E_intern;
    }
    public Expander GetClassExpander(object currentobj)
    {
        Label L_intern = new() { Text = currentobj.GetType().ToString() + "▽" };
        Expander E_intern = new() { Header = L_intern};
        Grid G_stack = [];
        int o = 0;
        foreach (var pr in currentobj.GetType().GetProperties().OrderBy(z=>z.Name))
        {

            
            Label L_prop = new() { Text = pr.Name };
            G_stack.Add(L_prop, 0, o);
            if (pr.PropertyType.IsEnum)
            {

                G_stack.Add(GetEnumCombo(currentobj), 1, o);
            }
            else if (pr.PropertyType.IsSZArray)
            {
                G_stack.Add(GetArrayExpander(currentobj, pr), 1, o);
            }
            else
            {

                G_stack.Add(GetPropertyEntry(currentobj, pr), 1, o);

            }
            o++;


        }
        E_intern.Content = G_stack;
        return E_intern;
    }
    public Expander GetArrayExpander(PropertyInfo CurrentProperty)
    {
        Label L_intern = new() { Text = CurrentProperty.PropertyType.ToString() + "▽" };
        Expander E_intern = new() { Header = L_intern };
        Grid A_stack = [];
        ICollection PropertyArray = (ICollection?)CurrentProperty?.GetValue(CurrentItem) ?? new List<object>();
        var PropertyArrayList = PropertyArray.Cast<object>().ToArray();
        for (int i =0;i<PropertyArray.Count;i++)
        {
            var PropertyArrayItem = PropertyArrayList[i];
           var PAItemProperties = PropertyArrayItem.GetType().GetProperties().OrderBy(z=>z.Name).ToArray();
           for (int a =0;a<PAItemProperties.Length;a++)
            {
                var PAItemProperty = PAItemProperties[a];
                Label L_prop = new() { Text = PAItemProperty.Name };
                A_stack.Add(L_prop, 0, a);
                if (PAItemProperty.PropertyType.IsEnum)
                {
                   A_stack.Add(GetEnumCombo(PropertyArrayItem,PAItemProperty), 1, a);
                }
                else if (PAItemProperty.PropertyType.IsClass)
                {
                    A_stack.Add(GetClassExpander(PAItemProperty), 1, a);
                }
                else
                {
                    A_stack.Add(GetPropertyEntry(PropertyArrayItem, PAItemProperty), 1, a);
                }
            }
        }
        E_intern.Content = A_stack;
        return E_intern;
    }
    public Expander GetArrayExpander(object? Value,PropertyInfo CurrentProperty)
    {
        
        Label L_intern = new() { Text = CurrentProperty.PropertyType.ToString() + "▽" };
        Expander E_intern = new() { Header = L_intern };
        if (Value == null) return E_intern;
        Grid A_stack = [];
        ICollection PropertyArray = (ICollection?)CurrentProperty?.GetValue(Value) ?? new List<object>();
        var PropertyArrayList = PropertyArray.Cast<object>().ToList();
        for (int i = 0; i < PropertyArray.Count; i++)
        {
            var PropertyArrayItem = PropertyArrayList[i];
            
                Label L_prop = new() { Text = PropertyArrayItem.ToString() };
                A_stack.Add(L_prop, 0, i);
                if (PropertyArrayItem.GetType().IsEnum)
                {
                    A_stack.Add(GetEnumCombo(PropertyArrayItem), 1, i);
                }
                else if (PropertyArrayItem.GetType().IsClass)
                {
                    A_stack.Add(GetClassExpander(PropertyArrayItem), 1, i);
                }
                else
                {
                    A_stack.Add(GetPropertyEntry(PropertyArrayItem, CurrentProperty), 1, i);
                }
        }
        E_intern.Content = A_stack;
        return E_intern;
    }
}
