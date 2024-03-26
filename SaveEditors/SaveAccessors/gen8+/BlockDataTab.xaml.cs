using PKHeX.Core;

namespace PKHeXMAUI;

public partial class BlockDataTab : TabbedPage
{
	public BlockDataTab()
	{
		InitializeComponent();;
        BDT.BarBackgroundColor = Color.FromArgb("303030");
        BDT.BarTextColor = Colors.White;
        BDT.Children.Add(new BlockEditor8((ISCBlockArray)MainPage.sav));
	}
}