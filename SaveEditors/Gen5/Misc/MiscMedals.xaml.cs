using PKHeX.Core;

namespace PKHeXMAUI;

public partial class MiscMedals : ContentPage
{
    public SAV5 SAV;
    private readonly string[] MedalNames = Util.GetStringList("medals", "en");
    private readonly string[] MedalTypeNames = Util.GetStringList("medal_types", "en");
    public MiscMedals(SAV5 sav)
	{
		InitializeComponent();
        SAV = sav;
        ReadMedals();
	}
    private void ReadMedals()
    {
        if (SAV is SAV5B2W2)
        {
            CB_CurrentMedal.ItemSource = MedalNames;
            CB_MedalState.ItemSource = new string[] { "Unobtained", "Can Obtain Hint Medal", "Hint Medal Obtained", "Can Obtain Medal", "Medal Obtained"};
            CB_CurrentMedal.SelectedIndex = 0;
        }
    }
    private void CB_CurrentMedal_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (SAV is SAV5B2W2 b2w2)
        {
            var index = CB_CurrentMedal.SelectedIndex;
            var medal = b2w2.Medals[index];
            var type = MedalList5.GetMedalType(index);
            TB_MedalType.Text = MedalTypeNames[(int)type];
            CB_MedalState.SelectedIndex = (int)medal.State;
            if (medal.CanHaveDate)
            {
                CAL_MedalDate.Date = medal.Date.ToDateTime(new TimeOnly());
                CAL_MedalDate.IsEnabled = true;
            }
            else
            {
                CAL_MedalDate.IsEnabled = false;
                CAL_MedalDate.DateSelected -= CAL_MedalDate_ValueChanged;
                CAL_MedalDate.Date = EncounterDate.GetDateNDS().ToDateTime(new TimeOnly());
                CAL_MedalDate.DateSelected += CAL_MedalDate_ValueChanged;
            }
            CHK_MedalUnread.IsChecked = medal.IsUnread;
        }
    }
    private void CAL_MedalDate_ValueChanged(object? sender, EventArgs e)
    {
        if (SAV is SAV5B2W2 b2w2)
        {
            var medal = b2w2.Medals[CB_CurrentMedal.SelectedIndex];
            medal.Date = DateOnly.FromDateTime((DateTime)CAL_MedalDate.Date);
        }
    }
    private void CB_MedalState_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (SAV is SAV5B2W2 b2w2)
        {
            var medal = b2w2.Medals[CB_CurrentMedal.SelectedIndex];
            medal.State = (Medal5State)CB_MedalState.SelectedIndex;
            if (medal.CanHaveDate)
            {
                if (!medal.HasDate)
                    medal.Date = EncounterDate.GetDateNDS();
                CAL_MedalDate.IsEnabled = true;
            }
            else
            {
                CAL_MedalDate.IsEnabled = false;
            }
        }
    }
    private void CHK_MedalUnread_CheckedChanged(object sender, EventArgs e)
    {
        if (SAV is SAV5B2W2 b2w2)
        {
            var medal = b2w2.Medals[CB_CurrentMedal.SelectedIndex];
            medal.IsUnread = CHK_MedalUnread.IsChecked;
        }
    }

    private void B_ObtainAllMedals_Click(object sender, EventArgs e)
    {
        if (SAV is SAV5B2W2 b2w2)
        {
            var now = EncounterDate.GetDateNDS();
            b2w2.Medals.ObtainAll(now, unread: true);
        }
    }
}