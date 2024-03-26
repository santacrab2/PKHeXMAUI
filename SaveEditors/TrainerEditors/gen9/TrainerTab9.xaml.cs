namespace PKHeXMAUI;

public partial class TrainerTab9 : TabbedPage
{
	public TrainerTab9()
	{
		InitializeComponent();
        trainertab9.BarBackgroundColor = Color.FromArgb("303030");
        trainertab9.BarTextColor = Colors.White;
        trainertab9.Children.Add(new TrainerEditor9());
    }
}