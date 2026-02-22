using System.Globalization;

namespace PKHeXMAUI;

public partial class NumericUpDown : ContentView
{
    public static BindableProperty MaxValueProperty = BindableProperty.Create(nameof(MaxValue), typeof(decimal), typeof(NumericUpDown), decimal.MaxValue);
    public static BindableProperty MinValueProperty = BindableProperty.Create(nameof(MinValue), typeof(decimal), typeof(NumericUpDown), decimal.MinValue);
	public static BindableProperty NumberProperty = BindableProperty.Create(nameof(Number), typeof(decimal), typeof(NumericUpDown), (decimal)0, propertyChanged: SetEntryNumber);
	public decimal Number { get => (decimal)GetValue(NumberProperty); set=>SetValue(NumberProperty,value); }
    public decimal MaxValue { get=>(decimal)GetValue(MaxValueProperty); set=>SetValue(MaxValueProperty,value); }
    public decimal MinValue { get => (decimal)GetValue(MinValueProperty); set => SetValue(MinValueProperty, value); }
    public event EventHandler? ValueChanged;
	public NumericUpDown()
	{
		InitializeComponent();
        E_Number.SetBinding(Entry.TextProperty, new Binding("Number",BindingMode.TwoWay,new intConverter(), source: ThisView));
	}
	public static void SetEntryNumber(object bindable, object oldValue,object newValue)
	{
        if((decimal)newValue+1>((NumericUpDown)bindable).MaxValue)
            newValue = ((NumericUpDown)bindable).MaxValue;
        if ((decimal)newValue < ((NumericUpDown)bindable).MinValue)
            newValue = ((NumericUpDown)bindable).MinValue;
        ((NumericUpDown)bindable).E_Number.Text = newValue.ToString();
        ((NumericUpDown)bindable).ValueChanged?.Invoke(bindable, EventArgs.Empty);
	}

    private void Increase(object sender, EventArgs e)
    {
        if (Number+1 >= MaxValue)
            Number = MaxValue;
        else
            Number++;
    }

    private void Decrease(object sender, EventArgs e)
    {
        if (Number <= MinValue)
            Number = MinValue;
        else
            Number--;
    }

    private void EnforceLimitations(object sender, TextChangedEventArgs e)
    {
        if (Number + 1 > MaxValue)
            Number = MaxValue;
        if (Number < MinValue)
            Number = MinValue;
    }
}
public class intConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value?.ToString();
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (decimal.TryParse(value?.ToString()?.ToCharArray() ?? [], out var result))
            return result;
        return value;
    }
}