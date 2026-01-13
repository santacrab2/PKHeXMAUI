
using Octokit;
using PKHeX.Core;
using System.Xml.Serialization;

namespace PKHeXMAUI;

public partial class TrainerEditor1 : ContentPage
{
    private readonly CheckBox[] cba;
    public SaveFile SAV = MainPage.sav;
    public TrainerEditor1()
	{
		InitializeComponent();
        cba = [Badge1, Badge2, Badge3, Badge4, Badge5, Badge6, Badge7, Badge8];
        OTGenderPicker.ItemsSource = GameInfo.GenderSymbolUnicode.Take(2).ToArray();
        MaxCoinsButton.IsVisible = MainPage.sav.Generation < 3;
        OTGenderPicker.IsVisible = SAV.Generation > 1;
        CoinLabel.IsVisible = CoinsEntry.IsVisible = SAV.Generation < 3;
        SIDLabel.IsVisible = SIDEntry.IsVisible = SAV.Generation > 2;
        RegionLabel.IsVisible = RegionPicker.IsVisible = CountryLabel.IsVisible = CountryPicker.IsVisible = SAV.Generation > 3;
        PFEntry.IsVisible = PFLabel.IsVisible = PBLabel.IsVisible = PBEntry.IsVisible = SAV.Generation == 1;
        OTNameEntry.Text = SAV.OT;
        OTGenderPicker.SelectedIndex = SAV.Gender;
        TIDEntry.Text = SAV.TID16.ToString("00000");
        SIDEntry.Text = SAV.SID16.ToString("00000");
        OTMoneyEntry.Text = SAV.Money.ToString();
        HrsEntry.Text = SAV.PlayedHours.ToString();
        MinsEntry.Text = SAV.PlayedMinutes.ToString();
        SecEntry.Text = SAV.PlayedSeconds.ToString();
        int badgeval = 0;
        if(SAV is SAV1 sav1)
        {
            CoinsEntry.Text = sav1.Coin.ToString();
            badgeval = sav1.Badges;
            GSLabel.IsVisible = GSDatePicker.IsVisible = GSTimerPicker.IsVisible = HOFLabel.IsVisible = HOFDatePicker.IsVisible = HOFTimePicker.IsVisible = false;
            MapGrid.IsVisible = false;
            BattleStylePicker.ItemsSource = new string[] { "Switch", "Set" };
            SoundTypePicker.ItemsSource = new string[] { "Mono", "Stereo", "Left", "Right" };
            TextSpeedPicker.ItemsSource = new string[] { "0 (Instant)", "1 (Fast)", "2", "3 (Normal)", "4", "5 (Slow)", "6", "7" };
            UseBattleEffectsCB.IsChecked = sav1.BattleEffects;
            BattleStylePicker.SelectedIndex = sav1.BattleStyleSwitch ? 0 : 1;
            SoundTypePicker.SelectedIndex = sav1.Sound;
            TextSpeedPicker.SelectedIndex = sav1.TextSpeed;
            PFEntry.Text = sav1.PikaFriendship.ToString();
            PBEntry.Text = sav1.PikaBeachScore.ToString();
            if (!sav1.Version.Contains(GameVersion.YW))
            {
               PFLabel.IsVisible = PFEntry.IsVisible = PBEntry.IsVisible = PBLabel.IsVisible = PBEntry.IsVisible = SoundTypePicker.IsVisible = SoundTypeLabel.IsVisible = false;
            }
        }
        if(SAV is SAV2 sav2)
        {
            CoinsEntry.Text = sav2.Coin.ToString();
            GSLabel.IsVisible = GSDatePicker.IsVisible = GSTimerPicker.IsVisible = HOFLabel.IsVisible = HOFDatePicker.IsVisible = HOFTimePicker.IsVisible = false;
            MapGrid.IsVisible = false;
            BattleStylePicker.ItemsSource = new string[] { "Switch", "Set" };
            SoundTypePicker.ItemsSource = new string[] { "Mono", "Stereo", "Left", "Right" };
            TextSpeedPicker.ItemsSource = new string[] { "0 (Instant)", "1 (Fast)", "2", "3 (Normal)", "4", "5 (Slow)", "6", "7" };
            UseBattleEffectsCB.IsChecked = sav2.BattleEffects;
            BattleStylePicker.SelectedIndex = sav2.BattleStyleSwitch ? 0 : 1;
            SoundTypePicker.SelectedIndex = sav2.Sound;
            TextSpeedPicker.SelectedIndex = sav2.TextSpeed;
            badgeval = sav2.Badges;
            cba = cba = [Badge1, Badge2, Badge3, Badge4, Badge5, Badge6, Badge7, Badge8, Badge9, Badge10, Badge11, Badge12, Badge13, Badge14, Badge15, Badge16];
        }
        if(SAV is SAV3 sav3)
        {
            GSLabel.IsVisible = GSDatePicker.IsVisible = GSTimerPicker.IsVisible = HOFLabel.IsVisible = HOFDatePicker.IsVisible = HOFTimePicker.IsVisible = false;
            MapGrid.IsVisible = false;
            OptionsGrid.IsVisible = false;
            badgeval = sav3.Badges;
        }
        if (SAV is SAV3Colosseum or SAV3XD)
        {
            GSLabel.IsVisible = GSDatePicker.IsVisible = GSTimerPicker.IsVisible = HOFLabel.IsVisible = HOFDatePicker.IsVisible = HOFTimePicker.IsVisible = false;
            MapGrid.IsVisible = false;
            OptionsGrid.IsVisible = false;
            HrsEntry.IsVisible = MinsEntry.IsVisible = SecEntry.IsVisible = minLabel.IsVisible = hrsLabel.IsVisible = secLabel.IsVisible = false;
            return;
        }
        if(SAV is SAV4 sav4)
        {
            OptionsGrid.IsVisible = false;
            CurrentMapEntry.Text = sav4.M.ToString();
            XCoordEntry.Text = sav4.X.ToString();
            YCoordinate.Text = sav4.Y.ToString();
            ZCoordEntry.Text = sav4.Z.ToString();
            badgeval = sav4.Badges;
            if (sav4 is SAV4HGSS hgss)
            {
                badgeval |= hgss.Badges16 << 8;
                cba = [.. cba, Badge9, Badge10, Badge11, Badge12, Badge13, Badge14, Badge15, Badge16];
            }
            CountryPicker.ItemsSource = Util.GetCountryRegionList("gen4_countries", GameInfo.CurrentLanguage);
            CountryPicker.ItemDisplayBinding = new Binding("Text");
            CountryPicker.SelectedItem = Util.GetCountryRegionList("gen4_countries", GameInfo.CurrentLanguage).Find(z=>z.Value == sav4.Country);
        }
        if(SAV is SAV5 sav5)
        {
            BPEntry.IsVisible = BPLabel.IsVisible = MaxBPButton.IsVisible = true;
            BPEntry.Text = sav5.BattleSubway.BP.ToString();
            var pd = sav5.PlayerPosition;
            OptionsGrid.IsVisible = false;
            MapGrid.IsVisible = true;
            CurrentMapEntry.Text = pd.M.ToString();
            XCoordEntry.Text = pd.X.ToString();
            YCoordinate.Text = pd.Y.ToString();
            ZCoordEntry.Text = pd.Z.ToString();
            badgeval = sav5.Misc.Badges;
            CountryPicker.ItemsSource = Util.GetCountryRegionList("gen5_countries", GameInfo.CurrentLanguage);
            CountryPicker.ItemDisplayBinding = new Binding("Text");
            CountryPicker.SelectedItem = Util.GetCountryRegionList("gen5_countries", GameInfo.CurrentLanguage).Find(z => z.Value == sav5.Country);
        }
        for (int i = 0; i < cba.Length; i++)
        {
            cba[i].IsVisible = true;
            cba[i].IsChecked = (badgeval & (1 << i)) != 0;
        }
        DateUtil.GetDateTime2000(SAV.SecondsToStart, out var date, out var time);
        GSDatePicker.Date = date;
        GSTimerPicker.Time = time.TimeOfDay;
        DateUtil.GetDateTime2000(SAV.SecondsToFame, out date, out time);
        HOFDatePicker.Date = date;
        HOFTimePicker.Time = time.TimeOfDay;
    }

    private void MaxMoney(object sender, EventArgs e)
    {
		OTMoneyEntry.Text =  MainPage.sav.MaxMoney.ToString();
    }

    private void MaxCoins(object sender, EventArgs e)
    {
        CoinsEntry.Text = MainPage.sav.MaxCoins.ToString();
    }

    private void MaxBP(object sender, EventArgs e)
    {
        BPEntry.Text = MainPage.sav.MaxCoins.ToString();
    }

    private void CloseTE1(object sender, EventArgs e)
    {
        Navigation.PopModalAsync();
    }

    private void SaveTE1(object sender, EventArgs e)
    {
        if (SAV.OT != OTNameEntry.Text)
            SAV.OT = OTNameEntry.Text;
        SAV.Gender = (byte)OTGenderPicker.SelectedIndex;
        var parsed = ushort.TryParse(TIDEntry.Text, out var result);
        if (parsed) SAV.TID16 = result;
        parsed = ushort.TryParse(SIDEntry.Text, out result);
        if (parsed) SAV.SID16 = result;
        parsed = uint.TryParse(OTMoneyEntry.Text, out var uresult);
        if (parsed) SAV.Money = uresult;
        parsed = int.TryParse(HrsEntry.Text, out var iresult);
        if (parsed) SAV.PlayedHours = iresult;
        parsed = int.TryParse(MinsEntry.Text, out iresult);
        if (parsed) SAV.PlayedMinutes = iresult;
        parsed = int.TryParse(SecEntry.Text, out iresult);
        if (parsed) SAV.PlayedSeconds = iresult;

        int badgeval = 0;
        for (int i = 0; i < cba.Length; i++)
            badgeval |= (cba[i].IsChecked ? 1 : 0) << i;
        if(SAV is SAV1 sav1)
        {
            parsed = uint.TryParse(CoinsEntry.Text, out uresult);
            if (parsed) sav1.Coin = (uint)Math.Clamp(uresult, 0, SAV.MaxCoins);
            sav1.Badges = badgeval & 0xFF;
            parsed = byte.TryParse(PFEntry.Text, out var bresult);
            if (parsed) sav1.PikaFriendship = Math.Clamp(bresult, (byte)0, (byte)255);
            parsed = ushort.TryParse(PBEntry.Text, out result);
            if (parsed) sav1.PikaBeachScore = Math.Clamp(result, (ushort)0, (ushort)9999);
            sav1.BattleEffects = UseBattleEffectsCB.IsChecked;
            sav1.BattleStyleSwitch = BattleStylePicker.SelectedIndex == 0;
            sav1.Sound = SoundTypePicker.SelectedIndex;
            sav1.TextSpeed = TextSpeedPicker.SelectedIndex;
        }
        if(SAV is SAV2 sav2)
        {
            parsed = uint.TryParse(CoinsEntry.Text, out uresult);
            if (parsed) sav2.Coin = (uint)Math.Clamp(uresult, 0, SAV.MaxCoins);
            sav2.Badges = badgeval & 0xFFFF;
            sav2.BattleEffects = UseBattleEffectsCB.IsChecked;
            sav2.BattleStyleSwitch = BattleStylePicker.SelectedIndex == 0;
            sav2.Sound = SoundTypePicker.SelectedIndex > 0 ? 2 : 0;
            sav2.TextSpeed = TextSpeedPicker.SelectedIndex;
        }
        if(SAV is SAV3 sav3)
        {
            sav3.Badges = badgeval & 0xFF;
        }
        if(SAV is SAV4 sav4)
        {
            parsed = int.TryParse(CurrentMapEntry.Text, out iresult);
            if (parsed && iresult != sav4.M) sav4.M = iresult;
            parsed = int.TryParse(XCoordEntry.Text, out iresult);
            if (parsed && iresult != sav4.X) sav4.X = iresult;
            parsed = int.TryParse(YCoordinate.Text, out iresult);
            if (parsed && iresult != sav4.Y) sav4.Y = iresult;
            parsed = int.TryParse(ZCoordEntry.Text, out iresult);
            if (parsed && iresult != sav4.Z) sav4.Z = iresult;
            sav4.Badges = badgeval & 0xFF;
            if (sav4 is SAV4HGSS hgss)
            {
                hgss.Badges16 = badgeval >> 8;
            }
            var country = (ComboItem)CountryPicker.SelectedItem;
            if (country is not null)
                sav4.Country = country.Value;
            var region = (ComboItem)RegionPicker.SelectedItem;
            if (region is not null)
                sav4.Region = region.Value;
        }
        if(SAV is SAV5 s)
        {
            var pd = s.PlayerPosition;
            parsed = int.TryParse(CurrentMapEntry.Text, out iresult);
            if (parsed && iresult != pd.M) pd.M = iresult;
            parsed = int.TryParse(XCoordEntry.Text, out iresult);
            if (parsed && iresult != pd.X) pd.X = iresult;
            parsed = int.TryParse(YCoordinate.Text, out iresult);
            if (parsed && iresult != pd.Y) pd.Y = iresult;
            parsed = int.TryParse(ZCoordEntry.Text, out iresult);
            if (parsed && iresult != pd.Z) pd.Z = iresult;
            s.Misc.Badges = badgeval & 0xFF;
            parsed = int.TryParse(BPEntry.Text, out iresult);
            if (parsed) s.BattleSubway.BP = Math.Clamp(iresult,0,s.MaxCoins);
            var country = (ComboItem)CountryPicker.SelectedItem;
            if (country is not null)
                s.Country = country.Value;
            var region = (ComboItem)RegionPicker.SelectedItem;
            if (region is not null)
                s.Region = region.Value;
        }
        DateTime Epoch2000 = new(2000, 1, 1);
        SAV.SecondsToStart = (uint)DateUtil.GetSecondsFrom2000(GSDatePicker.Date.GetValueOrDefault(), Epoch2000.AddSeconds(GSTimerPicker.Time.GetValueOrDefault().TotalSeconds % 86400));
        SAV.SecondsToFame = (uint)DateUtil.GetSecondsFrom2000(HOFDatePicker.Date.GetValueOrDefault(), Epoch2000.AddSeconds(HOFTimePicker.Time.GetValueOrDefault().TotalSeconds % 86400));
        Navigation.PopModalAsync();
    }

    private void UpdateRegions(object sender, EventArgs e)
    {
        if(SAV is SAV4 sav4)
        {
            var index = ((Picker)sender).SelectedIndex;
            var RegionItems = Util.GetCountryRegionList($"gen4_sr_{index:000}", GameInfo.CurrentLanguage);
            if(RegionItems.Count == 0)
                RegionItems = Util.GetCountryRegionList("gen4_sr_default", GameInfo.CurrentLanguage);
            RegionPicker.ItemsSource = RegionItems;
            RegionPicker.ItemDisplayBinding = new Binding("Text");
            RegionPicker.SelectedItem = RegionItems.Find(z => z.Value == sav4.Region)??new ComboItem("",0);
        }
        if(SAV is SAV5 sav5)
        {
            var index = ((Picker)sender).SelectedIndex;
            var RegionItems = Util.GetCountryRegionList($"gen5_sr_{index:000}", GameInfo.CurrentLanguage);
            if (RegionItems.Count == 0)
                RegionItems = Util.GetCountryRegionList("gen5_sr_default", GameInfo.CurrentLanguage);
            RegionPicker.ItemsSource = RegionItems;
            RegionPicker.ItemDisplayBinding = new Binding("Text");
            RegionPicker.SelectedItem = RegionItems.Find(z => z.Value == sav5.Region) ?? new ComboItem("", 0);
        }
    }
}