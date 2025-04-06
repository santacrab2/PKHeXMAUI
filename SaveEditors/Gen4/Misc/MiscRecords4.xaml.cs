using PKHeX.Core;

namespace PKHeXMAUI;

public partial class MiscRecords4 : ContentPage
{
    private readonly Record4 Record;
    public MiscRecords4(SAV4 sav)
	{
        Record = sav.Records;
		InitializeComponent();
        NUD_Record16.MaxValue = Record4.Record16 - 1;
        NUD_Record32.MaxValue = Record.Record32 - 1;
        NUD_Record16V.Number = Record.GetRecord16(0);
        NUD_Record32V.Number = Record.GetRecord32(0);
        NUD_Record16V.ValueChanged += (_, _) => Record.SetRecord16((int)NUD_Record16.Number, (ushort)NUD_Record16V.Number);
        NUD_Record32V.ValueChanged += (_, _) => Record.SetRecord32((int)NUD_Record32.Number, (uint)NUD_Record32V.Number);
        NUD_Record16.ValueChanged += (_, _) => NUD_Record16V.Number = Record.GetRecord16((int)NUD_Record16.Number);
        NUD_Record32.ValueChanged += (_, _) => NUD_Record32V.Number = Record.GetRecord32((int)NUD_Record32.Number);

    }
    public void SaveRecord() => Record.EndAccess();
}