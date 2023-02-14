using System.Windows.Input;
using PKHeX.Core;
using static PKHeXMAUI.MainPage;

namespace PKHeXMAUI;

public partial class StatsTab : ContentPage
{
    public bool moveonce = true;
	public StatsTab()
	{
		InitializeComponent();
        Teratypepicker.ItemsSource = Enum.GetValues(typeof(MoveType));
        MainTeratypepicker.ItemsSource = Enum.GetValues(typeof(MoveType));
        if(pk.Species !=0)
            applystatsinfo(pk);
    }

	public void applystatsinfo(PKM pkm)
	{
        if (pkm.HeldItem > 0)
        {
            itemsprite.Source = itemspriteurl;
            itemsprite.IsVisible = true;
        }
        else
            itemsprite.IsVisible = false;
        pkm.ResetPartyStats();
        if (pkm.IsShiny)
            shinysparklessprite.IsVisible = true;
        else
            shinysparklessprite.IsVisible = false;
        statpic.Source = spriteurl;
        hpbasedisplay.Text = pkm.PersonalInfo.HP.ToString();
        HPIV.Text = pkm.IV_HP.ToString();
        HPEV.Text = pkm.EV_HP.ToString();
        totalhpdisplay.Text = pkm.Stat_HPCurrent.ToString();
        if(pkm is IHyperTrain hpt)
        {
            hpHyper.IsChecked = hpt.IsHyperTrained(0);
            ATKHyper.IsChecked = hpt.IsHyperTrained(1);
            DEFHyper.IsChecked = hpt.IsHyperTrained(2);
            SPAHyper.IsChecked = hpt.IsHyperTrained(3);
            SPDHyper.IsChecked = hpt.IsHyperTrained(4);
            SPEHyper.IsChecked = hpt.IsHyperTrained(5);
        }
        
        atkbasedisplay.Text = pkm.PersonalInfo.ATK.ToString();
        AtkIV.Text = pkm.IV_ATK.ToString();
        AtkEV.Text = pkm.EV_ATK.ToString();
        totalatkdisplay.Text = pkm.Stat_ATK.ToString();
        
        defbasedisplay.Text = pkm.PersonalInfo.DEF.ToString();
        DEFIV.Text = pkm.IV_DEF.ToString();
        DEFEV.Text = pkm.EV_DEF.ToString();
        totaldefdisplay.Text = pkm.Stat_DEF.ToString();
      
        spabasedisplay.Text = pkm.PersonalInfo.SPA.ToString();
        SPAIV.Text = pkm.IV_SPA.ToString();
        SPAEV.Text = pkm.EV_SPA.ToString();
        totalspadisplay.Text = pkm.Stat_SPA.ToString();
       
        spdbasedisplay.Text = pkm.PersonalInfo.SPD.ToString();
        SPDIV.Text = pkm.IV_SPD.ToString();
        SPDEV.Text = pkm.EV_SPD.ToString();
        totalspddisplay.Text = pkm.Stat_SPD.ToString();
        
        spebasedisplay.Text = pkm.PersonalInfo.SPE.ToString();
        SPEIV.Text = pkm.IV_SPE.ToString();
        SPEEV.Text = pkm.EV_SPE.ToString();
        totalspedisplay.Text = pkm.Stat_SPE.ToString();
      
        totalbasedisplay.Text = pkm.PersonalInfo.GetBaseStatTotal().ToString();
        totalIVdisplay.Text = pkm.IVTotal.ToString();
        totalEVdisplay.Text = pkm.EVTotal.ToString();
        if (pkm is ITeraType tera)
        {
            OvTeralabel.IsVisible = true;
            OrTeralabel.IsVisible = true;
            Teratypepicker.IsVisible = true;
            MainTeratypepicker.IsVisible = true;
            if ((int)tera.TeraTypeOverride != 0x13)
            {
                Teratypepicker.SelectedIndex = (int)tera.TeraTypeOverride;
                if (moveonce)
                {
                    teratypeimage.TranslateTo(teratypeimage.TranslationX, teratypeimage.TranslationY - 50);
                    moveonce = false;
                }
            }
            MainTeratypepicker.SelectedIndex = (int)tera.TeraTypeOriginal;
            teratypeimage.IsVisible = true;
            teratypeimage.Source = $"gem_{(int)tera.TeraType:00}";
        }
        if(pkm is IGanbaru gb)
        {
            gvlabel.IsVisible = true;
            HPGV.IsVisible = true;
           
            HPGV.Text = gb.GV_HP.ToString();
            AtkGV.IsVisible = true;
            AtkGV.Text = gb.GV_ATK.ToString();
            DEFGV.IsVisible = true;
            DEFGV.Text = gb.GV_DEF.ToString();
            SPAGV.IsVisible = true;
            SPAGV.Text = gb.GV_SPA.ToString();
            SPDGV.IsVisible = true;
            SPDGV.Text = gb.GV_SPD.ToString();
            SPEGV.IsVisible = true;
            SPEGV.Text = gb.GV_SPE.ToString();
        }

    }

    private void applyhpIV(object sender, TextChangedEventArgs e)
    {
        if (HPIV.Text.Length > 0)
        {
            if (int.TryParse(HPIV.Text, out var result))
            {
                if (result > 31)
                    result = 31;
                pk.IV_HP = result;
                totalhpdisplay.Text = pk.Stat_HPCurrent.ToString();
                totalIVdisplay.Text = pk.IVTotal.ToString();
                totalEVdisplay.Text = pk.EVTotal.ToString();
            }
        }
    }

    private void applyhpEV(object sender, TextChangedEventArgs e)
    {
        if (HPEV.Text.Length > 0)
        {

            if (int.Parse(HPEV.Text) > 252)
                HPEV.Text = "252";
            pk.EV_HP = int.Parse(HPEV.Text);
            totalhpdisplay.Text = pk.Stat_HPCurrent.ToString();
            totalIVdisplay.Text = pk.IVTotal.ToString();
            totalEVdisplay.Text = pk.EVTotal.ToString();
        }
    }

    private void applyatkIV(object sender, TextChangedEventArgs e)
    {
        if (AtkIV.Text.Length > 0)
        {
          
            if (int.Parse(AtkIV.Text) > 31)
                AtkIV.Text = "31";
            pk.IV_ATK = int.Parse(AtkIV.Text);
            totalatkdisplay.Text = pk.Stat_ATK.ToString();
            totalIVdisplay.Text = pk.IVTotal.ToString();
            totalEVdisplay.Text = pk.EVTotal.ToString();
        }

    }

    private void applyatkEV(object sender, TextChangedEventArgs e)
    {
        if (AtkEV.Text.Length > 0)
        {

            if (int.Parse(AtkEV.Text) > 252)
                AtkEV.Text = "252";
            pk.EV_ATK = int.Parse(AtkEV.Text);
            totalatkdisplay.Text = pk.Stat_ATK.ToString();
            totalIVdisplay.Text = pk.IVTotal.ToString();
            totalEVdisplay.Text = pk.EVTotal.ToString();
        }
    }

    private void applydefIV(object sender, TextChangedEventArgs e)
    {
        if (DEFIV.Text.Length > 0)
        {
            if (int.Parse(DEFIV.Text) > 31)
                DEFIV.Text = "31";
            pk.IV_DEF = int.Parse(DEFIV.Text);
            totaldefdisplay.Text = pk.Stat_DEF.ToString();
            totalIVdisplay.Text = pk.IVTotal.ToString();
            totalEVdisplay.Text = pk.EVTotal.ToString();
        }
    }

    private void applydefEV(object sender, TextChangedEventArgs e)
    {
        if (DEFEV.Text.Length > 0)
        {
            if (int.Parse(DEFEV.Text) > 252)
                DEFEV.Text = "252";
            pk.EV_DEF = int.Parse(DEFEV.Text);
            totaldefdisplay.Text = pk.Stat_DEF.ToString();
            totalIVdisplay.Text = pk.IVTotal.ToString();
            totalEVdisplay.Text = pk.EVTotal.ToString();
        }
    }

    private void applyspaIV(object sender, TextChangedEventArgs e)
    {
        if (SPAIV.Text.Length > 0)
        {
            if (int.Parse(SPAIV.Text) > 31)
                SPAIV.Text = "31";
            pk.IV_SPA = int.Parse(SPAIV.Text);
            totalspadisplay.Text = pk.Stat_SPA.ToString();
            totalIVdisplay.Text = pk.IVTotal.ToString();
            totalEVdisplay.Text = pk.EVTotal.ToString();
        }
    }

    private void applyspaEV(object sender, TextChangedEventArgs e)
    {
        if (SPAEV.Text.Length > 0)
        {

            if (int.Parse(SPAEV.Text) > 252)
                SPAEV.Text = "252";
            pk.EV_SPA = int.Parse(SPAEV.Text);
            totalspadisplay.Text = pk.Stat_SPA.ToString();
            totalIVdisplay.Text = pk.IVTotal.ToString();
            totalEVdisplay.Text = pk.EVTotal.ToString();
        }
    }

    private void applyspdIV(object sender, TextChangedEventArgs e)
    {
        if (SPDIV.Text.Length > 0)
        {
            if (int.Parse(SPDIV.Text) > 31)
                SPDIV.Text = "31";
            pk.IV_SPD = int.Parse(SPDIV.Text);
            totalspddisplay.Text = pk.Stat_SPD.ToString();
            totalIVdisplay.Text = pk.IVTotal.ToString();
            totalEVdisplay.Text = pk.EVTotal.ToString();
        }
    }

    private void applyspdEV(object sender, TextChangedEventArgs e)
    {
        if (SPDEV.Text.Length > 0)
        {
            if (int.Parse(SPDEV.Text) > 252)
                SPDEV.Text = "252";
            pk.EV_SPD = int.Parse(SPDEV.Text);
            totalspddisplay.Text = pk.Stat_SPD.ToString();
            totalIVdisplay.Text = pk.IVTotal.ToString();
            totalEVdisplay.Text = pk.EVTotal.ToString();
        }
    }

    private void applyspeIV(object sender, TextChangedEventArgs e)
    {
        if (SPEIV.Text.Length > 0)
        {
            if (int.Parse(SPEIV.Text) > 31)
                SPEIV.Text = "31";
            pk.IV_SPE = int.Parse(SPEIV.Text);
            totalspedisplay.Text = pk.Stat_SPE.ToString();
            totalIVdisplay.Text = pk.IVTotal.ToString();
            totalEVdisplay.Text = pk.EVTotal.ToString();
        }

    }

    private void applyspeEV(object sender, TextChangedEventArgs e)
    {
        if (SPEEV.Text.Length > 0)
        {
            
            if (int.Parse(SPEEV.Text) > 252)
                SPEEV.Text = "252";
            pk.EV_SPE = int.Parse(SPEEV.Text);
            totalspedisplay.Text = pk.Stat_SPE.ToString();
            totalIVdisplay.Text = pk.IVTotal.ToString();
            totalEVdisplay.Text = pk.EVTotal.ToString();
        }
    }

    private void randomizeivs(object sender, EventArgs e)
    {
        
        pk.SetRandomIVs();
    }

    private void perfectivs(object sender, EventArgs e)
    {
        Span<int> ivs = stackalloc int[6];
        ivs.Fill(pk.MaxIV);
        pk.IVs = ivs.ToArray();
    }

    private void randomizeevs(object sender, EventArgs e)
    {
        Span<int> ivs = stackalloc int[6];
       
        EffortValues.SetRandom(ivs, 9);
        pk.SetEVs(ivs);
    }

    private void suggestedevs(object sender, EventArgs e)
    {
        Span<int> ivs = stackalloc int[6];
        
        EffortValues.SetMax(ivs,pk);
        pk.SetEVs(ivs);
    }

    private void applyHPhyper(object sender, CheckedChangedEventArgs e)
    {
        if(pk is IHyperTrain hpt)
            hpt.HyperTrainInvert(0);
    }
    private void applyATKhyper(object sender, CheckedChangedEventArgs e)
    {
        if(pk is IHyperTrain hpt)
            hpt.HyperTrainInvert(1);
    }
    private void applyDEFhyper(object sender, CheckedChangedEventArgs e)
    {
        if (pk is IHyperTrain hpt)
            hpt.HyperTrainInvert(2);
    }
    private void applySPAhyper(object sender, CheckedChangedEventArgs e)
    {
        if (pk is IHyperTrain hpt)
            hpt.HyperTrainInvert(3);
    }
    private void applySPDhyper(object sender, CheckedChangedEventArgs e)
    {
        if (pk is IHyperTrain hpt)
            hpt.HyperTrainInvert(4);
    }
    private void applySPEhyper(object sender, CheckedChangedEventArgs e)
    {
        if (pk is IHyperTrain hpt)
            hpt.HyperTrainInvert(5);
    }
    private void applytera(object sender, EventArgs e) 
    {
        if (pk is ITeraType pk9)
        {
            if (Teratypepicker.SelectedIndex == 18)
            {
                pk9.TeraTypeOverride = (MoveType)0x13;
                teratypeimage.TranslateTo(teratypeimage.TranslationX, teratypeimage.TranslationY + 50);
                moveonce = true;
            }
            else
            {
                pk9.TeraTypeOverride = (MoveType)Teratypepicker.SelectedIndex;
                if (moveonce)
                {
                    teratypeimage.TranslateTo(teratypeimage.TranslationX, teratypeimage.TranslationY - 50);
                    moveonce = false;
                }
            }
            teratypeimage.Source = $"gem_{(int)pk9.TeraType:00}";
        }
    }

    private void applymaintera(object sender, EventArgs e) 
    {
        if (pk is ITeraType pk9)
        {
            pk9.TeraTypeOriginal = (MoveType)MainTeratypepicker.SelectedIndex;
            teratypeimage.Source = $"gem_{(int)pk9.TeraType:00}";
        }
    }

    private void refreshstats(object sender, EventArgs e)
    {
        if (pk.Species != 0)
            applystatsinfo(pk);
    }

    private void applyhpGV(object sender, TextChangedEventArgs e)
    {
        if(HPGV.Text.Length > 0)
        {
            if(byte.TryParse(HPGV.Text,out var result))
            {
                if (result > 10)
                    result = 10;
                if (pk is IGanbaru gb)
                    gb.GV_HP= result;
                totalhpdisplay.Text = pk.Stat_HPCurrent.ToString();
            }
        }
    }

    private void applyatkGV(object sender, TextChangedEventArgs e)
    {
        if (AtkGV.Text.Length > 0)
        {
            if (byte.TryParse(AtkGV.Text, out var result))
            {
                if (result > 10)
                    result = 10;
                if (pk is IGanbaru gb)
                    gb.GV_ATK = result;
                totalhpdisplay.Text = pk.Stat_ATK.ToString();
            }
        }
    }

    private void applydefGV(object sender, TextChangedEventArgs e)
    {
        if (DEFGV.Text.Length > 0)
        {
            if (byte.TryParse(DEFGV.Text, out var result))
            {
                if (result > 10)
                    result = 10;
                if (pk is IGanbaru gb)
                    gb.GV_DEF = result;
                totalhpdisplay.Text = pk.Stat_DEF.ToString();
            }
        }
    }

    private void applyspaGV(object sender, TextChangedEventArgs e)
    {
        if (SPAGV.Text.Length > 0)
        {
            if (byte.TryParse(SPAGV.Text, out var result))
            {
                if (result > 10)
                    result = 10;
                if (pk is IGanbaru gb)
                    gb.GV_SPA = result;
                totalhpdisplay.Text = pk.Stat_SPA.ToString();
            }
        }
    }

    private void applyspdGV(object sender, TextChangedEventArgs e)
    {
        if (SPDGV.Text.Length > 0)
        {
            if (byte.TryParse(SPDGV.Text, out var result))
            {
                if (result > 10)
                    result = 10;
                if (pk is IGanbaru gb)
                    gb.GV_SPD = result;
                totalhpdisplay.Text = pk.Stat_SPD.ToString();
            }
        }
    }

    private void applyspeGV(object sender, TextChangedEventArgs e)
    {
        if (SPEGV.Text.Length > 0)
        {
            if (byte.TryParse(SPEGV.Text, out var result))
            {
                if (result > 10)
                    result = 10;
                if (pk is IGanbaru gb)
                    gb.GV_SPE = result;
                totalhpdisplay.Text = pk.Stat_SPE.ToString();
            }
        }
    }
}