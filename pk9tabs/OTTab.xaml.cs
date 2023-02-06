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
        htlanguagepicker.ItemsSource = Enum.GetValues(typeof(LanguageID));
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
        TIDdisplay.Text = pkm.TrainerTID7.ToString();
        otdisplay.Text = pkm.OT_Name;
        ecdisplay.Text = $"{pkm.EncryptionConstant:X}";
        htname.Text = pkm.HT_Name;
        htlanguagepicker.SelectedIndex = pkm.HT_Language;
        switch(pkm.CurrentHandler)
        {
            case 0:  OTcurrentcheck.IsChecked = true; HTcurrentcheck.IsChecked = false; break;
            case 1: HTcurrentcheck.IsChecked = true; OTcurrentcheck.IsChecked = false; break;
        };

    }

    private void applySID(object sender, TextChangedEventArgs e)
    {
        if(SIDdisplay.Text.Length > 0)
        {
            pk.TrainerSID7 = uint.Parse(SIDdisplay.Text);
        }
    }

    private void applyTID(object sender, TextChangedEventArgs e)
    {
        if(TIDdisplay.Text.Length > 0)
        {
            pk.TrainerTID7 = uint.Parse(TIDdisplay.Text);
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

    private void applyHT(object sender, TextChangedEventArgs e)
    {
        pk.HT_Name = htname.Text;
    }

    private void applyhtlanguage(object sender, EventArgs e)
    {
        pk.HT_Language = (byte)htlanguagepicker.SelectedIndex;
    }

    private void MakeOTCurrent(object sender, CheckedChangedEventArgs e)
    {
        pk.CurrentHandler = 0;
    }

    private void MakeHTCurrent(object sender, CheckedChangedEventArgs e)
    {
        pk.CurrentHandler = 1;
    }

    private void applyotgender(object sender, EventArgs e)
    {
        pk.OT_Gender = (byte)otgenderpicker.SelectedIndex;
    }

    private void openribbons(object sender, EventArgs e)
    {
        Navigation.PushModalAsync(new RibbonSelector());
    }
}