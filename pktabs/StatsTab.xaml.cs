using System.Windows.Input;
using PKHeX.Core;
using static PKHeXMAUI.MainPage;

namespace PKHeXMAUI;

public partial class StatsTab : ContentPage
{
    public bool SkipEvent = false;
    public bool moveonce = true;
    public bool FirstLoad = true;
    public int minStat = 0;
    public int maxIV = 31;
    public int maxEV = 252;
    public int maxAV = 200;
    public int maxGV = 10;
	public StatsTab()
	{
		InitializeComponent();
        Teratypepicker.ItemsSource = Enum.GetValues(typeof(MoveType));
        MainTeratypepicker.ItemsSource = Enum.GetValues(typeof(MoveType));
        HiddenPowerPicker.ItemsSource = Enum.GetValues(typeof(MoveType));
        ICommand refreshCommand = new Command(async () =>
        {

            await applystatsinfo(pk);
            StatsRefresh.IsRefreshing = false;
        });
        StatsRefresh.Command = refreshCommand;
        applystatsinfo(pk);
        FirstLoad = false;
    }

	public async Task applystatsinfo(PKM pkm)
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
        if (pkm.Species == 0)
            spriteurl = $"a_egg.png";
        else
            spriteurl = $"a_{pkm.Species}{((pkm.Form > 0 && !MainPage.NoFormSpriteSpecies.Contains(pkm.Species)) ? $"_{pkm.Form}" : "")}.png";
        statpic.Source = spriteurl;
        hpbasedisplay.Text = pkm.PersonalInfo.HP.ToString();
        HPIV.Text = pkm.IV_HP.ToString();
        atkbasedisplay.Text = pkm.PersonalInfo.ATK.ToString();
        AtkIV.Text = pkm.IV_ATK.ToString();
        defbasedisplay.Text = pkm.PersonalInfo.DEF.ToString();
        DEFIV.Text = pkm.IV_DEF.ToString();
        spabasedisplay.Text = pkm.PersonalInfo.SPA.ToString();
        SPAIV.Text = pkm.IV_SPA.ToString();
        spdbasedisplay.Text = pkm.PersonalInfo.SPD.ToString();
        SPDIV.Text = pkm.IV_SPD.ToString();
        spebasedisplay.Text = pkm.PersonalInfo.SPE.ToString();
        SPEIV.Text = pkm.IV_SPE.ToString();
        totalbasedisplay.Text = pkm.PersonalInfo.GetBaseStatTotal().ToString();
        totalIVdisplay.Text = pkm.IVTotal.ToString();
        if (pk is IAwakened woke)
        {
            EvLabel.Text = "AVs";
            randomEv.Text = "Random AVs";
            suggestedEv.Text = "Suggested AVs";
            HPEV.Text = woke.AV_HP.ToString();
            AtkEV.Text = woke.AV_ATK.ToString();
            DEFEV.Text = woke.AV_DEF.ToString();
            SPAEV.Text = woke.AV_SPA.ToString();
            SPDEV.Text = woke.AV_SPD.ToString();
            SPEEV.Text = woke.AV_SPE.ToString();
            totalEVdisplay.Text = woke.AwakeningSum().ToString();
        }
        else
        {
            HPEV.Text = pkm.EV_HP.ToString();
            AtkEV.Text = pkm.EV_ATK.ToString();
            DEFEV.Text = pkm.EV_DEF.ToString();
            SPAEV.Text = pkm.EV_SPA.ToString();
            SPDEV.Text = pkm.EV_SPD.ToString();
            SPEEV.Text = pkm.EV_SPE.ToString();
            totalEVdisplay.Text = pkm.EVTotal.ToString();
        }
        totalhpdisplay.Text = pkm.Stat_HPCurrent.ToString();
        totalatkdisplay.Text = pkm.Stat_ATK.ToString();
        totaldefdisplay.Text = pkm.Stat_DEF.ToString();
        totalspadisplay.Text = pkm.Stat_SPA.ToString();
        totalspddisplay.Text = pkm.Stat_SPD.ToString();
        totalspedisplay.Text = pkm.Stat_SPE.ToString();
        if (pkm is IHyperTrain hpt)
        {
            hpHyper.IsChecked = hpt.HT_HP;
            ATKHyper.IsChecked = hpt.HT_ATK;
            DEFHyper.IsChecked = hpt.HT_DEF;
            SPAHyper.IsChecked = hpt.HT_SPA;
            SPDHyper.IsChecked = hpt.HT_SPD;
            SPEHyper.IsChecked = hpt.HT_SPE;
        }
  
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

                pk.IV_HP = Math.Clamp(result,minStat,maxIV);
                pk.ResetPartyStats();
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
                    woke.AV_HP = Math.Clamp(result,(byte)minStat,(byte)maxAV);
                    pk.ResetPartyStats();
                    totalhpdisplay.Text = pk.Stat_HPCurrent.ToString();
                    totalIVdisplay.Text = pk.IVTotal.ToString();
                    totalEVdisplay.Text = woke.AwakeningSum().ToString();
                }
                else
                {
                    
                    pk.EV_HP = Math.Clamp(result, (byte)minStat, (byte)maxEV);
                    pk.ResetPartyStats();
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

            if (byte.TryParse(AtkIV.Text, out var result))
            {

                pk.IV_ATK = Math.Clamp(result, (byte)minStat, (byte)maxIV);
                pk.ResetPartyStats();
                totalatkdisplay.Text = pk.Stat_ATK.ToString();
                totalIVdisplay.Text = pk.IVTotal.ToString();
                totalEVdisplay.Text = pk.EVTotal.ToString();
            }
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
                    woke.AV_ATK = Math.Clamp(result, (byte)minStat, (byte)maxAV);
                    pk.ResetPartyStats();
                    totalatkdisplay.Text = pk.Stat_ATK.ToString();
                    totalIVdisplay.Text = pk.IVTotal.ToString();
                    totalEVdisplay.Text = woke.AwakeningSum().ToString();
                }
                else
                {
                   
                    pk.EV_ATK = Math.Clamp(result, (byte)minStat, (byte)maxEV);
                    pk.ResetPartyStats();
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
            if (int.TryParse(DEFIV.Text, out var result))
            {

                pk.IV_DEF = Math.Clamp(result, (byte)minStat, (byte)maxIV);
                pk.ResetPartyStats();
                totaldefdisplay.Text = pk.Stat_DEF.ToString();
                totalIVdisplay.Text = pk.IVTotal.ToString();
                totalEVdisplay.Text = pk.EVTotal.ToString();
            }
        }
    }

    private void applydefEV(object sender, TextChangedEventArgs e)
    {
        if (DEFEV.Text.Length > 0 && !SkipEvent)
        {
            if (byte.TryParse(DEFEV.Text, out byte result))
            {
                if (pk is IAwakened woke)
                {
                    woke.AV_DEF = Math.Clamp(result, (byte)minStat, (byte)maxAV);
                    pk.ResetPartyStats();
                    totaldefdisplay.Text = pk.Stat_DEF.ToString();
                    totalIVdisplay.Text = pk.IVTotal.ToString();
                    totalEVdisplay.Text = woke.AwakeningSum().ToString();
                }
                else
                {
                    pk.EV_DEF = Math.Clamp(result, (byte)minStat, (byte)maxEV);
                    pk.ResetPartyStats();
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
            if (int.TryParse(SPAIV.Text, out var result))
            {
                pk.IV_SPA = Math.Clamp(result, (byte)minStat, (byte)maxIV);
                pk.ResetPartyStats();
                totalspadisplay.Text = pk.Stat_SPA.ToString();
                totalIVdisplay.Text = pk.IVTotal.ToString();
                totalEVdisplay.Text = pk.EVTotal.ToString();
            }
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
                    woke.AV_SPA = Math.Clamp(result, (byte)minStat, (byte)maxAV);
                    pk.ResetPartyStats();
                    totalspadisplay.Text = pk.Stat_SPA.ToString();
                    totalIVdisplay.Text = pk.IVTotal.ToString();
                    totalEVdisplay.Text = woke.AwakeningSum().ToString();
                }
                else
                {
                    pk.EV_SPA = Math.Clamp(result, (byte)minStat, (byte)maxEV);
                    pk.ResetPartyStats();
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
            if (int.TryParse(SPDIV.Text, out var result))
            {
                pk.IV_SPD = Math.Clamp(result, (byte)minStat, (byte)maxIV);
                pk.ResetPartyStats();
                totalspddisplay.Text = pk.Stat_SPD.ToString();
                totalIVdisplay.Text = pk.IVTotal.ToString();
                totalEVdisplay.Text = pk.EVTotal.ToString();
            }
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
                    woke.AV_SPD = Math.Clamp(result, (byte)minStat, (byte)maxAV);
                    pk.ResetPartyStats();
                    totalspddisplay.Text = pk.Stat_SPD.ToString();
                    totalIVdisplay.Text = pk.IVTotal.ToString();
                    totalEVdisplay.Text = woke.AwakeningSum().ToString();
                }
                else
                {
                    pk.EV_SPD = Math.Clamp(result, (byte)minStat, (byte)maxEV);
                    pk.ResetPartyStats();
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
            if (int.TryParse(SPEIV.Text, out var result))
            {
                pk.IV_SPE = Math.Clamp(result, (byte)minStat, (byte)maxIV);
                pk.ResetPartyStats();
                totalspedisplay.Text = pk.Stat_SPE.ToString();
                totalIVdisplay.Text = pk.IVTotal.ToString();
                totalEVdisplay.Text = pk.EVTotal.ToString();
            }
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
                    woke.AV_SPE = Math.Clamp(result, (byte)minStat, (byte)maxAV);
                    pk.ResetPartyStats();
                    totalspedisplay.Text = pk.Stat_SPE.ToString();
                    totalIVdisplay.Text = pk.IVTotal.ToString();
                    totalEVdisplay.Text = woke.AwakeningSum().ToString();
                }
                else
                {
                    pk.EV_SPE = Math.Clamp(result, (byte)minStat, (byte)maxEV);
                    pk.ResetPartyStats();
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
        if (pk is IHyperTrain hpt && !SkipEvent)
            hpt.HT_HP = e.Value;
    }
    private void applyATKhyper(object sender, CheckedChangedEventArgs e)
    {
        if(pk is IHyperTrain hpt && !SkipEvent)
            hpt.HT_ATK = e.Value;
    }
    private void applyDEFhyper(object sender, CheckedChangedEventArgs e)
    {
        if (pk is IHyperTrain hpt && !SkipEvent)
            hpt.HT_DEF = e.Value;
    }
    private void applySPAhyper(object sender, CheckedChangedEventArgs e)
    {
        if (pk is IHyperTrain hpt && !SkipEvent)
            hpt.HT_SPA = e.Value;
    }
    private void applySPDhyper(object sender, CheckedChangedEventArgs e)
    {
        if (pk is IHyperTrain hpt && !SkipEvent)
            hpt.HT_SPD = e.Value;
    }
    private void applySPEhyper(object sender, CheckedChangedEventArgs e)
    {
        if (pk is IHyperTrain hpt && !SkipEvent)
            hpt.HT_SPE = e.Value;
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
                if (pk is IGanbaru gb)
                    gb.GV_HP= Math.Clamp(result, (byte)minStat, (byte)maxGV);
                pk.ResetPartyStats();
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
                if (pk is IGanbaru gb)
                    gb.GV_ATK = Math.Clamp(result, (byte)minStat, (byte)maxGV);
                pk.ResetPartyStats();
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
                if (pk is IGanbaru gb)
                    gb.GV_DEF = Math.Clamp(result, (byte)minStat, (byte)maxGV);
                pk.ResetPartyStats();
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
                if (pk is IGanbaru gb)
                    gb.GV_SPA = Math.Clamp(result, (byte)minStat, (byte)maxGV);
                pk.ResetPartyStats();
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
                if (pk is IGanbaru gb)
                    gb.GV_SPD = Math.Clamp(result, (byte)minStat, (byte)maxGV);
                pk.ResetPartyStats();
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
                if (pk is IGanbaru gb)
                    gb.GV_SPE = Math.Clamp(result, (byte)minStat, (byte)maxGV);
                pk.ResetPartyStats();
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
                if (pk is IDynamaxLevel dmax)
                    dmax.DynamaxLevel = Math.Clamp(result, (byte)minStat, (byte)maxGV);
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