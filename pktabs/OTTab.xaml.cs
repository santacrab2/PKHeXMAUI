using System.Windows.Input;
using System;
using PKHeX.Core;
using static pk9reader.MainPage;
namespace pk9reader;

public partial class OTTab : ContentPage
{
	public OTTab()
	{
		InitializeComponent();
        ICommand refreshCommand = new Command(() =>
        {
            if (pk.Species != 0)
                applyotinfo(pk);
            otrefresh.IsRefreshing = false;
        });
        otrefresh.Command = refreshCommand;
        if (pk.Species != 0)
            applyotinfo(pk);
    }

	public void applyotinfo(PK9 pkm)
	{
        OTpic.Source = spriteurl;
        SIDdisplay.Text = pkm.TrainerSID7.ToString();
        TIDdisplay.Text = pkm.TrainerID7.ToString();
        otdisplay.Text = pkm.OT_Name;
        ecdisplay.Text = $"{pkm.EncryptionConstant:X}";


    }

    private void applySID(object sender, TextChangedEventArgs e)
    {
        if(SIDdisplay.Text.Length > 0)
        {
            pk.TrainerSID7 = int.Parse(SIDdisplay.Text);
        }
    }

    private void applyTID(object sender, TextChangedEventArgs e)
    {
        if(TIDdisplay.Text.Length > 0)
        {
            pk.TrainerID7 = int.Parse(TIDdisplay.Text);
        }
    }

    private void applyot(object sender, TextChangedEventArgs e)
    {
        pk.OT_Name = otdisplay.Text;
    }

    private void applyec(object sender, TextChangedEventArgs e)
    {
        pk.EncryptionConstant = Convert.ToUInt32(ecdisplay.Text,16);
    }
}