using PKHeX.Core;

namespace pk9reader;

public partial class AppShell : Shell
{
	public AppShell(SaveFile sav)
	{
        AppSaveFile = sav;
        InitializeComponent();
		
	}
	public static SaveFile AppSaveFile { get; set; }
}
