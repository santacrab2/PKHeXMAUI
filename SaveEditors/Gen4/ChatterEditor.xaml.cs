using Plugin.Maui.Audio;
using CommunityToolkit.Maui.Storage;
using PKHeX.Core;
using static PKHeXMAUI.MainPage;
namespace PKHeXMAUI;

public partial class ChatterEditor : ContentPage
{
    private readonly SaveFile Origin;
    private readonly SaveFile SAV;
    private readonly IChatter Chatter;
    public ChatterEditor()
	{
		InitializeComponent();
        SAV = (Origin = sav).Clone();
        Chatter = SAV is SAV5 s5 ? s5.Chatter : ((SAV4)SAV).Chatter;

        CHK_Initialized.IsChecked = Chatter.Initialized;
        Entry_Confusion.Text = Chatter.ConfusionChance.ToString();
    }

    private void B_PlayRecording_Click(object sender, EventArgs e)
    {
        if (!Chatter.Initialized && !Chatter.Recording.ContainsAnyExcept<byte>(0x00))
            return;
        var data = ConvertPCMToWAV(Chatter.Recording);
        MemoryStream test = new(data);
        var player = AudioManager.Current.CreatePlayer(test);
        player.Volume = 1;
        player.Play();
        
    }

    private void CHK_Initialized_CheckChanged(object sender, CheckedChangedEventArgs e)
    {
        Chatter.Initialized = CHK_Initialized.IsChecked;
        Entry_Confusion.Text = Chatter.ConfusionChance.ToString();
    }

    private async void B_Exportwav_clicked(object sender, EventArgs e)
    {
        await using var CrossedStreams = new MemoryStream(ConvertPCMToWAV(Chatter.Recording));
        var result = await FileSaver.SaveAsync("recording.wav", CrossedStreams);
        if (result.IsSuccessful)
            await DisplayAlertAsync("Success", "Recording saved.", "OK");
        else
            await DisplayAlertAsync("Error", "Failed to save recording.", "OK");
    }

    private async void B_Exportpcm_clicked(object sender, EventArgs e)
    {
        await using var CrossedStreams = new MemoryStream(Chatter.Recording.ToArray());
        var result = await FileSaver.SaveAsync("recording.pcm", CrossedStreams);
        if (result.IsSuccessful)
            await DisplayAlertAsync("Success", "Recording saved.", "OK");
        else
            await DisplayAlertAsync("Error", "Failed to save recording.", "OK");
    }

    private async void B_Importpcm_clicked(object sender, EventArgs e)
    {
        var soundfile = await FilePicker.PickAsync();
        if(soundfile == null)
            return;
        var len = new FileInfo(soundfile.FullPath).Length;
        if (len != IChatter.SIZE_PCM)
        {
            await DisplayAlertAsync("Error",$"Incorrect size, got {len} bytes, expected {IChatter.SIZE_PCM} bytes.","cancel");
            return;
        }
        byte[] data = File.ReadAllBytes(soundfile.FullPath);
        data.CopyTo(Chatter.Recording);
        CHK_Initialized.IsChecked = Chatter.Initialized = true;
        Entry_Confusion.Text = Chatter.ConfusionChance.ToString();
    }

    private void B_Save_Click(object sender, EventArgs e)
    {
        Origin.CopyChangesFrom(SAV);
        Navigation.PopModalAsync();
    }

    private void B_Cancel_Click(object sender, EventArgs e)
    {
        Navigation.PopModalAsync();
    }
    private static int GetWAVExpectedLength() => WAVHeader.Length + (IChatter.SIZE_PCM * 2);

    /// <summary>
    /// Size: 2x <see cref="IChatter.SIZE_PCM"/>"/>
    /// </summary>
    private static ReadOnlySpan<byte> WAVHeader =>
    [
        // RIFF chunk
        0x52, 0x49, 0x46, 0x46, // chunk name: "RIFF"
        0xF4, 0x07, 0x00, 0x00, // chunk size: 2036
        0x57, 0x41, 0x56, 0x45, // format: "WAVE"

        // fmt subchunk
        0x66, 0x6D, 0x74, 0x20, // subchunk name: "fmt "
        0x10, 0x00, 0x00, 0x00, // subchunk size: 16
        0x01, 0x00,             // wFormatTag: WAVE_FORMAT_PCM (1)
        0x01, 0x00,             // nChannels: mono (1)
        0xD0, 0x07, 0x00, 0x00, // nSamplesPerSec: 2000
        0xD0, 0x07, 0x00, 0x00, // nAvgBytesPerSec: 2000
        0x01, 0x00,             // nBlockAlign: 1
        0x08, 0x00,             // wBitsPerSample: 8

        // data subchunk
        0x64, 0x61, 0x74, 0x61, // subchunk name: "data"
        0xD0, 0x07, 0x00, 0x00, // subchunk size: 2000
    ];

    /// <summary>
    /// Convert 4-bit PCM to 8-bit PCM and adds the WAV file header.
    /// </summary>
    /// <param name="pcm">Unsigned 4-bit PCM data</param>
    /// <returns>WAV file with unsigned 8-bit PCM data</returns>
    private static byte[] ConvertPCMToWAV(ReadOnlySpan<byte> pcm)
    {
        byte[] data = new byte[GetWAVExpectedLength()];
        ConvertPCMToWAV(pcm, data);
        return data;
    }

    private static void ConvertPCMToWAV(ReadOnlySpan<byte> pcm, Span<byte> result)
    {
        WAVHeader.CopyTo(result);
        var i = WAVHeader.Length;
        foreach (byte b in pcm)
        {
            result[i++] = (byte)((b & 0x0F) << 4);
            result[i++] = (byte)(b & 0xF0);
        }
    }
}