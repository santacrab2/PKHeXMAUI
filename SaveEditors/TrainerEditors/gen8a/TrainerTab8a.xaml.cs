
namespace PKHeXMAUI;

public partial class TrainerTab8a : TabbedPage
{
    public static TrainerEditor8a? TE8a;
    public static TrainerEditor8aMap? TE8aM;
	public TrainerTab8a()
	{
		InitializeComponent();
        TE8a = new();
        TE8aM = new();
        trainertab8a.BarBackgroundColor = Color.FromArgb("303030");
        trainertab8a.BarTextColor = Colors.White;
        trainertab8a.Children.Add(TE8a);
        trainertab8a.Children.Add(TE8aM);
        trainertab8a.Children.Add(new SavePage8a());
        trainertab8a.Children.Add(new cancelpage());
    }
}

public partial class SavePage8a : ContentPage
{
    public SavePage8a()
    {
        this.Title = L10n.T("common_save");
    }
    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        TrainerTab8a.TE8a?.SaveTE8a();
        TrainerTab8a.TE8aM?.SaveTE8aMap();
        Navigation.PopModalAsync();
    }
}