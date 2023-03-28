using PKHeX.Core;
using static PKHeXMAUI.MainPage;

namespace PKHeXMAUI;

public partial class Cosmeticstab : ContentPage
{
	public Cosmeticstab()
	{
		InitializeComponent();
        applycomsetics(pk);
	}
    private static readonly string[] SizeClass = Enum.GetNames(typeof(PokeSize));
    private static readonly string[] SizeClassDetailed = Enum.GetNames(typeof(PokeSizeDetailed));

    public void applycomsetics(PKM pkm)
	{
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
            scaledisplay.Text = cp.Stat_CP.ToString();
        }
        if(pkm is PK4 pk4)
        {
            LeafCheckBox1.IsVisible = true;
            LeafSprite1.IsVisible = true;
            LeafCheckBox1.IsChecked = pk4.ShinyLeaf > 0;
            LeafCheckBox2.IsVisible = true;
            LeafSprite2.IsVisible = true;
            LeafCheckBox2.IsChecked = pk4.ShinyLeaf > 1;
            LeafCheckBox3.IsVisible = true;
            LeafSprite3.IsVisible = true;
            LeafCheckBox3.IsChecked = pk4.ShinyLeaf > 2;
            LeafCheckBox4.IsVisible = true;
            LeafSprite4.IsVisible = true;
            LeafCheckBox4.IsChecked = pk4.ShinyLeaf > 3;
            LeafCheckBox5.IsVisible = true;
            LeafSprite5.IsVisible = true;
            LeafCheckBox5.IsChecked = pk4.ShinyLeaf > 4;
            LeafCheckBox6.IsVisible = true;
            LeafSprite6.IsVisible = true;
            LeafCheckBox6.IsChecked = pk4.ShinyLeaf > 5;
            CrownCheckbox.IsVisible = true;
            CrownSprite.IsVisible = true;
            CrownCheckbox.IsChecked = pk4.ShinyLeaf== 6;

        }
    }
    private void applyheight(object sender, TextChangedEventArgs e)
    {
        if (Heightdisplay.Text.Length > 0)
        {
            if (!int.TryParse(Heightdisplay.Text, out var result))
                return;
            if (result > 255)
            {
                result = 255;
                Heightdisplay.Text = $"{result}";
            }
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

    private void applyweight(object sender, TextChangedEventArgs e)
    {
        if (Weightdisplay.Text.Length > 0)
        {
            if (!int.TryParse(Weightdisplay.Text, out var result))
                return;
            if (result > 255)
            {
                result = 255;
                Weightdisplay.Text = $"{result}";
            }
            if (pk is IScaledSize ss)
            {
                ss.WeightScalar = (byte)result;
                if(pk is IScaledSizeValue sv)
                    sv.ResetWeight();
                HeighDetailLabel.Text = SizeClass[(int)PokeSizeUtil.GetSizeRating(ss.WeightScalar)];
            }
        }
    }

    private void applyscale(object sender, TextChangedEventArgs e)
    {
        if (!byte.TryParse(scaledisplay.Text, out var result))
            return;
        if (result > 255)
        {
            result = 255;
            scaledisplay.Text = $"{result}";
        }
        if (pk is IScaledSize3 sz3)
        {
            sz3.Scale = result;
            ScaleDetailLabel.Text= SizeClassDetailed[(int)PokeSizeDetailedUtil.GetSizeRating(sz3.Scale)];
            if (pk is ICombatPower cp)
                cp.ResetCP();
        }
    }
    private void openribbons(object sender, EventArgs e)
    {
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
        if(pk is IContestStats CTstats)
        {
            if (!byte.TryParse(Coolstats.Text,out var result))
                return;
            if (result > 255) 
            { 
                result = 255;
                Coolstats.Text = result.ToString();
            }
            CTstats.CNT_Cool = result;
            
        }
    }

    private void makepretty(object sender, TextChangedEventArgs e)
    {
        if (pk is IContestStats CTstats)
        {
            if (!byte.TryParse(Beautystats.Text, out var result))
                return;
            if (result > 255)
            {
                result = 255;
                Beautystats.Text = result.ToString();
            }
            CTstats.CNT_Beauty = result;

        }
    }

    private void applycuteness(object sender, TextChangedEventArgs e)
    {
        if (pk is IContestStats CTstats)
        {
            if (!byte.TryParse(Cutestats.Text, out var result))
                return;
            if (result > 255)
            {
                result = 255;
                Cutestats.Text = result.ToString();
            }
            CTstats.CNT_Cute = result;

        }
    }

    private void makesmart(object sender, TextChangedEventArgs e)
    {
        if (pk is IContestStats CTstats)
        {
            if (!byte.TryParse(Cleverstats.Text, out var result))
                return;
            if (result > 255)
            {
                result = 255;
                Cleverstats.Text = result.ToString();
            }
            CTstats.CNT_Smart = result;

        }
    }

    private void GoToTheGym(object sender, TextChangedEventArgs e)
    {
        if (pk is IContestStats CTstats)
        {
            if (!byte.TryParse(toughstats.Text, out var result))
                return;
            if (result > 255)
            {
                result = 255;
                toughstats.Text = result.ToString();
            }
            CTstats.CNT_Tough = result;

        }
    }

    private void MakeMeSparkle(object sender, TextChangedEventArgs e)
    {
        if (pk is IContestStats CTstats)
        {
            if (!byte.TryParse(sheenstats.Text, out var result))
                return;
            if (result > 255)
            {
                result = 255;
                sheenstats.Text = result.ToString();
            }
            CTstats.CNT_Sheen = result;

        }
    }

    private async void openmedaleditor(object sender, EventArgs e)
    {
        await Navigation.PushModalAsync(new MedalEditor());
    }

    private void ApplyLeaf1(object sender, CheckedChangedEventArgs e)
    {

    }
    private void ApplyLeaf2(object sender, CheckedChangedEventArgs e)
    {

    }
    private void ApplyLeaf3(object sender, CheckedChangedEventArgs e)
    {

    }
    private void ApplyLeaf4(object sender, CheckedChangedEventArgs e)
    {

    }
    private void ApplyLeaf5(object sender, CheckedChangedEventArgs e)
    {

    }
    private void ApplyLeaf6(object sender, CheckedChangedEventArgs e)
    {

    }
    private void ApplyCrown(object sender, CheckedChangedEventArgs e)
    {

    }

}