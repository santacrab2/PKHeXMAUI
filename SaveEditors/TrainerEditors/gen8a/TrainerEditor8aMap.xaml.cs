
using PKHeX.Core;

namespace PKHeXMAUI;

public partial class TrainerEditor8aMap : ContentPage
{
	public SAV8LA SAV = (SAV8LA)MainPage.sav;
	public TrainerEditor8aMap()
	{
		InitializeComponent();
		TE8aCMapEntry.Text = SAV.Coordinates.M;
		TE8aXEntry.Number = (decimal)SAV.Coordinates.X;
		TE8aZEntry.Number = (decimal)SAV.Coordinates.Z;
		TE8aYEntry.Number = (decimal)SAV.Coordinates.Y;
		TE8aREntry.Number = (decimal)(Math.Atan2(SAV.Coordinates.RZ, SAV.Coordinates.RW) * 360.0 / Math.PI);
    }

	public void SaveTE8aMap()
	{
		SAV.Coordinates.M = TE8aCMapEntry.Text;
		SAV.Coordinates.X = (float)TE8aXEntry.Number;
		SAV.Coordinates.Z = (float)TE8aZEntry.Number;
		SAV.Coordinates.Y = (float)TE8aYEntry.Number;

	    var angle = (float)TE8aREntry.Number * Math.PI / 360.0;
		SAV.Coordinates.RX = 0;
		SAV.Coordinates.RZ = (float)Math.Sin(angle);
		SAV.Coordinates.RY = 0;
		SAV.Coordinates.RW = (float)Math.Cos(angle);
	}
}