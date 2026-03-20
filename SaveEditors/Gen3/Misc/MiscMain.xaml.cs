
using PKHeX.Core;

namespace PKHeXMAUI;

public partial class MiscMainEditor : ContentPage
{
    private readonly SAV3 SAV = (SAV3)MainPage.sav;
    public MiscMainEditor()
	{
		InitializeComponent();
        if (SAV is SAV3E em)
        {
            E_BP.Number = em.SmallBlock.BP;
            E_EarnedBP.Number = em.SmallBlock.BPEarned;
            E_EarnedBP.ValueChanged += (_, _) => em.SmallBlock.BPEarned = (ushort)E_EarnedBP.Number;
        }
        else
        {
            E_BP.IsVisible = L_BP.IsVisible = E_EarnedBP.IsVisible = L_EarnedBP.IsVisible = false;
        }
        if (SAV is SAV3FRLG frlg)
        {
            E_Rival.Text = frlg.RivalName;

            // Trainer Card Species
            Picker[] cba = [P_TCM1, P_TCM2, P_TCM3, P_TCM4, P_TCM5, P_TCM6];
            var legal = GameInfo.FilteredSources.Species.ToList();
            for (int i = 0; i < cba.Length; i++)
            {
                cba[i].ItemsSource = legal;
                cba[i].ItemDisplayBinding = new Binding("Text");
                var g3Species = SAV.GetWork(0x43 + i);
                var species = SpeciesConverter.GetNational3(g3Species);
                cba[i].SelectedIndex = (int)species;
            }
        }
        else
        {
            E_Rival.IsVisible = L_Rival.IsVisible = L_TrainerCards.IsVisible = false;
            Picker[] cba = [P_TCM1, P_TCM2, P_TCM3, P_TCM4, P_TCM5, P_TCM6];
            foreach (var c in cba)
            {
                c.IsVisible = false;
            }
        }
    }
}