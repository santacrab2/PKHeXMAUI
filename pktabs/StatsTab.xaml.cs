using System.Windows.Input;
using PKHeX.Core;
using static PKHeXMAUI.MainPage;

namespace PKHeXMAUI;

public partial class StatsTab : ContentPage
{
    public bool SkipEvent = false;
    public bool moveonce = true;
	public StatsTab()
	{
		InitializeComponent();
        Teratypepicker.ItemsSource = Enum.GetValues(typeof(MoveType));
        MainTeratypepicker.ItemsSource = Enum.GetValues(typeof(MoveType));
        HiddenPowerPicker.ItemsSource = Enum.GetValues(typeof(MoveType));
            applystatsinfo(pk);
    }

	public void applystatsinfo(PKM pkm)
	{
        SkipEvent = true;
        eggsprite.IsVisible = pkm.IsEgg;
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
        if (pk is IAwakened)
        {
            EvLabel.Text = "AVs";
            randomEv.Text = "Random AVs";
            suggestedEv.Text = "Suggested AVs";
        }
        hpbasedisplay.Text = pkm.PersonalInfo.HP.ToString();
        HPIV.Text = pkm.IV_HP.ToString();
        if (pkm is IAwakened woke)
            HPEV.Text = woke.AV_HP.ToString();
        else
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
        if (pkm is IAwakened wokeattack)
            AtkEV.Text = wokeattack.AV_ATK.ToString();
        else
            AtkEV.Text = pkm.EV_ATK.ToString();
        totalatkdisplay.Text = pkm.Stat_ATK.ToString();
        
        defbasedisplay.Text = pkm.PersonalInfo.DEF.ToString();
        DEFIV.Text = pkm.IV_DEF.ToString();
        if (pkm is IAwakened wokeDEF)
            DEFEV.Text = wokeDEF.AV_DEF.ToString();
        else
            DEFEV.Text = pkm.EV_DEF.ToString();
        totaldefdisplay.Text = pkm.Stat_DEF.ToString();
      
        spabasedisplay.Text = pkm.PersonalInfo.SPA.ToString();
        SPAIV.Text = pkm.IV_SPA.ToString();
        if (pkm is IAwakened wokeSPA)
           SPAEV.Text = wokeSPA.AV_SPA.ToString();
        else
            SPAEV.Text = pkm.EV_SPA.ToString();
        totalspadisplay.Text = pkm.Stat_SPA.ToString();
       
        spdbasedisplay.Text = pkm.PersonalInfo.SPD.ToString();
        SPDIV.Text = pkm.IV_SPD.ToString();
        if (pkm is IAwakened wokeSPD)
            SPDEV.Text = wokeSPD.AV_SPD.ToString();
        else
            SPDEV.Text = pkm.EV_SPD.ToString();
        totalspddisplay.Text = pkm.Stat_SPD.ToString();
        
        spebasedisplay.Text = pkm.PersonalInfo.SPE.ToString();
        SPEIV.Text = pkm.IV_SPE.ToString();
        if (pkm is IAwakened wokeSPE)
            SPEEV.Text = wokeSPE.AV_SPE.ToString();
        else
            SPEEV.Text = pkm.EV_SPE.ToString();
        totalspedisplay.Text = pkm.Stat_SPE.ToString();
      
        totalbasedisplay.Text = pkm.PersonalInfo.GetBaseStatTotal().ToString();
        totalIVdisplay.Text = pkm.IVTotal.ToString();
        if (pk is IAwakened awake)
            totalEVdisplay.Text = awake.AwakeningSum().ToString();
        else
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
        if(pkm is IDynamaxLevel dmax)
        {
            dmaxlabel.IsVisible = true;
            dmaxleveleditor.IsVisible = true;
            dmaxleveleditor.Text = dmax.DynamaxLevel.ToString();
        }
        if(pkm is IGigantamax gmax)
        {
            gmaxlabel.IsVisible = true;
            GmaxCheck.IsVisible = true;
            GmaxCheck.IsChecked = gmax.CanGigantamax;
        }
        if(pkm is IAlpha alpha)
        {
            alphalabel.IsVisible = true;
            Alphacheck.IsVisible = true;
            Alphacheck.IsChecked = alpha.IsAlpha;
        }
        if(pkm is INoble noble)
        {
            noblelabel.IsVisible = true;
            Noblecheck.IsVisible = true;
            Noblecheck.IsChecked = noble.IsNoble;
        }
        if((byte)pkm.Context <= 7 || pkm is PB8 || pkm is PB7)
        {
            HiddenPowerPicker.IsVisible = true;
            HiddenPLabel.IsVisible = true;
            HiddenPowerPicker.SelectedItem = (MoveType)pkm.HPType;
        }
        SkipEvent = false;
    }
   
    private void applyhpIV(object sender, TextChangedEventArgs e)
    {
        if (HPIV.Text.Length > 0 && !SkipEvent)
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
        if (HPEV.Text.Length > 0 && !SkipEvent)
        {

            if (byte.TryParse(HPEV.Text, out var result))
            {
                if (pk is IAwakened woke)
                {
                    if (result > 200)
                        result = 200;
                    woke.AV_HP = result;
                    totalhpdisplay.Text = pk.Stat_HPCurrent.ToString();
                    totalIVdisplay.Text = pk.IVTotal.ToString();
                    totalEVdisplay.Text = woke.AwakeningSum().ToString();
                }
                else
                {
                    if (result > 252)
                        result = 252;
                    pk.EV_HP = result;
                    totalhpdisplay.Text = pk.Stat_HPCurrent.ToString();
                    totalIVdisplay.Text = pk.IVTotal.ToString();
                    totalEVdisplay.Text = pk.EVTotal.ToString();
                }
            }
        }
    }

    private void applyatkIV(object sender, TextChangedEventArgs e)
    {
        if (AtkIV.Text.Length > 0 && !SkipEvent)
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
        if (AtkEV.Text.Length > 0 && !SkipEvent)
        {
            if (byte.TryParse(AtkEV.Text, out var result))
            {
                if (pk is IAwakened woke)
                {
                    if (result > 200)
                        result = 200;
                    woke.AV_ATK = result;
                    totalatkdisplay.Text = pk.Stat_ATK.ToString();
                    totalIVdisplay.Text = pk.IVTotal.ToString();
                    totalEVdisplay.Text = woke.AwakeningSum().ToString();
                }
                else
                {
                    if (result > 252)
                        result = 252;
                    pk.EV_ATK = result;
                    totalatkdisplay.Text = pk.Stat_ATK.ToString();
                    totalIVdisplay.Text = pk.IVTotal.ToString();
                    totalEVdisplay.Text = pk.EVTotal.ToString();
                }
            }
        }
    }

    private void applydefIV(object sender, TextChangedEventArgs e)
    {
        if (DEFIV.Text.Length > 0 && !SkipEvent)
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
        if (DEFEV.Text.Length > 0 && !SkipEvent)
        {
            if (byte.TryParse(DEFEV.Text, out byte value))
            {
                if (pk is IAwakened woke)
                {
                    if (value > 200)
                        value = 200;
                    woke.AV_DEF = value;
                    totaldefdisplay.Text = pk.Stat_DEF.ToString();
                    totalIVdisplay.Text = pk.IVTotal.ToString();
                    totalEVdisplay.Text = woke.AwakeningSum().ToString();
                }
                else
                {
                    if (value > 252)
                        value = 252;
                    pk.EV_DEF = value;
                    totaldefdisplay.Text = pk.Stat_DEF.ToString();
                    totalIVdisplay.Text = pk.IVTotal.ToString();
                    totalEVdisplay.Text = pk.EVTotal.ToString();
                }
            }
        }
    }

    private void applyspaIV(object sender, TextChangedEventArgs e)
    {
        if (SPAIV.Text.Length > 0 && !SkipEvent)
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
        if (SPAEV.Text.Length > 0 && !SkipEvent)
        {

            if (byte.TryParse(SPAEV.Text, out var result))
            {
                if (pk is IAwakened woke)
                {
                    if (result > 200)
                        result = 200;
                    woke.AV_SPA = result;
                    totalspadisplay.Text = pk.Stat_SPA.ToString();
                    totalIVdisplay.Text = pk.IVTotal.ToString();
                    totalEVdisplay.Text = woke.AwakeningSum().ToString();
                }
                else
                {
                    if (result > 252)
                        result = 252;
                    pk.EV_SPA = result;
                    totalspadisplay.Text = pk.Stat_SPA.ToString();
                    totalIVdisplay.Text = pk.IVTotal.ToString();
                    totalEVdisplay.Text = pk.EVTotal.ToString();
                }
            }
        }
    }

    private void applyspdIV(object sender, TextChangedEventArgs e)
    {
        if (SPDIV.Text.Length > 0 && !SkipEvent)
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
        if (SPDEV.Text.Length > 0 && !SkipEvent)
        {
            if (byte.TryParse(SPDEV.Text, out var result))
            {
                if (pk is IAwakened woke)
                {
                    if (result > 200)
                        result = 200;
                    woke.AV_SPD = result;
                    totalspddisplay.Text = pk.Stat_SPD.ToString();
                    totalIVdisplay.Text = pk.IVTotal.ToString();
                    totalEVdisplay.Text = woke.AwakeningSum().ToString();
                }
                else
                {
                    if (result > 252)
                        result = 252;
                    pk.EV_SPD = result;
                    totalspddisplay.Text = pk.Stat_SPD.ToString();
                    totalIVdisplay.Text = pk.IVTotal.ToString();
                    totalEVdisplay.Text = pk.EVTotal.ToString();
                }
            }
        }
    }

    private void applyspeIV(object sender, TextChangedEventArgs e)
    {
        if (SPEIV.Text.Length > 0 && !SkipEvent)
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
        if (SPEEV.Text.Length > 0 && !SkipEvent)
        {

            if (byte.TryParse(SPEEV.Text, out var result))
            {
                if (pk is IAwakened woke)
                {
                    if (result > 200)
                        result = 200;
                    woke.AV_SPE = result;
                    totalspedisplay.Text = pk.Stat_SPE.ToString();
                    totalIVdisplay.Text = pk.IVTotal.ToString();
                    totalEVdisplay.Text = woke.AwakeningSum().ToString();
                }
                else
                {
                    if (result > 252)
                        result = 252;
                    pk.EV_SPE = result;
                    totalspedisplay.Text = pk.Stat_SPE.ToString();
                    totalIVdisplay.Text = pk.IVTotal.ToString();
                    totalEVdisplay.Text = pk.EVTotal.ToString();
                }
            }
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
        if (pk is IAwakened woke)
        {
           woke.AwakeningSetRandom();
        }
        else
        {
            Span<int> ivs = stackalloc int[6];

            EffortValues.SetRandom(ivs, 9);
            pk.SetEVs(ivs);
        }
    }

    private void suggestedevs(object sender, EventArgs e)
    {
        if (pk is IAwakened woke)
        {
            woke.SetSuggestedAwakenedValues(pk);
        }
        else
        {
            Span<int> ivs = stackalloc int[6];

            EffortValues.SetMax(ivs, pk);
            pk.SetEVs(ivs);
        }
    }

    private void applyHPhyper(object sender, CheckedChangedEventArgs e)
    {
        if(pk is IHyperTrain hpt && !SkipEvent)
            hpt.HyperTrainInvert(0);
    }
    private void applyATKhyper(object sender, CheckedChangedEventArgs e)
    {
        if(pk is IHyperTrain hpt && !SkipEvent)
            hpt.HyperTrainInvert(1);
    }
    private void applyDEFhyper(object sender, CheckedChangedEventArgs e)
    {
        if (pk is IHyperTrain hpt && !SkipEvent)
            hpt.HyperTrainInvert(2);
    }
    private void applySPAhyper(object sender, CheckedChangedEventArgs e)
    {
        if (pk is IHyperTrain hpt && !SkipEvent)
            hpt.HyperTrainInvert(3);
    }
    private void applySPDhyper(object sender, CheckedChangedEventArgs e)
    {
        if (pk is IHyperTrain hpt && !SkipEvent)
            hpt.HyperTrainInvert(4);
    }
    private void applySPEhyper(object sender, CheckedChangedEventArgs e)
    {
        if (pk is IHyperTrain hpt && !SkipEvent)
            hpt.HyperTrainInvert(5);
    }
    private void applytera(object sender, EventArgs e) 
    {
        if (pk is ITeraType pk9 && !SkipEvent)
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
        if (pk is ITeraType pk9 && !SkipEvent)
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
        if(HPGV.Text.Length > 0 && !SkipEvent)
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
        if (AtkGV.Text.Length > 0 && !SkipEvent)
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
        if (DEFGV.Text.Length > 0 && !SkipEvent)
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
        if (SPAGV.Text.Length > 0 && !SkipEvent)
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
        if (SPDGV.Text.Length > 0 && !SkipEvent)
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
        if (SPEGV.Text.Length > 0 && !SkipEvent)
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

    private void applydmaxlevel(object sender, TextChangedEventArgs e)
    {
        if (dmaxleveleditor.Text.Length > 0 && !SkipEvent)
        {
            if(byte.TryParse(dmaxleveleditor.Text,out var result))
            {
                if (result > 10)
                    result = 10;
                if (pk is IDynamaxLevel dmax)
                    dmax.DynamaxLevel = result;
            }
        }
    }

    private void applygmax(object sender, CheckedChangedEventArgs e)
    {
        if(pk is IGigantamax gmax && !SkipEvent)
        {
            if (gmax.CanToggleGigantamax(pk.Species, pk.Form))
                gmax.CanGigantamax = GmaxCheck.IsChecked;
            else
                GmaxCheck.IsChecked = false;
        }
    }

    private void applyalhpastatus(object sender, CheckedChangedEventArgs e)
    {
        if(pk is IAlpha alpha && !SkipEvent)
        {
            alpha.IsAlpha = Alphacheck.IsChecked;
        }
    }

    private void applynoblestatus(object sender, CheckedChangedEventArgs e)
    {
        if (pk is INoble noble && !SkipEvent)
            noble.IsNoble = Noblecheck.IsChecked;
    }

    private void applyHiddenPower(object sender, EventArgs e)
    {
        if (!SkipEvent)
        {
            pk.HPType = HiddenPowerPicker.SelectedIndex;
            HiddenPower.SetIVs(HiddenPowerPicker.SelectedIndex, pk.IVs, pk.Context);
        }
    }
}