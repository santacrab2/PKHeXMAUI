using PKHeX.Core;

namespace PKHeXMAUI;

public partial class AppShell : Shell
{
	public AppShell(SaveFile sav)
	{
        AppSaveFile = sav;
		
        InitializeComponent();
       

    }
	public static SaveFile AppSaveFile { get; set; }
}
