using PKHeX.Core;

namespace PKHeXMAUI;

public partial class MiscJoyful : ContentPage
{
    ISaveBlock3SmallExpansion SAV;
	public MiscJoyful(ISaveBlock3SmallExpansion j)
	{
		InitializeComponent();
		SAV = j;
        inaRowEntry.Text = Math.Min((ushort)9999, j.JoyfulJumpInRow).ToString();
        PJhighscoreEntry.Text = Math.Min(99990, j.JoyfulJumpScore).ToString();
        PJfiveEntry.Text = Math.Min((ushort)9999, j.JoyfulJump5InRow).ToString();
        MaxPlayersEntry.Text = Math.Min((ushort)9999, j.JoyfulJumpGamesMaxPlayers).ToString();
        caughtEntry.Text = Math.Min((ushort)9999, j.JoyfulBerriesInRow).ToString();
        BPhighscoreEntry.Text = Math.Min(99990, j.JoyfulBerriesScore).ToString();
        BPfiveEntry.Text = Math.Min((ushort)9999, j.JoyfulBerries5InRow).ToString();
        BerryPowderEntry.Text = Math.Min(99999u, j.BerryPowder).ToString();
    }
    public void SaveJoyful()
    {
        SAV.JoyfulJumpInRow = (ushort)Util.ToUInt32(inaRowEntry.Text);
        SAV.JoyfulJumpScore = (ushort)Util.ToUInt32(PJhighscoreEntry.Text);
        SAV.JoyfulJump5InRow = (ushort)Util.ToUInt32(PJfiveEntry.Text);
        SAV.JoyfulJumpGamesMaxPlayers = (ushort)Util.ToUInt32(MaxPlayersEntry.Text);
        SAV.JoyfulBerriesInRow = (ushort)Util.ToUInt32(caughtEntry.Text);
        SAV.JoyfulBerriesScore = (ushort)Util.ToUInt32(BPhighscoreEntry.Text);
        SAV.JoyfulBerries5InRow = (ushort)Util.ToUInt32(BPfiveEntry.Text);
        SAV.BerryPowder = Util.ToUInt32(BerryPowderEntry.Text);
    }
}