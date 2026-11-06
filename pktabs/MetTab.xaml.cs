
using System.Globalization;
using System.Windows.Input;
using PKHeX.Core;

using static PKHeXMAUI.MainPage;

namespace PKHeXMAUI;

public partial class MetTab : ContentPage
{
    public bool SkipEvent = false;
    public bool FirstLoad = true;
	public MetTab()
	{
		InitializeComponent();
        // 蛋图：b_ 集里没有蛋素材（b_0.png 是空图），统一用唯一那张 a_egg.png
        eggsprite.Source = SpeciesSpriteUtil.EggFileName(sav);
        FirstLoad = false;
        mettabpic.Source = spriteurl;
        origingamepicker.ItemDisplayBinding = new Binding("Text");
        origingamepicker.ItemsSource = (System.Collections.IList)datasourcefiltered.Games;
        battleversionpicker.ItemsSource = GameInfo.Strings.gamelist;
        metlocationpicker.ItemDisplayBinding = new Binding("Text");
        metlocationpicker.ItemsSource = (System.Collections.IList)GameInfo.GetLocationList((GameVersion)pk.Version, pk.Context);
        // 球种用中文名显示（GameInfo.Strings.balllist 与 Ball 枚举一一对应）
        ballpicker.ItemDisplayBinding = new Binding(".", converter: new BallNameConverter());

        eggmetpicker.ItemsSource = (System.Collections.IList)GameInfo.GetLocationList(sav.Version, sav.Context, true);
        eggmetpicker.ItemDisplayBinding = new Binding("Text");
        if (PSettings.DisplayLegalBallsOnly)
        {
            // GetLegalBalls 需要足够大的 Span，空 Span 在写入时会抛 IndexOutOfRangeException；
            // 这里按全量球枚举分配，再剔除未被填充的 Ball.None。
            Ball[] PokemonBalls = new Ball[Enum.GetValues<Ball>().Length];
            BallApplicator.GetLegalBalls(PokemonBalls, pk);
            var legalBalls = PokemonBalls.Where(b => b != Ball.None).ToList();
            ballpicker.ItemsSource = legalBalls.Count != 0 ? legalBalls : Enum.GetValues<Ball>().ToList();
        }
        else
        {
            ballpicker.ItemsSource = Enum.GetValues<Ball>();
        }

        ICommand refreshCommand = new Command(async () =>
        {
            await applymetinfo(pk);
            MetRefresh.IsRefreshing = false;
        });
        MetRefresh.Command = refreshCommand;
        applymetinfo(pk);
        
    }
    public static string ballspriteurl = "ball4.png";
    protected override void OnAppearing()
    {
        if (!FirstLoad)
            applymetinfo(pk);
    }
    public async Task applymetinfo(PKM pkm)
    {
        try
        {
        SkipEvent = true;
        eggsprite.IsVisible = pkm.IsEgg;
        if (PSettings.DisplayLegalBallsOnly)
        {
            Ball[] PokemonBalls = new Ball[Enum.GetValues<Ball>().Length];
            BallApplicator.GetLegalBalls(PokemonBalls, pkm);
            var legalBalls = PokemonBalls.Where(b => b != Ball.None).ToList();
            ballpicker.ItemsSource = legalBalls.Count != 0 ? legalBalls : Enum.GetValues<Ball>().ToList();
        }
        if (pkm.HeldItem > 0)
        {
            itemsprite.Source = itemspriteurl;
            itemsprite.IsVisible = true;
        }
        else
        {
            itemsprite.IsVisible = false;
        }

        shinysparklessprite.IsVisible = pkm.IsShiny;
        spriteurl = SpeciesSpriteUtil.FileName(sav, pkm.Species, pkm.Form, pkm.IsShiny);
        mettabpic.Source = spriteurl;
        origingamepicker.SelectedItem = datasourcefiltered.Games.FirstOrDefault(z => z.Value == (int)pkm.Version);
        if (pkm is IBattleVersion bv)
        {
            battleversionlabel.IsVisible = true;
            battleversionpicker.IsVisible = true;
            int bvIdx = (int)bv.BattleVersion;
            // 守卫：BattleVersion 超出 gamelist 长度时 Picker 会抛异常，越界则留空(-1)
            battleversionpicker.SelectedIndex = bvIdx >= 0 && bvIdx < GameInfo.Strings.gamelist.Length ? bvIdx : -1;
        }

        // 地点表必须与当前 pkm 的版本/上下文一致，且回填的项要来自同一张表，
        // 否则 Picker 按引用找不到匹配项，会显示空白（GetLocationList 返回的是缓存实例）。
        var metLocations = GameInfo.GetLocationList((GameVersion)pkm.Version, pkm.Context);
        metlocationpicker.ItemsSource = (System.Collections.IList)metLocations;
        metlocationpicker.SelectedItem = metLocations.FirstOrDefault(z=>z.Value == pkm.MetLocation);
        ballpicker.SelectedItem = pkm.Ball > 0 ? (Ball)pkm.Ball : Ball.None;
        ballspriteurl = $"{(pkm.Ball>0?$"ball{pkm.Ball}":"ball4")}.png";
        ballimage.Source = ballspriteurl;

        var metdate = pkm.MetDate!=null? (DateOnly)pkm.MetDate:DateOnly.MinValue;
        metdatepicker.Date = pkm.MetDate != null?metdate.ToDateTime(TimeOnly.Parse("10:00 PM", CultureInfo.InvariantCulture)) :DateTime.Now;
        metleveldisplay.Text = pkm.MetLevel>0?pkm.MetLevel.ToString():"0";
        if (pkm is IObedienceLevel ob)
        {
            obediencelevellabel.IsVisible = true;
            obedienceleveldisplay.IsVisible = true;
            obedienceleveldisplay.Text = ob.ObedienceLevel.ToString();
        }
        fatefulcheck.IsChecked = pkm.FatefulEncounter;
        eggcheck.IsChecked = pkm.WasEgg;

        var eggmetdate = pkm.EggMetDate!=null? (DateOnly)pkm.EggMetDate:DateOnly.MinValue;
        eggdatepicker.Date = pkm.EggMetDate != null?eggmetdate.ToDateTime(TimeOnly.Parse("10:00 PM", CultureInfo.InvariantCulture)):DateTime.Now;
        var eggLocations = GameInfo.GetLocationList((GameVersion)sav.Version, sav.Context, true);
        eggmetpicker.ItemsSource = (System.Collections.IList)eggLocations;
        eggmetpicker.SelectedItem = eggLocations.FirstOrDefault(z => z.Value == pkm.EggLocation);
        SkipEvent = false;
        }
        catch (Exception ex)
        {
            // 相遇页填充若仍有异常（如某代宝可梦数据触发的越界），记到 debug.log 并继续渲染，避免整页崩掉
            CrashLog.Write("相遇页填充", ex);
            SkipEvent = false;
        }
    }

    public void applyorigingame(object sender, EventArgs e)
    {
        if (!SkipEvent && origingamepicker.SelectedItem is ComboItem version)
        {
            try
            {
                pk.Version = (GameVersion)version.Value;
                // 换游戏后整张地点表都变了，必须先换表再回填选中项，否则下拉显示与实际值对不上
                var locations = GameInfo.GetLocationList((GameVersion)pk.Version, pk.Context);
                metlocationpicker.ItemsSource = (System.Collections.IList)locations;
                metlocationpicker.SelectedItem = locations.FirstOrDefault(z => z.Value == pk.MetLocation);
            }
            catch (Exception ex)
            {
                CrashLog.Write("相遇页切换来源游戏", ex);
            }
        }
    }

    private void applybattleversion(object sender, EventArgs e)
    {
        if (!SkipEvent && battleversionpicker.SelectedIndex >= 0 && pk is IBattleVersion bv)
            bv.BattleVersion = (GameVersion)battleversionpicker.SelectedIndex;
    }

    private void applymetlocation(object sender, EventArgs e)
    {
        // MetLocation 是 ushort：Gen8 地点编码可达 0xFFFE(65534)，Gen4/5/6/7 也超过 255。
        // 这里若按 byte 强转会静默截断，写入错误地点甚至抛 OverflowException 让整页崩掉。
        if (metlocationpicker.SelectedItem is ComboItem metlocation && !SkipEvent)
            pk.MetLocation = (ushort)metlocation.Value;
    }

    private void givebackballs(object sender, EventArgs e)
    {
        if (!SkipEvent && ballpicker.SelectedItem is Ball ball)
        {
            pk.Ball = (byte)ball;
            ballspriteurl = $"ball{pk.Ball}.png";
            ballimage.Source = ballspriteurl;
        }
    }

    private void applymetdate(object sender, DateChangedEventArgs e)
    {
        if(!SkipEvent)
            pk.MetDate = DateOnly.FromDateTime(metdatepicker.Date??DateTime.MinValue);
    }

    private void applymetlevel(object sender, TextChangedEventArgs e)
    {
        // 输入框可能被清空或填入非数字，用 TryParse 避免 FormatException 把整页带崩
        if (!SkipEvent && byte.TryParse(metleveldisplay.Text, out var metLevel))
        {
            pk.MetLevel = metLevel;
        }
    }

    private void applyobediencelevel(object sender, TextChangedEventArgs e)
    {
        if (pk is IObedienceLevel ob && !SkipEvent)
        {
            if (byte.TryParse(obedienceleveldisplay.Text, out var obedience))
                ob.ObedienceLevel = obedience;
        }
    }

    private void applyfateful(object sender, CheckedChangedEventArgs e)
    {
        if(!SkipEvent)
            pk.FatefulEncounter = fatefulcheck.IsChecked;
    }

    private void applyeggmetlocation(object sender, EventArgs e)
    {
        // EggLocation 同为 ushort，不能按 byte 截断；SelectedItem 可能为 null，需守卫
        if (eggmetpicker.SelectedItem is ComboItem egglocation && !SkipEvent)
            pk.EggLocation = (ushort)egglocation.Value;
    }

    private void applyeggdate(object sender, DateChangedEventArgs e)
    {
        if(!SkipEvent)
        pk.EggMetDate = DateOnly.FromDateTime(eggdatepicker.Date??DateTime.MinValue);
    }

    private void wasegg(object sender, CheckedChangedEventArgs e)
    {
        if (!SkipEvent)
        {
            if (eggcheck.IsChecked)
            {
                pk.EggLocation = EncounterSuggestion.GetSuggestedEncounterEggLocationEgg(pk, true);
                // 原写法 FromDayNumber(DateTime.Now.Day) 会把日期算到公元 0001 年，这里用真实日期
                pk.EggMetDate = DateOnly.FromDateTime(DateTime.Now);
            }
            else
            {
                pk.EggLocation = LocationEdits.GetNoneLocation(pk);
                pk.EggMetDate = null;
            }
        }
    }

}

/// <summary>
/// 球种中文显示转换器：把 Ball 枚举按 GameInfo.Strings.balllist 译成中文（索引与枚举值一一对应）。
/// </summary>
internal class BallNameConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is Ball ball)
        {
            var list = GameInfo.Strings.balllist;
            int idx = (int)ball;
            if (list is not null && idx >= 0 && idx < list.Length && !string.IsNullOrEmpty(list[idx]))
                return list[idx];
        }
        return value?.ToString() ?? string.Empty;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
