
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
        var telporterpointer = new long[] { 0x43A7828, 0x140, 0xE8, 0x48, 0x160, 0x30, 0x260 };
        var teleporteroff = await botBase.PointerRelative(telporterpointer);
        var co = await botBase.ReadBytesAsync((uint)teleporteroff, 12);
        recentcoords = co;
        xlab.Text = $"{BitConverter.ToString( co,0,4)}";
        ylab.Text = $"{BitConverter.ToString(co, 4,4)}";
        zlab.Text = $"{BitConverter.ToString(co, 8,4)}";
    }

    private async void telep(object sender, EventArgs e)
    {
        var telporterpointer = new long[] { 0x43A7828, 0x140, 0xE8, 0x48, 0x160, 0x30, 0x260 };
        var teleporteroff = await botBase.PointerRelative(telporterpointer);
        await botBase.WriteBytesAsync(recentcoords, (uint)teleporteroff - 0x10);
        await botBase.WriteBytesAsync(recentcoords, (uint)teleporteroff);
    
    }
}