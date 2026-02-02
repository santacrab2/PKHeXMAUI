using PKHeX.Core;

namespace PKHeXMAUI;

public partial class MiscEntree : ContentPage
{
	public SAV5 SAV;
	public MiscEntree(SAV5 sav)
	{
		SAV = sav;
        InitializeComponent();
        CV_FunfestMissions.ItemTemplate = new DataTemplate(() =>
        {
            var Label = new Label();
            Label.SetBinding(Label.TextProperty, ".");
            return Label;
        });
    }
    private void ReadEntralink()
	{
        var entree = SAV.Entralink;
        NUD_EntreeWhiteLV.Number = entree.WhiteForestLevel;
        NUD_EntreeBlackLV.Number = entree.BlackCityLevel;
        if (SAV is SAV5B2W2 b2w2)
        {
            var pass = (Entralink5B2W2)entree;
            var ppv = Enum.GetValues<PassPower5>();
            var ppn = Enum.GetNames(typeof(PassPower5));
            var PassPowerB = new ComboItem[ppv.Length];
            for (int i = 0; i < ppv.Length; i++)
                PassPowerB[i] = new ComboItem(ppn[i], (int)ppv[i]);
            foreach (var cb in (comboBox[])[CB_PassPower1, CB_PassPower2, CB_PassPower3])
            {
                cb.ItemSource = PassPowerB;
            }

            CB_PassPower1.SelectedItem = (int)pass.PassPower1;
            CB_PassPower2.SelectedItem = (int)pass.PassPower2;
            CB_PassPower3.SelectedItem = (int)pass.PassPower3;
            var block = b2w2.Festa;
            NUD_FMHosted.Number = block.Hosted;
            NUD_FMParticipated.Number = block.Participated;
            NUD_FMCompleted.Number = block.Completed;
            NUD_FMTopScores.Number = block.TopScores;
            NUD_FMMostParticipants.Number = block.Participants;
            NUD_EntreeWhiteEXP.Number = block.WhiteEXP;
            NUD_EntreeBlackEXP.Number = block.BlackEXP;
            string[] FMTitles = Enum.GetNames(typeof(Funfest5Mission));
            CV_FunfestMissions.ItemsSource = FMTitles;
            string[] levels = ["Lv.1", "Lv.2 +", "Lv.3 ++", "Lv.3 +++"];
            CB_FMLevel.ItemSource = levels;
            CV_FunfestMissions.SelectedItem = FMTitles[0];
        }
        else
        {
            NUD_EntreeWhiteEXP.IsVisible = NUD_EntreeBlackEXP.IsVisible = false;
        }
    }

    private void B_FunfestMissions_Click(object sender, EventArgs e)
    {
        FestaBlock5 block = ((SAV5B2W2)SAV).Festa;
        block.UnlockAllFunfestMissions();
    }

    private void CV_FunfestMissions_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        LoadFestaMissionRecord();
    }
    private void LoadFestaMissionRecord()
    {
        FestaBlock5 block = ((SAV5B2W2)SAV).Festa;
        int mission = Array.IndexOf([..CV_FunfestMissions.ItemsSource],CV_FunfestMissions.SelectedItem);
        if ((uint)mission > FestaBlock5.MaxMissionIndex)
            return;
        bool unlocked = block.IsFunfestMissionUnlocked(mission);
        L_FMLocked.Text = !unlocked ? "Locked" : "Unlocked";

        var record = block.GetMissionRecord(mission);
        CHK_FMNew.IsChecked = record.IsNew;
        CB_FMLevel.SelectedIndex = record.Level;
        NUD_FMBestScore.Number = (record.Score);
        NUD_FMBestTotal.Number = (record.Total);
    }
    private void ChangeFestaMissionValue(object sender, EventArgs e)
    {

        FestaBlock5 block = ((SAV5B2W2)SAV).Festa;
        int mission = Array.IndexOf([..CV_FunfestMissions.ItemsSource],CV_FunfestMissions.SelectedItem);
        if ((uint)mission > FestaBlock5.MaxMissionIndex)
            return;

        var score = new Funfest5Score((int)NUD_FMBestTotal.Number, (int)NUD_FMBestScore.Number, CB_FMLevel.SelectedIndex & 3, CHK_FMNew.IsChecked);
        block.SetMissionRecord(mission, score);
    }
    public void SaveEntralink()
    {
        var entree = SAV.Entralink;
        entree.WhiteForestLevel = (ushort)NUD_EntreeWhiteLV.Number;
        entree.BlackCityLevel = (ushort)NUD_EntreeBlackLV.Number;

        if (SAV is SAV5B2W2 b2w2)
        {
            var pass = (Entralink5B2W2)entree;
            if (CB_PassPower1.SelectedIndex >= 0)
                pass.PassPower1 = (byte)CB_PassPower1.SelectedIndex;
            if (CB_PassPower2.SelectedIndex >= 0)
                pass.PassPower2 = (byte)CB_PassPower2.SelectedIndex;
            if (CB_PassPower3.SelectedIndex >= 0)
                pass.PassPower3 = (byte)CB_PassPower3.SelectedIndex;

            var block = b2w2.Festa;
            block.Hosted = (ushort)NUD_FMHosted.Number;
            block.Participated = (ushort)NUD_FMParticipated.Number;
            block.Completed = (ushort)NUD_FMCompleted.Number;
            block.TopScores = (ushort)NUD_FMTopScores.Number;
            block.WhiteEXP = (byte)NUD_EntreeWhiteEXP.Number;
            block.BlackEXP = (byte)NUD_EntreeBlackEXP.Number;
            block.Participants = (byte)NUD_FMMostParticipants.Number;
        }
    }
}