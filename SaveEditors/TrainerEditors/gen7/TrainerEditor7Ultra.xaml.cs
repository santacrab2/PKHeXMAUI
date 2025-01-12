
using PKHeX.Core;

namespace PKHeXMAUI;

public partial class TrainerEditor7Ultra : ContentPage
{
	public static SAV7 SAV = (SAV7)MainPage.sav;
	public TrainerEditor7Ultra()
	{
		InitializeComponent();
		MeleSurfScoreEntry.Number = SAV.Misc.GetSurfScore(0);
		AkaSurfScoreEntry.Number = SAV.Misc.GetSurfScore(1);
		UlaSurfScoreEntry.Number = SAV.Misc.GetSurfScore(2);
		PoniSurfScoreEntry.Number = SAV.Misc.GetSurfScore(3);
		RotomOTEntry.Text = SAV.FieldMenu.RotomOT;
		AffectionEntry.Number = SAV.FieldMenu.RotomAffection;
		Loto1Check.IsChecked = SAV.FieldMenu.RotomLoto1;
		Loto2Check.IsChecked = SAV.FieldMenu.RotomLoto2;
	}
	public void SaveTE7U()
	{
		SAV.Misc.SetSurfScore(0, (int)MeleSurfScoreEntry.Number);
		SAV.Misc.SetSurfScore(1, (int)AkaSurfScoreEntry.Number);
		SAV.Misc.SetSurfScore(2, (int)UlaSurfScoreEntry.Number);
		SAV.Misc.SetSurfScore(3, (int)PoniSurfScoreEntry.Number);
		SAV.FieldMenu.RotomOT = RotomOTEntry.Text;
		SAV.FieldMenu.RotomAffection = (ushort)AffectionEntry.Number;
		SAV.FieldMenu.RotomLoto1 = Loto1Check.IsChecked;
		SAV.FieldMenu.RotomLoto2 = Loto2Check.IsChecked;
	}
}