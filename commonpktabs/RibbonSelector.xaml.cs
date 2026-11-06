
using PKHeX.Core;
using static PKHeXMAUI.MainPage;
namespace PKHeXMAUI;

public partial class RibbonSelector : ContentPage
{
    public static bool ApplicatorMode = false;
    public RibbonSelector()
    {
        InitializeComponent();
        if (ApplicatorMode)
            applyribbons.IsVisible = false;
        else
            affixribbons.IsVisible = false;

        ribboncollection.ItemTemplate = new DataTemplate(() =>
        {
            Grid grid = new() { Padding = 10 };
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = 350 });
            Label ribbonname = new() { HorizontalTextAlignment = TextAlignment.End };
            ribbonname.SetBinding(Label.TextProperty, "Name");
            grid.Add(ribbonname);
            Image allthetests = new() { HeightRequest = 50, WidthRequest = 50, HorizontalOptions = LayoutOptions.Start, Margin = new Thickness(50, 0, 0, 0) };
            allthetests.SetBinding(Image.SourceProperty, "spritename");
            Image AffixedSprite = new() { HeightRequest = 16, WidthRequest = 16, HorizontalOptions = LayoutOptions.End, VerticalOptions = LayoutOptions.End, Margin = new Thickness(50, 0, 0, 0), Source = "valid.png" };
            AffixedSprite.SetBinding(Image.IsVisibleProperty, "affixed");
            grid.Add(allthetests);
            grid.Add(AffixedSprite);
            grid.SetBinding(Grid.BackgroundColorProperty, "legal");
            SwipeView Swipe = new();
            SwipeItem affix = new()
            {
                Text = L10n.T("gen3_decorations"),
                BackgroundColor = Colors.DeepSkyBlue,
                IconImageSource = "ribbon_affix_noneb.png"
            };
            affix.Invoked += AffixRibbon;
            if(ApplicatorMode)
                Swipe.LeftItems.Add(affix);
            Swipe.Content = grid;
            return Swipe;
        });
        var clone = pk.Clone();
        RibbonApplicator.SetAllValidRibbons(clone);
        var ribbs = RibbonInfo.GetRibbonInfo(clone);
        List<Ribbonstuff> idk = [];
        foreach(var fg in ribbs)
        {
            idk.Add(new Ribbonstuff(fg) { legal = fg.HasRibbon ? Color.FromArgb("80008000") : Colors.Transparent});
        }
        var selectedribbonslist = new List<object>();
        var pkhasribbonslist = RibbonInfo.GetRibbonInfo(pk);
        foreach(var pkrib in pkhasribbonslist)
        {
            if (pkrib.HasRibbon)
            {
                foreach(var imrunningoutofnames in idk)
                {
                    if(imrunningoutofnames.info.Name == pkrib.Name)
                    {
                        selectedribbonslist.Add(imrunningoutofnames);
                    }
                }
            }
        }
        if (!ApplicatorMode)
        {
            ribboncollection.SelectionMode = SelectionMode.Multiple;
            ribboncollection.ItemsSource = idk;
            ribboncollection.UpdateSelectedItems(selectedribbonslist);
        }
        else
        {
            ribboncollection.SelectionMode = SelectionMode.Single;
            ribboncollection.ItemsSource = selectedribbonslist;
        }
    }
    public void Refresh()
    {
        var clone = pk.Clone();
        RibbonApplicator.SetAllValidRibbons(clone);
        var ribbs = RibbonInfo.GetRibbonInfo(clone);
        List<Ribbonstuff> idk = [];
        foreach (var fg in ribbs)
        {
            idk.Add(new Ribbonstuff(fg) { legal = fg.HasRibbon ? Colors.Green : Colors.Transparent });
        }
        var selectedribbonslist = new List<object>();
        var pkhasribbonslist = RibbonInfo.GetRibbonInfo(pk);
        foreach (var pkrib in pkhasribbonslist)
        {
            if (pkrib.HasRibbon)
            {
                foreach (var imrunningoutofnames in idk)
                {
                    if (imrunningoutofnames.info.Name == pkrib.Name)
                    {
                        selectedribbonslist.Add(imrunningoutofnames);
                    }
                }
            }
        }
        if (!ApplicatorMode)
        {
            ribboncollection.SelectionMode = SelectionMode.Multiple;
            ribboncollection.ItemsSource = idk;
            ribboncollection.UpdateSelectedItems(selectedribbonslist);
        }
        else
        {
            ribboncollection.SelectionMode = SelectionMode.Single;
            ribboncollection.ItemsSource = selectedribbonslist;
        }
        ribboncollection.ScrollTo(0);
    }
    private void AffixRibbon(object? sender,EventArgs? e)
    {
        if (ribboncollection.SelectedItem == null)
            return;
        if(pk is IRibbonSetAffixed a)
        {
            var ribbon = (Ribbonstuff)ribboncollection.SelectedItem;
            // 没有装饰序号的奖章（旧世代/特殊奖章）不做装饰，否则会误清除已有装饰
            if (ribbon.index < 0)
                return;
            a.AffixedRibbon = a.AffixedRibbon == ribbon.index ? (sbyte)-1 : (sbyte)ribbon.index;
        }
        Refresh();
    }
    private void applyribbonsandclose(object sender, EventArgs e)
    {
        var riblist = RibbonInfo.GetRibbonInfo(pk);
        foreach (var rib in riblist)
        {
            ReflectUtil.SetValue(pk, rib.Name, rib.Type is RibbonValueType.Boolean ? false : rib.RibbonCount);
        }

        foreach (var ribs in ribboncollection.SelectedItems)
        {
            var rib = (Ribbonstuff)ribs;
            var test = ReflectUtil.SetValue(pk, rib.info.Name, rib.typer is RibbonValueType.Boolean ? true : rib.info.RibbonCount);
        }
        Navigation.PopModalAsync();
    }

    private void closer(object sender, EventArgs e)
    {
        Navigation.PopModalAsync();
    }
}

public class Ribbonstuff
{
    public Ribbonstuff(RibbonInfo rib)
    {
        // RibbonIndex 是「装饰奖章」用的序号表，只有 Gen8+ 才有装饰。
        // 早期版本只扫到 110，枚举里最后两个（MarkPartner / Partner）永远匹配不上，
        // 这里改成遍历全部枚举值；匹配不到的旧世代奖章用 -1 表示「不可装饰」。
        index = -1;
        var stripped = rib.Name.Replace("Ribbon", "");
        foreach (var ri in Enum.GetValues<RibbonIndex>())
        {
            if (stripped == ri.ToString())
            {
                index = (int)ri;
                break;
            }
        }

        if(pk is IRibbonSetAffixed af)
        {
            // index 为 -1 表示该奖章没有对应的装饰序号，不能拿来当装饰
            affixed = index >= 0 && af.AffixedRibbon == index;
        }
        typer = rib.Type;
        Name = DisplayName(rib.Name);
        spritename = SpriteName(rib.Name);
        info = rib;
    }

    /// <summary>去掉 Ribbon 前缀后的显示名；Gen3 计数类奖章额外去掉中间的 Count。</summary>
    private static string DisplayName(string coreName)
    {
        var n = coreName.StartsWith("Ribbon", StringComparison.Ordinal) ? coreName["Ribbon".Length..] : coreName;
        if (n.StartsWith("CountG3", StringComparison.Ordinal))
            n = n["Count".Length..];
        return n;
    }

    /// <summary>
    /// Core 的奖章属性名 -> APK 里的图片名。
    ///
    /// Android 的资源名会被 AAPT2 强制转成小写（APK 的 R 表里清一色是 ribbonxxx），
    /// 所以早期写的 $"{rib.Name}.png"（例如 RibbonAbility.png）永远找不到资源，
    /// 结果就是整个奖章列表的图片位置一片空白。
    ///
    /// 例外：Gen3 的计数类奖章在 Core 里叫 RibbonCountG3Cool，图片却叫 ribbong3cool.png。
    /// </summary>
    private static string SpriteName(string coreName)
    {
        var n = coreName.Replace("RibbonCountG3", "RibbonG3").ToLowerInvariant() + ".png";
        // 极少数奖章（目前只有 RibbonPartner）没有对应图片，退回「无装饰」占位图
        return ResUtil.Pick(n, "ribbon_affix_none.png");
    }

    public string Name { get; set; }
    public string spritename { get; set; }
    public RibbonValueType typer{ get; set; }
    public int index { get; set; }
    public bool affixed { get; set; }
    public Color legal { get; set; } = Colors.Transparent;
   public RibbonInfo info { get; set; }
}