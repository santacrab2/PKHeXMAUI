
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
        ICommand refreshCommand = new Command(() =>
        {
            if (recentcoords.Length != 0)
            {
                xlab.Text = $"{BitConverter.ToString(recentcoords, 0, 4)}";
                ylab.Text = $"{BitConverter.ToString(recentcoords, 4, 4)}";
                zlab.Text = $"{BitConverter.ToString(recentcoords, 8, 4)}";
            }
            teleportrefresh.IsRefreshing = false;

        });
        teleportrefresh.Command = refreshCommand;
    }
    public static byte[] recentcoords = Array.Empty<byte>();
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

    private async void teleloc(object sender, EventArgs e)
    {

        var telporterpointer = new long[] { 0x43A75B0, 0x2A8, 0x0, 0x0, 0x80 };
        var teleporteroff = await botBase.PointerRelative(telporterpointer);
        var x = BitConverter.ToUInt32(recentcoords, 0);
        var xbit = BitConverter.GetBytes(x);
        var y = BitConverter.ToUInt32(recentcoords, 4) + 30;
        var ybit = BitConverter.GetBytes(y);
        var z = BitConverter.ToUInt32(recentcoords, 8);
        var zbit = BitConverter.GetBytes(z);
        var xyz = xbit.Concat(ybit).ToArray();
        xyz = xyz.Concat(zbit).ToArray();
        await botBase.WriteBytesAsync(xyz, (uint)teleporteroff);
    }
}