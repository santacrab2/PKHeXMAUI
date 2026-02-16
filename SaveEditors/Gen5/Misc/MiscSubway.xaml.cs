using PKHeX.Core;

namespace PKHeXMAUI;

public partial class MiscSubway : ContentPage
{
    private readonly SAV5 SAV;
    private readonly BattleSubwayPlay5 swp;
    private readonly BattleSubway5 sw;
    public MiscSubway(SAV5 sav)
	{
		InitializeComponent();
        SAV = sav;
        swp = SAV.BattleSubwayPlay;
        sw = SAV.BattleSubway;
        ReadSubway();
    }
    private void ReadSubway()
    {
        // Running Battle Subway Data
        NUD_CurrentType.Number = swp.CurrentType;
        NUD_CurrentBattle.Number = swp.CurrentBattle;

        // Save Normal Checks
        CHK_Subway0.IsChecked = sw.Flag0;
        CHK_Subway1.IsChecked = sw.Flag1;
        CHK_Subway2.IsChecked = sw.Flag2;
        CHK_Subway7.IsChecked = sw.Flag3;

        // Save Super Checks
        CHK_SuperSingle.IsChecked = sw.SuperSingle;
        CHK_SuperDouble.IsChecked = sw.SuperDouble;
        CHK_SuperMulti.IsChecked = sw.SuperMulti;
        CHK_Subway7.IsChecked = sw.Flag7;

        // NPC Met Flag
        CHK_SWNPCMet.IsChecked = sw.NPCMet;

        // Current Run Checks
        CHK_SingleSet.IsChecked = sw.SingleSet == ((sw.SinglePast / 7) + 1);
        L_SinglePast.Text = CHK_SingleSet.IsChecked ? "Current" : "Past";
        CHK_DoubleSet.IsChecked = sw.DoubleSet == ((sw.DoublePast / 7) + 1);
        L_DoublePast.Text = CHK_DoubleSet.IsChecked ? "Current" : "Past";
        CHK_MultiNPCSet.IsChecked = sw.MultiNPCSet == ((sw.MultiNPCPast / 7) + 1);
        L_MultiNpcPast.Text = CHK_MultiNPCSet.IsChecked ? "Current" : "Past";
        CHK_MultiFriendsSet.IsChecked = sw.MultiFriendsSet == ((sw.MultiFriendsPast / 7) + 1);
        L_MultiFriendsPast.Text = CHK_MultiFriendsSet.IsChecked ? "Current" : "Past";
        CHK_SuperSingleSet.IsChecked = sw.SuperSingleSet == ((sw.SuperSinglePast / 7) + 1);
        L_SSinglePast.Text = CHK_SuperSingleSet.IsChecked ? "Current" : "Past";
        CHK_SuperDoubleSet.IsChecked = sw.SuperDoubleSet == ((sw.SuperDoublePast / 7) + 1);
        L_SDoublePast.Text = CHK_SuperDoubleSet.IsChecked ? "Current" : "Past";
        CHK_SuperMultiNPCSet.IsChecked = sw.SuperMultiNPCSet == ((sw.SuperMultiNPCPast / 7) + 1);
        L_SMultiNpcPast.Text = CHK_SuperMultiNPCSet.IsChecked ? "Current" : "Past";
        CHK_SuperMultiFriendsSet.IsChecked = sw.SuperMultiFriendsSet == ((sw.SuperMultiFriendsPast / 7) + 1);
        L_SMultiFriendsPast.Text = CHK_SuperMultiFriendsSet.IsChecked ? "Current" : "Past";

        // Normal
        // Single
        NUD_SinglePast.Number = sw.SinglePast;
        NUD_SingleRecord.Number = sw.SingleRecord;

        // Double
        NUD_DoublePast.Number = sw.DoublePast;
        NUD_DoubleRecord.Number = sw.DoubleRecord;

        // Multi NPC
        NUD_MultiNpcPast.Number = sw.MultiNPCPast;
        NUD_MultiNpcRecord.Number = sw.MultiNPCRecord;

        // Multi Friends
        NUD_MultiFriendsPast.Number = sw.MultiFriendsPast;
        NUD_MultiFriendsRecord.Number = sw.MultiFriendsRecord;

        // Super
        // Single
        NUD_SSinglePast.Number = sw.SuperSinglePast;
        NUD_SSingleRecord.Number = sw.SuperSingleRecord;

        // Double
        NUD_SDoublePast.Number = sw.SuperDoublePast;
        NUD_SDoubleRecord.Number = sw.SuperDoubleRecord;

        // Multi NPC
        NUD_SMultiNpcPast.Number = sw.SuperMultiNPCPast;
        NUD_SMultiNpcRecord.Number = sw.SuperMultiNPCRecord;

        // Multi Friends
        NUD_SMultiFriendsPast.Number = sw.SuperMultiFriendsPast;
        NUD_SMultiFriendsRecord.Number = sw.SuperMultiFriendsRecord;
    }

    public void SaveSubway()
    {
        // Running Battle Subway Data
        swp.CurrentType = (int)NUD_CurrentType.Number;
        swp.CurrentBattle = (int)NUD_CurrentBattle.Number;

        // Save Normal Checks
        sw.Flag0 = CHK_Subway0.IsChecked;
        sw.Flag1 = CHK_Subway1.IsChecked;
        sw.Flag2 = CHK_Subway2.IsChecked;
        sw.Flag3 = CHK_Subway7.IsChecked;

        // Save Super Checks
        sw.SuperSingle = CHK_SuperSingle.IsChecked;
        sw.SuperDouble = CHK_SuperDouble.IsChecked;
        sw.SuperMulti = CHK_SuperMulti.IsChecked;
        sw.Flag7 = CHK_Subway7.IsChecked;

        // NPC Met Flag
        sw.NPCMet = CHK_SWNPCMet.IsChecked;

        // Normal
        // Single
        sw.SinglePast = (int)NUD_SinglePast.Number;
        sw.SingleRecord = (int)NUD_SingleRecord.Number;

        // Double
        sw.DoublePast = (int)NUD_DoublePast.Number;
        sw.DoubleRecord = (int)NUD_DoubleRecord.Number;

        // Multi NPC
        sw.MultiNPCPast = (int)NUD_MultiNpcPast.Number;
        sw.MultiNPCRecord = (int)NUD_MultiNpcRecord.Number;

        // Multi Friends
        sw.MultiFriendsPast = (int)NUD_MultiFriendsPast.Number;
        sw.MultiFriendsRecord = (int)NUD_MultiFriendsRecord.Number;

        // Super
        // Single
        sw.SuperSinglePast = (int)NUD_SSinglePast.Number;
        sw.SuperSingleRecord = (int)NUD_SSingleRecord.Number;

        // Double
        sw.SuperDoublePast = (int)NUD_SDoublePast.Number;
        sw.SuperDoubleRecord = (int)NUD_SDoubleRecord.Number;

        // Multi NPC
        sw.SuperMultiNPCPast = (int)NUD_SMultiNpcPast.Number;
        sw.SuperMultiNPCRecord = (int)NUD_SMultiNpcRecord.Number;

        // Multi Friends
        sw.SuperMultiFriendsPast = (int)NUD_SMultiFriendsPast.Number;
        sw.SuperMultiFriendsRecord = (int)NUD_SMultiFriendsRecord.Number;

        // Current Run Checks
        sw.SingleSet = (CHK_SingleSet.IsChecked ? (sw.SinglePast / 7) + 1 : 0);
        sw.DoubleSet = (CHK_DoubleSet.IsChecked ? (sw.DoublePast / 7) + 1 : 0);
        sw.MultiNPCSet = (CHK_MultiNPCSet.IsChecked ? (sw.MultiNPCPast / 7) + 1 : 0);
        sw.MultiFriendsSet = (CHK_MultiFriendsSet.IsChecked ? (sw.MultiFriendsPast / 7) + 1 : 0);
        sw.SuperSingleSet = (CHK_SuperSingleSet.IsChecked ? (sw.SuperSinglePast / 7) + 1 : 0);
        sw.SuperDoubleSet = (CHK_SuperDoubleSet.IsChecked ? (sw.SuperDoublePast / 7) + 1 : 0);
        sw.SuperMultiNPCSet = (CHK_SuperMultiNPCSet.IsChecked ? (sw.SuperMultiNPCPast / 7) + 1 : 0);
        sw.SuperMultiFriendsSet = (CHK_SuperMultiFriendsSet.IsChecked ? (sw.SuperMultiFriendsPast / 7) + 1 : 0);
    }

    private void CHK_SingleSet_CheckedChanged(object sender, EventArgs e)
    {
        L_SinglePast.Text = CHK_SingleSet.IsChecked ? "Current" : "Past";
    }

    private void CHK_DoubleSet_CheckedChanged(object sender, EventArgs e)
    {
        L_DoublePast.Text = CHK_DoubleSet.IsChecked ? "Current" : "Past";
    }

    private void CHK_MultiNPCSet_CheckedChanged(object sender, EventArgs e)
    {
        L_MultiNpcPast.Text = CHK_MultiNPCSet.IsChecked ? "Current" : "Past";
    }

    private void CHK_MultiFriendsSet_CheckedChanged(object sender, EventArgs e)
    {
        L_MultiFriendsPast.Text = CHK_MultiFriendsSet.IsChecked ? "Current" : "Past";
    }

    private void CHK_SuperSingleSet_CheckedChanged(object sender, EventArgs e)
    {
        L_SSinglePast.Text = CHK_SuperSingleSet.IsChecked ? "Current" : "Past";
    }

    private void CHK_SuperDoubleSet_CheckedChanged(object sender, EventArgs e)
    {
        L_SDoublePast.Text = CHK_SuperDoubleSet.IsChecked ? "Current" : "Past";
    }

    private void CHK_SuperMultiNPCSet_CheckedChanged(object sender, EventArgs e)
    {
        L_SMultiNpcPast.Text = CHK_SuperMultiNPCSet.IsChecked ? "Current" : "Past";
    }

    private void CHK_SuperMultiFriendsSet_CheckedChanged(object sender, EventArgs e)
    {
        L_SMultiFriendsPast.Text = CHK_SuperMultiFriendsSet.IsChecked ? "Current" : "Past";
    }
}