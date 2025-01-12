
using PKHeX.Core;

namespace PKHeXMAUI;

public partial class TrainerEditor7BattleTree : ContentPage
{
	public SAV7 SAV = (SAV7)MainPage.sav;
	public TrainerEditor7BattleTree()
	{
		InitializeComponent();
		var bt = SAV.BattleTree;
		CSSNEntry.Number = bt.GetTreeStreak(0, false, false);
		CSSSEntry.Number = bt.GetTreeStreak(0, true, false);
		MSSNEntry.Number = bt.GetTreeStreak(0, false, true);
		MSSSEntry.Number = bt.GetTreeStreak(0, true, true);
		CSDNEntry.Number = bt.GetTreeStreak(1,false, false);
		CSDSEntry.Number = bt.GetTreeStreak(1, true, false);
		MSDNEntry.Number = bt.GetTreeStreak(1, false, true);
		MSDSEntry.Number = bt.GetTreeStreak(1, true, true);
		CSMNEntry.Number = bt.GetTreeStreak(2, false, false);
		CSMSEntry.Number = bt.GetTreeStreak(2, true, false);
		MSMNEntry.Number = bt.GetTreeStreak(2, false, true);
		MSMSEntry.Number = bt.GetTreeStreak(2, true, true);
		SuperSingleCheck.IsChecked = SAV.EventWork.GetEventFlag(333);
		SuperDoubleCheck.IsChecked = SAV.EventWork.GetEventFlag(334);
		SuperMultiCheck.IsChecked = SAV.EventWork.GetEventFlag(335);
	}

	public void SaveTE7B()
	{
        var bt = SAV.BattleTree;
		bt.SetTreeStreak((int)CSSNEntry.Number, 0, false, false);
		bt.SetTreeStreak((int)CSSSEntry.Number, 0, true, false);
		bt.SetTreeStreak((int)MSSNEntry.Number, 0, false, true);
		bt.SetTreeStreak((int)MSSSEntry.Number, 0, true, true);
		bt.SetTreeStreak((int)CSDNEntry.Number, 1, false, false);
		bt.SetTreeStreak((int)CSDSEntry.Number, 1, true, false);
		bt.SetTreeStreak((int)MSDNEntry.Number, 1, false, true);
		bt.SetTreeStreak((int)MSDSEntry.Number, 1, true, true);
		bt.SetTreeStreak((int)CSMNEntry.Number, 2, false, false);
		bt.SetTreeStreak((int)CSMSEntry.Number, 2, true, false);
		bt.SetTreeStreak((int)MSMNEntry.Number, 2, false, true);
		bt.SetTreeStreak((int)MSMSEntry.Number, 2, true, true);
		SAV.EventWork.SetEventFlag(333, SuperSingleCheck.IsChecked);
		SAV.EventWork.SetEventFlag(334, SuperDoubleCheck.IsChecked);
		SAV.EventWork.SetEventFlag(335, SuperMultiCheck.IsChecked);
    }
}