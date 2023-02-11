using PKHeX.Core;
using static pk9reader.MainPage;

namespace pk9reader;

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
            if (!int.TryParse(Heightdisplay.Text, out var result))
                return;
            if (result > 255)
            {
                result = 255;
                Heightdisplay.Text = $"{result}";
            }
            if (pk is IScaledSize sz)
            {

                sz.HeightScalar = (byte)result;
               
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
            if (pk is IScaledSize sz)
            {
                sz.WeightScalar = (byte)result;

            }
        }
    }

    private void applyscale(object sender, TextChangedEventArgs e)
    {
        if (!int.TryParse(scaledisplay.Text, out var result))
            return;
        if (result > 255)
        {
            result = 255;
            scaledisplay.Text = $"{result}";
        }
        if (pk is IScaledSize3 sz3)
        {
            sz3.Scale = (byte)result;
         
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
}