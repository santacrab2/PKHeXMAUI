using PKHeX.Core;

namespace pk9reader;

public partial class AppShell : Shell
{
	public AppShell(SaveFile sav)
	{
        AppSaveFile = sav;
		
        InitializeComponent();
        pkeditortab.Items.Insert(0, new MainPage());

    }
	public static SaveFile AppSaveFile { get; set; }
}
