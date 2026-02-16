using PKHeX.Core;

namespace PKHeXMAUI;

public partial class MiscMusical : ContentPage
{
	public SAV5 SAV;
    public string[] PropNames = Util.GetStringList("props","en");
    public MiscMusical(SAV5 sav)
	{
		InitializeComponent();
		SAV = sav;
        ReadMusical();
    }
    private void ReadMusical()
    {
        CB_Prop.ItemSource = PropNames;
        CB_Prop.SelectedIndex = 0;
    }
    private void CB_Prop_SelectedIndexChanged(object sender, EventArgs e)
    {
        CHK_PropObtained.IsChecked = SAV.Musical.GetHasProp(CB_Prop.SelectedIndex);
    }
    private void CHK_PropObtained_CheckedChanged(object sender, EventArgs e)
    {
        SAV.Musical.SetHasProp(CB_Prop.SelectedIndex, CHK_PropObtained.IsChecked);
    }
    private void B_UnlockAllProps_Click(object sender, EventArgs e)
    {
        SAV.Musical.UnlockAllMusicalProps();
        B_UnlockAllProps.IsEnabled = false;
    }
}