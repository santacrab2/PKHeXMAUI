using PKHeX.Core;
using static PKHeXMAUI.MainPage;

namespace PKHeXMAUI;

public partial class Cosmeticstab : ContentPage
{
	public Cosmeticstab()
	{
		InitializeComponent();
        if (pk.Species != 0)
            applycomsetics(pk);
	}

	public void applycomsetics(PKM pkm)
	{
        if (pkm.HeldItem > 0)
        {
            itemsprite.Source = $"aitem_{pkm.HeldItem}.png";
            itemsprite.IsVisible = true;
        }
        else
            itemsprite.IsVisible = false;
        if (pkm.IsShiny)
            shinysparklessprite.IsVisible = true;
        else
            shinysparklessprite.IsVisible = false;
        pic.Source = spriteurl;
        if (pkm is IScaledSize ssz)
        {
            Heightdisplay.Text = $"{ssz.HeightScalar}";
            Weightdisplay.Text = $"{ssz.WeightScalar}";
        }
        if (pkm is IScaledSize3 ssz3)
        {
            scaledisplay.IsVisible = true;
            scaledisplay.Text = $"{ssz3.Scale}";
        }
    }
    private void applyheight(object sender, TextChangedEventArgs e)
    {
        if (Heightdisplay.Text.Length > 0)
        {
            if (!byte.TryParse(Heightdisplay.Text, out var result))
                return;
            if (result > 255)
            {
                result = 255;
                Heightdisplay.Text = $"{result}";
            }
            if (pk is IScaledSize sz)
            {

                sz.HeightScalar = result;
               
            }
        }
    }

    private void applyweight(object sender, TextChangedEventArgs e)
    {
        if (Weightdisplay.Text.Length > 0)
        {
            if (!byte.TryParse(Weightdisplay.Text, out var result))
                return;
            if (result > 255)
            {
                result = 255;
                Weightdisplay.Text = $"{result}";
            }
            if (pk is IScaledSize sz)
            {
                sz.WeightScalar = result;

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
}