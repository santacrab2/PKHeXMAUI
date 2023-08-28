using PKHeX.Core;
using System.Windows.Input;
using static PKHeXMAUI.MainPage;

namespace PKHeXMAUI;

public partial class Cosmeticstab : ContentPage
{
    public bool SkipEvent = false;
    public bool FirstLoad = true;
    public static byte minStat = 0;
    public static byte maxCosmStat = 255;
	public Cosmeticstab()
	{
		InitializeComponent();
        Flags = new[]{ LeafCheckBox1, LeafCheckBox2, LeafCheckBox3, LeafCheckBox4, LeafCheckBox5, CrownCheckbox };
        applycomsetics(pk);
        ICommand refreshCommand = new Command(async () =>
        {

            await applycomsetics(pk);
            CosmeticsRefresh.IsRefreshing = false;
        });
        CosmeticsRefresh.Command = refreshCommand;
        FirstLoad = false;
    }
    private static readonly string[] SizeClass = Enum.GetNames(typeof(PokeSize));
    private static readonly string[] SizeClassDetailed = Enum.GetNames(typeof(PokeSizeDetailed));
    private readonly CheckBox[] Flags;
    public async Task applycomsetics(PKM pkm)
	{
        SkipEvent = true;
        eggsprite.IsVisible = pkm.IsEgg;
        MedalEditorButton.IsVisible = pkm is ISuperTrain;
        memoriesbutton.IsVisible = pkm is ITrainerMemories;
        SizeMarkImage.IsVisible = false;
        if (pkm.HeldItem > 0)
        {
            itemsprite.Source = itemspriteurl;
            itemsprite.IsVisible = true;
        }
        else
            itemsprite.IsVisible = false;
        if (pkm.IsShiny)
            shinysparklessprite.IsVisible = true;
        else
            shinysparklessprite.IsVisible = false;
        if (pkm.Species == 0)
            spriteurl = $"a_egg.png";
        else
            spriteurl = $"a_{pkm.Species}{((pkm.Form > 0 && !MainPage.NoFormSpriteSpecies.Contains(pkm.Species)) ? $"_{pkm.Form}" : "")}.png";
        pic.Source = spriteurl;
        if (pkm is IScaledSize ss)
        {
            HeightLabel.IsVisible = true;
            Heightdisplay.IsVisible = true;
            Heightdisplay.Text = $"{ss.HeightScalar}";
            if (pkm is IScaledSizeValue sv)
            {
                HeightAbsoluteEditor.IsVisible = true;
                HeightAbsoluteEditor.Text = sv.HeightAbsolute.ToString();
            }
            HeighDetailLabel.IsVisible = true;
            HeighDetailLabel.Text = SizeClass[(int)PokeSizeUtil.GetSizeRating(ss.HeightScalar)];
            WeightLabel.IsVisible = true;
            Weightdisplay.IsVisible = true;
            Weightdisplay.Text = $"{ss.WeightScalar}";
            if (pkm is IScaledSizeValue sv2)
            {
                WeightAbsoluteEditor.IsVisible = true;
                WeightAbsoluteEditor.Text = sv2.WeightAbsolute.ToString();
            }
            WeightDetailLabel.IsVisible = true;
            WeightDetailLabel.Text = SizeClass[(int)PokeSizeUtil.GetSizeRating(ss.WeightScalar)];
        }
        if (pkm is IScaledSize3 scale)
        {
            scaledisplay.IsVisible = true;
            scalelabel.IsVisible = true;
            scaledisplay.Text = $"{scale.Scale}";
            ScaleDetailLabel.IsVisible = true;
            ScaleDetailLabel.Text = SizeClassDetailed[(int)PokeSizeDetailedUtil.GetSizeRating(scale.Scale)];
            if (scale.Scale == 0)
            {
                SizeMarkImage.IsVisible = true;
                SizeMarkImage.Source = "ribbonmarkmini.png";
            }
            if(scale.Scale == 255)
            {
                SizeMarkImage.IsVisible = true;
                SizeMarkImage.Source = "ribbonmarkjumbo.png";
            }
        }
        else if(pkm is ICombatPower cp)
        {
            scalelabel.IsVisible = true;
            scaledisplay.IsVisible = true;
            scalelabel.Text = "CP:";
            cpAutoLabel.IsVisible = true;
            AutoCP.IsVisible = true;
            if (AutoCP.IsChecked)
                cp.ResetCP();
            scaledisplay.Text = cp.Stat_CP.ToString();
        }
        if(pkm is PK4 pk4)
        {
            LeafCheckBox1.IsVisible = true;
            LeafSprite1.IsVisible = true;
            
            LeafCheckBox2.IsVisible = true;
            LeafSprite2.IsVisible = true;
            
            LeafCheckBox3.IsVisible = true;
            LeafSprite3.IsVisible = true;
         
            LeafCheckBox4.IsVisible = true;
            LeafSprite4.IsVisible = true;
          
            LeafCheckBox5.IsVisible = true;
            LeafSprite5.IsVisible = true;
            
            CrownCheckbox.IsVisible = true;
            CrownSprite.IsVisible = true;
            for (int i = 0; i < Flags.Length; i++)
                Flags[i].IsChecked = ((pk4.ShinyLeaf >> i) & 1) == 1;

        }
        if(pkm is IRibbonSetAffixed a)
        {
            AffixedRibbonSprite.IsVisible = true;
            if (a.AffixedRibbon != -1)
                AffixedRibbonSprite.Source = $"ribbon{((RibbonIndex)a.AffixedRibbon).ToString().ToLower()}.png";
            else
                AffixedRibbonSprite.Source = "ribbon_affix_none.png";
        }
        if(pkm is IContestStats contest)
        {
            Coolstats.Text = $"{contest.CNT_Cool}";
            Beautystats.Text = $"{contest.CNT_Beauty}";
            Cutestats.Text = $"{contest.CNT_Cute}";
            Cleverstats.Text = $"{contest.CNT_Smart}";
            toughstats.Text = $"{contest.CNT_Tough}";
            sheenstats.Text = $"{contest.CNT_Sheen}";
        }
        SkipEvent = false;
    }
    private void applyheight(object sender, TextChangedEventArgs e)
    {
        if (Heightdisplay.Text.Length > 0 && !SkipEvent)
        {
            if (int.TryParse(Heightdisplay.Text, out var result))
            {


                result = Math.Clamp(result, 0, 255);
                Heightdisplay.Text = $"{result}";
                
                if (pk is IScaledSize ss)
                {
                    ss.HeightScalar = (byte)result;
                    if (pk is IScaledSizeValue sv)
                    {
                        sv.ResetHeight();
                        sv.ResetWeight();
                    }
                    HeighDetailLabel.Text = SizeClass[(int)PokeSizeUtil.GetSizeRating(ss.HeightScalar)];
                }
            }
        }
    }

    private void applyweight(object sender, TextChangedEventArgs e)
    {
        if (Weightdisplay.Text.Length > 0 && !SkipEvent)
        {
            if (int.TryParse(Weightdisplay.Text, out var result))
            {


                result = Math.Clamp(result, 0, 255);
                Weightdisplay.Text = $"{result}";
                
                if (pk is IScaledSize ss)
                {
                    ss.WeightScalar = (byte)result;
                    if (pk is IScaledSizeValue sv)
                        sv.ResetWeight();
                    HeighDetailLabel.Text = SizeClass[(int)PokeSizeUtil.GetSizeRating(ss.WeightScalar)];
                }
            }
        }
    }

    private void applyscale(object sender, TextChangedEventArgs e)
    {
        if (!SkipEvent)
        {
            if (int.TryParse(scaledisplay.Text, out var result))
            {

                if (pk is IScaledSize3 sz3)
                {
                    result = Math.Clamp(result, 0, 255);
                    scaledisplay.Text = $"{result}";
                    sz3.Scale = (byte)result;
                    ScaleDetailLabel.Text = SizeClassDetailed[(int)PokeSizeDetailedUtil.GetSizeRating(sz3.Scale)];

                }
                if(pk is ICombatPower cp)
                {
                    result = Math.Clamp(result, 0, 65535);
                    cp.Stat_CP = result;
                    scaledisplay.Text = $"{result}";
                }
            }
                
        }
    }
    private void openribbons(object sender, EventArgs e)
    {
        RibbonSelector.ApplicatorMode = false;
        Navigation.PushModalAsync(new RibbonSelector());
    }

    private void refreshcosmetics(object sender, EventArgs e)
    {
        applycomsetics(pk);
    }

    private void openmemories(object sender, EventArgs e)
    {
        Navigation.PushModalAsync(new MemoriesAmie());
    }

    private void applycoolness(object sender, TextChangedEventArgs e)
    {
        if (pk is IContestStats CTstats && !SkipEvent)
        {
            if (byte.TryParse(Coolstats.Text, out var result))
            {
                result = Math.Clamp(result, minStat, maxCosmStat);
                Coolstats.Text = result.ToString();
                
                CTstats.CNT_Cool = result;

            }
        }
    }

    private void makepretty(object sender, TextChangedEventArgs e)
    {
        if (pk is IContestStats CTstats && !SkipEvent)
        {
            if (byte.TryParse(Beautystats.Text, out var result))
            {
                result = Math.Clamp(result, minStat, maxCosmStat);
                    Beautystats.Text = result.ToString();
                
                CTstats.CNT_Beauty = result;

            }
        }
    }

    private void applycuteness(object sender, TextChangedEventArgs e)
    {
        if (pk is IContestStats CTstats && !SkipEvent)
        {
            if (!byte.TryParse(Cutestats.Text, out var result))
            {
                result = Math.Clamp(result, minStat, maxCosmStat);
                    Cutestats.Text = result.ToString();
                
                CTstats.CNT_Cute = result;

            }
        }
    }

    private void makesmart(object sender, TextChangedEventArgs e)
    {
        if (pk is IContestStats CTstats && !SkipEvent)
        {
            if (!byte.TryParse(Cleverstats.Text, out var result))
            {
                result = Math.Clamp(result, minStat, maxCosmStat);
                Cleverstats.Text = result.ToString();
                
                CTstats.CNT_Smart = result;

            }
        }
    }

    private void GoToTheGym(object sender, TextChangedEventArgs e)
    {
        if (pk is IContestStats CTstats && !SkipEvent)
        {
            if (!byte.TryParse(toughstats.Text, out var result))
            {
                result = Math.Clamp(result, minStat, maxCosmStat);
                    toughstats.Text = result.ToString();
                
                CTstats.CNT_Tough = result;

            }
        }
    }

    private void MakeMeSparkle(object sender, TextChangedEventArgs e)
    {
        if (pk is IContestStats CTstats && !SkipEvent)
        {
            if (byte.TryParse(sheenstats.Text, out var result))
            {
                result = Math.Clamp(result, minStat, maxCosmStat);
                    sheenstats.Text = result.ToString();
                
                CTstats.CNT_Sheen = result;

            }
        }
    }

    private async void openmedaleditor(object sender, EventArgs e)
    {
        await Navigation.PushModalAsync(new MedalEditor());
    }

    private void ApplyLeaf1(object sender, CheckedChangedEventArgs e)
    {
        if(!SkipEvent)
        {
            if(pk is PK4 pk4)
            {
                int value = 0;
                for (int i = 0; i < Flags.Length; i++)
                {
                    if (Flags[i].IsChecked)
                        value |= 1 << i;
                }
                pk4.ShinyLeaf = value;
            }
        }
    }

    private void OpenRibbonEditor(object sender, EventArgs e)
    {
        RibbonSelector.ApplicatorMode = true;
        Navigation.PushModalAsync(new RibbonSelector());
    }

    private void AutoCalcCP(object sender, CheckedChangedEventArgs e)
    {
        if (pk is ICombatPower cp && AutoCP.IsChecked)
        {
            cp.ResetCP();
            scaledisplay.Text = cp.Stat_CP.ToString();
        }
    }
}