
using PKHeX.Core;

namespace PKHeXMAUI;

public partial class RTC3Editor : ContentPage
{
	public RTC3Editor(SaveFile sav)
	{
		InitializeComponent();
        SAV = (SAV3)sav;
        Small = (ISaveBlock3SmallHoenn)SAV.SmallBlock;
        ClockInitial = Small.ClockInitial;
        ClockElapsed = Small.ClockElapsed;
        InitialDayEntry.Text = ClockInitial.Day.ToString();
        InitialHoursEntry.Text = ClockInitial.Hour.ToString();
        InitialMinutesEntry.Text = ClockInitial.Minute.ToString();
        InitialSecondsEntry.Text = ClockInitial.Second.ToString();
        ElapsedDayEntry.Text = ClockElapsed.Day.ToString();
        ElapsedHoursEntry.Text = ClockElapsed.Hour.ToString();
        ElapsedMinutesEntry.Text = ClockElapsed.Minute.ToString();
        ElapsedSecondsEntry.Text = ClockElapsed.Second.ToString();
    }
    private readonly SAV3 SAV;
    private readonly ISaveBlock3SmallHoenn Small;
    private readonly RTC3 ClockInitial;
    private readonly RTC3 ClockElapsed;
    private void ResetRTCClick(object sender, EventArgs e)
    {
        InitialSecondsEntry.Text = InitialDayEntry.Text = InitialHoursEntry.Text = InitialMinutesEntry.Text = "0";
        ElapsedDayEntry.Text = ElapsedHoursEntry.Text = ElapsedMinutesEntry.Text = ElapsedSecondsEntry.Text = "0";
    }

    private void BerryFixClick(object sender, EventArgs e)
    {
        ElapsedDayEntry.Text = Math.Max((2 * 366) + 2, int.Parse(ElapsedDayEntry.Text)).ToString();
    }

    private void CloseRTC(object sender, EventArgs e)
    {
        Navigation.PopModalAsync();
    }

    private void SaveRTC(object sender, EventArgs e)
    {
        SaveData();
        Small.ClockInitial = ClockInitial;
        Small.ClockElapsed = ClockElapsed;
        Navigation.PopModalAsync();
    }
    private void SaveData()
    {
        ClockInitial.Day = ushort.Parse(InitialDayEntry.Text);
        ClockInitial.Hour = byte.Parse(InitialHoursEntry.Text);
        ClockInitial.Minute = byte.Parse(InitialMinutesEntry.Text);
        ClockInitial.Second = byte.Parse(InitialSecondsEntry.Text);
        ClockElapsed.Day = ushort.Parse(ElapsedDayEntry.Text);
        ClockElapsed.Hour = byte.Parse(ElapsedHoursEntry.Text);
        ClockElapsed.Minute = byte.Parse(ElapsedMinutesEntry.Text);
        ClockElapsed.Second = byte.Parse(ElapsedSecondsEntry.Text);
    }
}