
using PKHeX.Core;

using System.Windows.Input;
using static PKHeX.Core.Encounters9;


using static pk9reader.MainPage;

namespace pk9reader;

public partial class teleporter : ContentPage
{
	public teleporter()
	{
		InitializeComponent();
	}
    public static byte[] recentcoords;
    private async void readloc(object sender, EventArgs e)
    {
        var telporterpointer = new long[] { 0x43A75B0, 0x2A8, 0x0, 0x0, 0x80 };
        var teleporteroff = await botBase.PointerRelative(telporterpointer);
        var co = await botBase.ReadBytesAsync((uint)teleporteroff, 12);
        recentcoords = co;
        xlab.Text = $"{BitConverter.ToString( co,0,4)}";
        ylab.Text = $"{BitConverter.ToString(co, 4,4)}";
        zlab.Text = $"{BitConverter.ToString(co, 8,4)}";
    }


}