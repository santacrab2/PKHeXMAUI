
using PKHeX.Core;

using System.Windows.Input;
using static PKHeX.Core.Encounters9;


using static pk9reader.MainPage;





namespace pk9reader;

public partial class RaidViewer : ContentPage
{

    public RaidViewer()
    {
        InitializeComponent();
        ICommand refreshCommand = new Command(async () =>
        {
            var itemso = await raidlist();
            raidview.ItemsSource = itemso;

            raidrefresh.IsRefreshing = false;
        });
        raidrefresh.Command = refreshCommand;
        raidview.ItemTemplate = new DataTemplate(() =>
        {
            Grid grid = new Grid { Padding = 10 };
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = 350 });


            Image image = new Image() { HeightRequest = 100, WidthRequest = 100, HorizontalOptions = LayoutOptions.Start, Margin = new Thickness(50, 0, 0, 0) };
            image.SetBinding(Image.SourceProperty, "url");
            image.SetBinding(Image.BackgroundColorProperty, "bgcolor");
            grid.Add(image);
            Label teratype = new Label() { HorizontalTextAlignment = TextAlignment.Start, Margin = new Thickness(20, 0, 0, 0) };
            teratype.SetBinding(Label.TextProperty, "pkm.TeraTypeOriginal");
            grid.Add(teratype);
            Label loc = new Label() { HorizontalTextAlignment = TextAlignment.End };
            loc.SetBinding(Label.TextProperty, "location");
            grid.Add(loc);
            Label sta = new Label() { HorizontalTextAlignment = TextAlignment.End, VerticalTextAlignment = TextAlignment.End };
            sta.SetBinding(Label.TextProperty, "Stars");
            grid.Add(sta);
            return grid;
        });
        initializedicts();

    }
    public static Dictionary<string, double[]> denloc;

    public static raidsprite mainpk;
    public uint getpid(uint seed)
    {
        var xoro = new Xoroshiro128Plus(seed);
        xoro.NextInt(4294967295uL);
        xoro.NextInt(4294967295uL);
        uint pid = (uint)xoro.NextInt(4294967295uL);
        return pid;
    }
    public uint getoid(uint seed)
    {
        var xoro = new Xoroshiro128Plus(seed);
        xoro.NextInt(4294967295uL);
        uint oid = (uint)xoro.NextInt(4294967295uL);
        return oid;
    }
    public int getxororandrate(uint seed, uint max, int stars)
    {
        var xoro = new Xoroshiro128Plus(seed);
        if (stars < 6) xoro.NextInt(100);
        return (int)xoro.NextInt(max);
    }
    public static int getxoronextint(uint seed, ulong rand)
    {
        var xoro = new Xoroshiro128Plus(seed);

        return (int)xoro.NextInt(rand);
    }
    public ulong getxoronextseed(uint seed)
    {
        var xoro = new Xoroshiro128Plus(seed);
        return xoro.Next();
    }
    public async Task<List<raidsprite>> raidlist()
    {

        List<raidsprite> returnlist = new();
        SAV9SV tempsave = (SAV9SV)sav;
        var raidpointer = new List<long> { 0x4384B18, 0x180, 0x40 };
        var raidoff = await botBase.PointerRelative(raidpointer);
        byte[] raiddata = await botBase.ReadBytesAsync((uint)raidoff, 0x910);

        var accessor = tempsave.Blocks;
        var raidblock = accessor.GetBlock(0xCAAC8800);
        raiddata.CopyTo(raidblock.Data, 0);
        var raidspawn = tempsave.Raid;
        var allraids = raidspawn.GetAllRaids();
        EncounterMight9 mencounter = null;
        EncounterDist9 dencounter = null;
        EncounterTera9 encounter = null;
        int i = 0;
        foreach (var raid in allraids)
        {

            switch (raid.Content)
            {
                case TeraRaidContentType.Distribution:

                    var game = GameVersion.SL;





                    //var dist = EncounterDist9.GetArray( tempsave.Blocks.GetBlock(0x520A1B0).Data);
                    foreach (var theencounter in Dist)
                    {
                        var maxd = game is GameVersion.SL ? theencounter.GetRandRateTotalScarlet(3) : theencounter.GetRandRateTotalViolet(3);
                        var min = game is GameVersion.SL ? theencounter.GetRandRateMinScarlet(3) : theencounter.GetRandRateMinViolet(3);
                        if (min >= 0 && maxd > 0)
                        {

                            var rateRandd = getxororandrate(raid.Seed, maxd, 5);
                            if ((uint)(rateRandd - min) < theencounter.RandRate)
                            {
                                dencounter = theencounter; break;
                            }

                        }
                    }
                    if (dencounter == null)
                        dencounter = Dist[1];
                    var param = new GenerateParam9()
                    {

                        GenderRatio = (byte)PersonalTable.SV.GetFormEntry(dencounter.Species, dencounter.Form).Gender,
                        FlawlessIVs = dencounter.FlawlessIVCount,
                        RollCount = 1,
                        Height = 0,
                        Weight = 0,
                        Ability = dencounter.Ability,
                        Shiny = dencounter.Shiny,
                        Nature = dencounter.Nature,
                        IVs = dencounter.IVs,
                    };
                    var pkr = new PK9
                    {
                        Species = dencounter.Species,
                        Form = dencounter.Form,
                        TrainerID7 = sav.TrainerID7,
                        TrainerSID7 = sav.TrainerSID7,
                        TeraTypeOriginal = (MoveType)Tera9RNG.GetTeraType(raid.Seed, dencounter.TeraType, dencounter.Species, dencounter.Form),
                    };

                    Encounter9RNG.GenerateData(pkr, param, EncounterCriteria.Unrestricted, raid.Seed, true);

                    returnlist.Add(new raidsprite(pkr, raid)); break;
                case TeraRaidContentType.Base05:
                    var starxoro = getxoronextint(raid.Seed, 100);
                    var stars = starxoro switch
                    {
                        <= 30 => 3,
                        <= 70 => 4,
                        > 70 => 5,
                    };
                    if (raid.Content == TeraRaidContentType.Black6)
                        stars = 6;
                    var game5 = GameVersion.SL;


                    var max = game5 is GameVersion.SL ? EncounterTera9.GetRateTotalBaseSL(stars) : EncounterTera9.GetRateTotalBaseVL(stars);

                    var rateRand = getxororandrate(raid.Seed, (uint)max, stars);

                    foreach (var theencounter in Tera)
                    {
                        var min = game5 is GameVersion.SL ? theencounter.RandRateMinScarlet : theencounter.RandRateMinViolet;
                        if (theencounter.Stars == stars && min >= 0 && (uint)(rateRand - min) < theencounter.RandRate)
                        {

                            encounter = theencounter;
                            break;
                        }
                    }
                    if (encounter == null)
                        encounter = Tera[1];
                    var param5 = new GenerateParam9()
                    {

                        GenderRatio = (byte)PersonalTable.SV.GetFormEntry(encounter.Species, encounter.Form).Gender,
                        FlawlessIVs = encounter.FlawlessIVCount,
                        RollCount = 1,
                        Height = 0,
                        Weight = 0,
                        Ability = encounter.Ability,
                        Shiny = encounter.Shiny,
                        Nature = encounter.Nature,
                        IVs = encounter.IVs,
                    };
                    var pk5 = new PK9
                    {
                        Species = encounter.Species,
                        Form = encounter.Form,
                        TrainerID7 = sav.TrainerID7,
                        TrainerSID7 = sav.TrainerSID7,
                        TeraTypeOriginal = (MoveType)Tera9RNG.GetTeraType(raid.Seed, encounter.TeraType, encounter.Species, encounter.Form),
                    };

                    Encounter9RNG.GenerateData(pk5, param5, EncounterCriteria.Unrestricted, raid.Seed);

                    returnlist.Add(new raidsprite(pk5, raid)); break;
                case TeraRaidContentType.Black6:
                    starxoro = getxoronextint(raid.Seed, 100);
                    stars = starxoro switch
                    {
                        <= 30 => 3,
                        <= 70 => 4,
                        > 70 => 5,
                    };
                    if (raid.Content == TeraRaidContentType.Black6)
                        stars = 6;
                    var game6 = GameVersion.SL;


                    max = game6 is GameVersion.SL ? EncounterTera9.GetRateTotalBaseSL(stars) : EncounterTera9.GetRateTotalBaseVL(stars);

                    rateRand = getxororandrate(raid.Seed, (uint)max, stars);

                    foreach (var theencounter in Tera)
                    {
                        var min = game6 is GameVersion.SL ? theencounter.RandRateMinScarlet : theencounter.RandRateMinViolet;
                        if (theencounter.Stars == stars && min >= 0 && (uint)(rateRand - min) < theencounter.RandRate)
                        {

                            encounter = theencounter;
                            break;
                        }
                    }
                    if (encounter == null)
                        encounter = Tera[1];
                    var param6 = new GenerateParam9()
                    {

                        GenderRatio = (byte)PersonalTable.SV.GetFormEntry(encounter.Species, encounter.Form).Gender,
                        FlawlessIVs = encounter.FlawlessIVCount,
                        RollCount = 1,
                        Height = 0,
                        Weight = 0,
                        Ability = encounter.Ability,
                        Shiny = encounter.Shiny,
                        Nature = encounter.Nature,
                        IVs = encounter.IVs,
                    };
                    var pk6 = new PK9
                    {
                        Species = encounter.Species,
                        Form = encounter.Form,
                        TrainerID7 = sav.TrainerID7,
                        TrainerSID7 = sav.TrainerSID7,
                        TeraTypeOriginal = (MoveType)Tera9RNG.GetTeraType(raid.Seed, encounter.TeraType, encounter.Species, encounter.Form),
                    };

                    Encounter9RNG.GenerateData(pk6, param6, EncounterCriteria.Unrestricted, raid.Seed);

                    returnlist.Add(new raidsprite(pk6, raid)); break;
                case TeraRaidContentType.Might7:
                    var game7 = GameVersion.SL;





                    //var dist = EncounterDist9.GetArray( tempsave.Blocks.GetBlock(0x520A1B0).Data);
                    foreach (var theencounter in Might)
                    {
                        var maxd = game7 is GameVersion.SL ? theencounter.RandRate3TotalScarlet : theencounter.GetRandRateTotalViolet(3);
                        var min = game7 is GameVersion.SL ? theencounter.GetRandRateMinScarlet(3) : theencounter.GetRandRateMinViolet(3);
                        if (min >= 0 && maxd > 0)
                        {

                            var rateRandd = getxororandrate(raid.Seed, (uint)maxd, 5);
                            if ((uint)(rateRandd - min) < theencounter.RandRate)
                            {
                                mencounter = theencounter; break;
                            }

                        }
                    }
                    if (mencounter == null)
                        mencounter = Might[1];
                    var param7 = new GenerateParam9()
                    {

                        GenderRatio = (byte)PersonalTable.SV.GetFormEntry(mencounter.Species, mencounter.Form).Gender,
                        FlawlessIVs = mencounter.FlawlessIVCount,
                        RollCount = 1,
                        Height = 0,
                        Weight = 0,
                        Ability = mencounter.Ability,
                        Shiny = mencounter.Shiny,
                        Nature = mencounter.Nature,
                        IVs = mencounter.IVs,
                    };
                    var pk7 = new PK9
                    {
                        Species = mencounter.Species,
                        Form = mencounter.Form,
                        TrainerID7 = sav.TrainerID7,
                        TrainerSID7 = sav.TrainerSID7,
                        TeraTypeOriginal = (MoveType)Tera9RNG.GetTeraType(raid.Seed, mencounter.TeraType, mencounter.Species, mencounter.Form),
                    };

                    Encounter9RNG.GenerateData(pk7, param7, EncounterCriteria.Unrestricted, raid.Seed);

                    returnlist.Add(new raidsprite(pk7, raid)); break;

            }
            if (i > 67)
                break;
            i++;
        }



        return returnlist;

    }

    private async void shinify(object sender, EventArgs e)
    {
        testlab.Text = "searching";


        ulong seed = (ulong)mainpk.Raid.Seed;
        while (!mainpk.pkm.IsShiny)
        {

            seed = getxoronextseed((uint)seed);
            switch (mainpk.Raid.Content)
            {
                case TeraRaidContentType.Base05:
                    int starxoro = getxoronextint((uint)seed, 100);
                    var stars = starxoro switch
                    {
                        <= 30 => 3,
                        <= 70 => 4,
                        > 70 => 5,
                    };
                    if (mainpk.Raid.Content == TeraRaidContentType.Black6)
                        stars = 6;
                    var game = GameVersion.SL;


                    var max = game is GameVersion.SL ? PKHeX.Core.EncounterTera9.GetRateTotalBaseSL(stars) : PKHeX.Core.EncounterTera9.GetRateTotalBaseVL(stars);

                    var rateRand = getxororandrate((uint)seed, (uint)max, stars);

                    PKHeX.Core.EncounterTera9 encounter = null;
                    foreach (var theencounter in Tera)
                    {
                        var min = game is GameVersion.SL ? theencounter.RandRateMinScarlet : theencounter.RandRateMinViolet;
                        if (theencounter.Stars == stars && min >= 0 && (uint)(rateRand - min) < theencounter.RandRate)
                        {
                            if ((Species)theencounter.Species == (Species)mainpk.pkm.Species)
                            {
                                encounter = theencounter;
                                break;
                            }
                        }
                    }

                    if (encounter == null)
                    {
                        // seed = seed;
                        continue;
                    }


                    var oid = getoid((uint)seed);
                    var pid = getpid((uint)seed);
                    uint num = pid ^ oid;
                    var xor = (num ^ (num >> 16)) & 0xFFFFu;
                    mainpk.pkm.SetIsShiny(xor < 16); break;

                case TeraRaidContentType.Black6:
                    starxoro = getxoronextint((uint)seed, 100);
                    stars = starxoro switch
                    {
                        <= 30 => 3,
                        <= 70 => 4,
                        > 70 => 5,
                    };
                    if (mainpk.Raid.Content == TeraRaidContentType.Black6)
                        stars = 6;
                    game = GameVersion.SL;


                    max = game is GameVersion.SL ? PKHeX.Core.EncounterTera9.GetRateTotalBaseSL(stars) : PKHeX.Core.EncounterTera9.GetRateTotalBaseVL(stars);

                    rateRand = getxororandrate((uint)seed, (uint)max, stars);

                    encounter = null;
                    foreach (var theencounter in Tera)
                    {
                        var min = game is GameVersion.SL ? theencounter.RandRateMinScarlet : theencounter.RandRateMinViolet;
                        if (theencounter.Stars == stars && min >= 0 && (uint)(rateRand - min) < theencounter.RandRate)
                        {
                            if ((Species)theencounter.Species == (Species)mainpk.pkm.Species)
                            {
                                encounter = theencounter;
                                break;
                            }
                        }
                    }

                    if (encounter == null)
                    {
                        // seed = seed;
                        continue;
                    }


                    oid = getoid((uint)seed);
                    pid = getpid((uint)seed);
                    num = pid ^ oid;
                    xor = (num ^ (num >> 16)) & 0xFFFFu;
                    mainpk.pkm.SetIsShiny(xor < 16); break;
                case TeraRaidContentType.Distribution:


                    game = GameVersion.SL;




                    EncounterDist9 dencounter = null;
                    //var dist = EncounterDist9.GetArray( tempsave.Blocks.GetBlock(0x520A1B0).Data);
                    foreach (var theencounter in Dist)
                    {
                        var maxd = game is GameVersion.SL ? theencounter.GetRandRateTotalScarlet(3) : theencounter.GetRandRateTotalViolet(3);
                        var min = game is GameVersion.SL ? theencounter.GetRandRateMinScarlet(3) : theencounter.GetRandRateMinViolet(3);
                        if (min >= 0 && maxd > 0)
                        {

                            var rateRandd = getxororandrate((uint)seed, maxd, theencounter.Stars);
                            if ((uint)(rateRandd - min) < theencounter.RandRate)
                            {
                                if ((Species)theencounter.Species == (Species)mainpk.pkm.Species)
                                {
                                    dencounter = theencounter;
                                    break;
                                }
                            }

                        }
                    }
                    if (dencounter == null)
                        continue;
                    oid = getoid((uint)seed);
                    pid = getpid((uint)seed);
                    num = pid ^ oid;
                    xor = (num ^ (num >> 16)) & 0xFFFFu;
                    mainpk.pkm.SetIsShiny(xor < 16); break;
                case TeraRaidContentType.Might7:

                    game = GameVersion.SL;




                    EncounterMight9 mencounter = null;
                    //var dist = EncounterDist9.GetArray( tempsave.Blocks.GetBlock(0x520A1B0).Data);
                    foreach (var theencounter in Might)
                    {
                        var maxd = game is GameVersion.SL ? theencounter.GetRandRateTotalScarlet(3) : theencounter.GetRandRateTotalViolet(3);
                        var min = game is GameVersion.SL ? theencounter.GetRandRateMinScarlet(3) : theencounter.GetRandRateMinViolet(3);
                        if (min >= 0 && maxd > 0)
                        {

                            var rateRandd = getxororandrate((uint)seed, maxd, 5);
                            if ((uint)(rateRandd - min) < theencounter.RandRate)
                            {
                                if ((Species)theencounter.Species == (Species)mainpk.pkm.Species)
                                {
                                    mencounter = theencounter;
                                    break;
                                }
                            }

                        }
                    }
                    if (mencounter == null)
                        continue;
                    oid = getoid((uint)seed);
                    pid = getpid((uint)seed);
                    num = pid ^ oid;
                    xor = (num ^ (num >> 16)) & 0xFFFFu;
                    mainpk.pkm.SetIsShiny(xor < 16); break;
            }
        }
        SAV9SV tempsave = (SAV9SV)sav;
        var raidspawn = tempsave.Raid;
        var allraids = raidspawn.GetAllRaids();
        int j = 0;
        foreach (var raid in allraids)
        {
            if (raid.Seed == mainpk.Raid.Seed)
            {
                raid.Seed = (uint)seed;

            }

        }
        var raidpointer = new List<long> { 0x4384B18, 0x180, 0x40 };
        var raidoff = await botBase.PointerRelative(raidpointer);
        await botBase.WriteBytesAsync(tempsave.Raid.Data, (uint)raidoff);
        testlab.Text = "Shinify";
    }

    private void update(object sender, SelectionChangedEventArgs e)
    {
        foreach (var s in e.CurrentSelection)
        {
            mainpk = (raidsprite)s;

        }
    }
    public async void teleport(object sender, EventArgs e)
    {

        var telporterpointer = new long[] { 0x43A75B0,0x2A8,0x0,0x0,0x80};
        
        var telporteroff = await botBase.PointerRelative(telporterpointer);
        var x = BitConverter.GetBytes((float)mainpk.coords[0]);
        float yarr;
        if (mainpk.Raid.AreaID != 2 || mainpk.Raid.AreaID != 5 || mainpk.Raid.AreaID != 6 || mainpk.Raid.AreaID != 7 || mainpk.Raid.AreaID != 8 || mainpk.Raid.AreaID != 9)
            yarr = (float)mainpk.coords[1] + 50;
        else
            yarr = (float)mainpk.coords[1] + 30;
        var y = BitConverter.GetBytes(yarr);
        var z = BitConverter.GetBytes((float)mainpk.coords[2]);
        var XYZ = x.Concat(y).ToArray();
        XYZ = XYZ.Concat(z).ToArray();
        await botBase.WriteBytesAsync(XYZ, (uint)telporteroff);
       
      
    }
    public static Color GetTypeSpriteColor(byte type)
    {
        return (sbyte)type switch
        {
            0 => Color.FromRgb(159, 161, 159),
            1 => Color.FromRgb(255, 128, 0),
            2 => Color.FromRgb(129, 185, 239),
            3 => Color.FromRgb(143, 65, 203),
            4 => Color.FromRgb(145, 81, 33),
            5 => Color.FromRgb(175, 169, 129),
            6 => Color.FromRgb(145, 161, 25),
            7 => Color.FromRgb(112, 65, 112),
            8 => Color.FromRgb(96, 161, 184),
            9 => Color.FromRgb(230, 40, 41),
            10 => Color.FromRgb(41, 128, 239),
            11 => Color.FromRgb(63, 161, 41),
            12 => Color.FromRgb(250, 192, 0),
            13 => Color.FromRgb(239, 65, 121),
            14 => Color.FromRgb(63, 216, 255),
            15 => Color.FromRgb(80, 97, 225),
            16 => Color.FromRgb(80, 65, 63),
            17 => Color.FromRgb(239, 113, 239),
            _ => throw new ArgumentOutOfRangeException("type", type, null),
        };

    }

    private async void takeascreenshot(object sender, EventArgs e)
    {
        sceenshotbutton.Text = "loading...";
        var screenarray = await botBase.Screengrab(CancellationToken.None);
        await File.WriteAllBytesAsync("/storage/emulated/0/Pictures/RaidScreenshot.jpg", screenarray);
        sceenshotbutton.Text = "screenshot";
    }
    public async Task initializedicts()
    {

        denloc = new Dictionary<string, double[]>
        {
            {
                "21-9",
                new double[3] {
                    3682.7138671875,
                    250.69371032714844,
                    -3523.624755859375
                }
            },
            {
                "16-4",
                new double[3] {
                    4098.39990234375,
                    40.29210662841797,
                    -2343.135986328125
                }
            },
            {
                "6-2",
                new double[3] {
                    1137.0035400390625,
                    408.0660400390625,
                    -1138.3153076171875
                }
            },
            {
                "19-9_",
                new double[3] {
                    2776.740478515625,
                    166.3944854736328,
                    -4743.41796875
                }
            },
            {
                "12-12",
                new double[3] {
                    2155.5,
                    184.75726318359375,
                    -3238.837890625
                }
            },
            {
                "7-9",
                new double[3] {
                    2725.94384765625,
                    126.64124298095703,
                    -1356.868408203125
                }
            },
            {
                "14-7",
                new double[3] {
                    4063.61962890625,
                    174.99179077148438,
                    -3204.710693359375
                }
            },
            {
                "12-16",
                new double[3] {
                    1692.9822998046875,
                    155.56199645996094,
                    -3032.88037109375
                }
            },
            {
                "8-15",
                new double[3] {
                    3119.893798828125,
                    110.41221618652344,
                    -1932.7418212890625
                }
            },
            {
                "15-4_",
                new double[3] {
                    3286.61376953125,
                    136.53489685058594,
                    -2210.962646484375
                }
            },
            {
                "7-2",
                new double[3] {
                    3162.503662109375,
                    117.25439453125,
                    -1504.7491455078125
                }
            },
            {
                "1-7",
                new double[3] {
                    2757.51611328125,
                    53.00542068481445,
                    -947.4797973632812
                }
            },
            {
                "1-8",
                new double[3] {
                    2940.730712890625,
                    51.828392028808594,
                    -891.1803588867188
                }
            },
            {
                "1-11",
                new double[3] {
                    2697.370361328125,
                    74.76914978027344,
                    -1060.09716796875
                }
            },
            {
                "4-5",
                new double[3] {
                    1553.404296875,
                    102.5262222290039,
                    -1469.782958984375
                }
            },
            {
                "4-1",
                new double[3] {
                    1899.160400390625,
                    131.08920288085938,
                    -1732.433837890625
                }
            },
            {
                "6-5",
                new double[3] {
                    679.8595581054688,
                    303.43792724609375,
                    -1165.469482421875
                }
            },
            {
                "6-1",
                new double[3] {
                    1215.52001953125,
                    439.1628723144531,
                    -1254.981689453125
                }
            },
            {
                "9-14",
                new double[3] {
                    978.5487670898438,
                    231.2777099609375,
                    -1449.6282958984375
                }
            },
            {
                "5-8",
                new double[3] {
                    1159.606689453125,
                    103.91116333007812,
                    -1402.3223876953125
                }
            },
            {
                "4-9",
                new double[3] {
                    1475.571533203125,
                    124.27598571777344,
                    -1911.5458984375
                }
            },
            {
                "9-4",
                new double[3] {
                    1083.04052734375,
                    191.4821319580078,
                    -2093.066162109375
                }
            },
            {
                "9-1",
                new double[3] {
                    703.1109619140625,
                    317.58984375,
                    -2202.5263671875
                }
            },
            {
                "9-16",
                new double[3] {
                    554.13818359375,
                    309.96929931640625,
                    -2122.413330078125
                }
            },
            {
                "9-6_",
                new double[3] {
                    1288.543212890625,
                    236.87100219726562,
                    -2472.98583984375
                }
            },
            {
                "10-5",
                new double[3] {
                    967.3399047851562,
                    154.0867462158203,
                    -2576.123779296875
                }
            },
            {
                "10-2",
                new double[3] {
                    934.2491455078125,
                    154.18565368652344,
                    -3069.734130859375
                }
            },
            {
                "11-3",
                new double[3] {
                    458.5235290527344,
                    28.70948600769043,
                    -3199.963134765625
                }
            },
            {
                "10-9",
                new double[3] {
                    408.50433349609375,
                    153.78384399414062,
                    -2811.291259765625
                }
            },
            {
                "11-6",
                new double[3] {
                    914.6806640625,
                    69.37954711914062,
                    -3468.398681640625
                }
            },
            {
                "11-5",
                new double[3] {
                    641.4882202148438,
                    52.14754104614258,
                    -3602.66015625
                }
            },
            {
                "12-3",
                new double[3] {
                    1730.2315673828125,
                    102.7404556274414,
                    -3720.3984375
                }
            },
            {
                "18-10",
                new double[3] {
                    1640.6337890625,
                    134.85496520996094,
                    -4364.7109375
                }
            },
            {
                "18-9",
                new double[3] {
                    1505.6724853515625,
                    107.27595520019531,
                    -4404.1611328125
                }
            },
            {
                "19-2",
                new double[3] {
                    2076.33056640625,
                    197.01681518554688,
                    -4451.93408203125
                }
            },
            {
                "20-6",
                new double[3] {
                    1968.591552734375,
                    107.09345245361328,
                    -4706.95263671875
                }
            },
            {
                "20-7",
                new double[3] {
                    1889.7476806640625,
                    27.639942169189453,
                    -4804.30712890625
                }
            },
            {
                "20-4",
                new double[3] {
                    2404.298828125,
                    39.00518035888672,
                    -4737.60205078125
                }
            },
            {
                "19-10",
                new double[3] {
                    2553.3095703125,
                    459.0953369140625,
                    -3786.40283203125
                }
            },
            {
                "17-10",
                new double[3] {
                    3060.15576171875,
                    529.2732543945312,
                    -3662.169677734375
                }
            },
            {
                "17-8",
                new double[3] {
                    3304.818359375,
                    457.1964416503906,
                    -3818.607421875
                }
            },
            {
                "19-5_",
                new double[3] {
                    2518.13037109375,
                    312.2546691894531,
                    -4326.14111328125
                }
            },
            {
                "19-5",
                new double[3] {
                    3287.599853515625,
                    409.3759460449219,
                    -4118.67822265625
                }
            },
            {
                "19-8_",
                new double[3] {
                    2933.6494140625,
                    294.987548828125,
                    -4489.7802734375
                }
            },
            {
                "21-10",
                new double[3] {
                    3438.7041015625,
                    200.2812957763672,
                    -3585.455810546875
                }
            },
            {
                "13-2",
                new double[3] {
                    3415.8193359375,
                    165.1154327392578,
                    -3544.62744140625
                }
            },
            {
                "13-1",
                new double[3] {
                    3528.427001953125,
                    165.16822814941406,
                    -3542.313232421875
                }
            },
            {
                "21-5",
                new double[3] {
                    3782.30908203125,
                    215.0296630859375,
                    -3941.70654296875
                }
            },
            {
                "22-4",
                new double[3] {
                    4005.251953125,
                    249.03334045410156,
                    -3800.29541015625
                }
            },
            {
                "22-10",
                new double[3] {
                    4043.005126953125,
                    249.94529724121094,
                    -3479.774169921875
                }
            },
            {
                "22-9",
                new double[3] {
                    4231.9833984375,
                    230.2823028564453,
                    -3484.5703125
                }
            },
            {
                "14-9",
                new double[3] {
                    3733.7158203125,
                    152.13026428222656,
                    -2826.691162109375
                }
            },
            {
                "14-6",
                new double[3] {
                    4079.509521484375,
                    151.98397827148438,
                    -3084.779296875
                }
            },
            {
                "15-1_",
                new double[3] {
                    3505.80029296875,
                    114.15591430664062,
                    -2305.112548828125
                }
            },
            {
                "15-2",
                new double[3] {
                    3370.668701171875,
                    131.46180725097656,
                    -2432.503173828125
                }
            },
            {
                "16-8",
                new double[3] {
                    3549.198486328125,
                    116.0923080444336,
                    -2506.169677734375
                }
            },
            {
                "16-6",
                new double[3] {
                    4020.930908203125,
                    28.21793556213379,
                    -2547.00537109375
                }
            },
            {
                "5-3",
                new double[3] {
                    2140.05029296875,
                    154.71087646484375,
                    -1307.365966796875
                }
            },
            {
                "5-1",
                new double[3] {
                    1868.1063232421875,
                    123.81390380859375,
                    -1160.320556640625
                }
            },
            {
                "7-1",
                new double[3] {
                    3260.0517578125,
                    139.03958129882812,
                    -1664.84716796875
                }
            },
            {
                "8-7",
                new double[3] {
                    2870.009521484375,
                    149.5354461669922,
                    -1747.124755859375
                }
            },
            {
                "8-5",
                new double[3] {
                    2897.840087890625,
                    135.16934204101562,
                    -1629.7481689453125
                }
            },
            {
                "9-5",
                new double[3] {
                    949.016357421875,
                    205.28765869140625,
                    -2092.015869140625
                }
            },
            {
                "13-3",
                new double[3] {
                    3214.967041015625,
                    164.55772399902344,
                    -3475.13232421875
                }
            },
            {
                "15-9",
                new double[3] {
                    4449.3818359375,
                    39.88765335083008,
                    -1995.7310791015625
                }
            },
            {
                "15-8",
                new double[3] {
                    4346.45703125,
                    101.91188049316406,
                    -2056.72216796875
                }
            },
            {
                "15-5",
                new double[3] {
                    4090.06982421875,
                    78.72682189941406,
                    -2140.98193359375
                }
            },
            {
                "18-3",
                new double[3] {
                    538.864990234375,
                    59.06913757324219,
                    -4296.1669921875
                }
            },
            {
                "17-6",
                new double[3] {
                    2287.58544921875,
                    273.86834716796875,
                    -3155.218017578125
                }
            },
            {
                "5-17",
                new double[3] {
                    1932.8270263671875,
                    62.656211853027344,
                    -866.1071166992188
                }
            },
            {
                "9-8",
                new double[3] {
                    1090.183349609375,
                    282.9974365234375,
                    -2287.44775390625
                }
            },
            {
                "13-5",
                new double[3] {
                    3117.244140625,
                    138.7787628173828,
                    -3314.323486328125
                }
            },
            {
                "4-3",
                new double[3] {
                    1790.699462890625,
                    170.19273376464844,
                    -1969.0260009765625
                }
            },
            {
                "11-11",
                new double[3] {
                    1420.6207275390625,
                    107.87718963623047,
                    -3378.29736328125
                }
            },
            {
                "11-4",
                new double[3] {
                    575.8120727539062,
                    27.262725830078125,
                    -3590.307373046875
                }
            },
            {
                "6-9",
                new double[3] {
                    756.291015625,
                    277.506103515625,
                    -1449.158935546875
                }
            },
            {
                "6-10",
                new double[3] {
                    705.9667358398438,
                    49.61960220336914,
                    -1508.3389892578125
                }
            },
            {
                "6-7",
                new double[3] {
                    697.6875610351562,
                    289.4073791503906,
                    -1282.9647216796875
                }
            },
            {
                "15-11",
                new double[3] {
                    3075.80908203125,
                    170.83840942382812,
                    -2082.50830078125
                }
            },
            {
                "1-6",
                new double[3] {
                    2706.399658203125,
                    38.40924072265625,
                    -896.2372436523438
                }
            },
            {
                "1-9",
                new double[3] {
                    2903.0771484375,
                    43.3607292175293,
                    -956.45458984375
                }
            },
            {
                "4-7",
                new double[3] {
                    1630.3037109375,
                    123.85369110107422,
                    -1501.316650390625
                }
            },
            {
                "5-5",
                new double[3] {
                    1704.212890625,
                    99.97447204589844,
                    -1272.4688720703125
                }
            },
            {
                "5-2",
                new double[3] {
                    2001.416259765625,
                    123.07888793945312,
                    -1167.27392578125
                }
            },
            {
                "7-13",
                new double[3] {
                    3221.201171875,
                    50.31945037841797,
                    -1203.0789794921875
                }
            },
            {
                "7-8",
                new double[3] {
                    2745.7978515625,
                    89.88677215576172,
                    -1133.404296875
                }
            },
            {
                "8-4",
                new double[3] {
                    2845.95947265625,
                    116.14042663574219,
                    -1648.4124755859375
                }
            },
            {
                "8-8",
                new double[3] {
                    2858.086181640625,
                    153.32460021972656,
                    -1715.0218505859375
                }
            },
            {
                "9-11",
                new double[3] {
                    1391.70556640625,
                    241.65773010253906,
                    -2588.3349609375
                }
            },
            {
                "9-7",
                new double[3] {
                    1037.7032470703125,
                    157.78436279296875,
                    -1760.611083984375
                }
            },
            {
                "9-9",
                new double[3] {
                    1122.1995849609375,
                    247.47457885742188,
                    -2442.93115234375
                }
            },
            {
                "9-7_",
                new double[3] {
                    1612.12646484375,
                    194.54722595214844,
                    -3009.384033203125
                }
            },
            {
                "10-8",
                new double[3] {
                    709.9904174804688,
                    151.18959045410156,
                    -2633.322021484375
                }
            },
            {
                "10-6",
                new double[3] {
                    968.0906372070312,
                    205.7713623046875,
                    -2746.34375
                }
            },
            {
                "12-2",
                new double[3] {
                    1492.70654296875,
                    126.80134582519531,
                    -3376.07421875
                }
            },
            {
                "12-15",
                new double[3] {
                    1825.818359375,
                    160.84909057617188,
                    -3003.546142578125
                }
            },
            {
                "12-13",
                new double[3] {
                    1966.613525390625,
                    227.8946533203125,
                    -2886.77392578125
                }
            },
            {
                "13-4",
                new double[3] {
                    3338.171142578125,
                    149.7206268310547,
                    -3179.464599609375
                }
            },
            {
                "14-15",
                new double[3] {
                    3222.493896484375,
                    137.0845947265625,
                    -2716.77734375
                }
            },
            {
                "14-8",
                new double[3] {
                    3912.251708984375,
                    110.19703674316406,
                    -2703.46728515625
                }
            },
            {
                "14-5",
                new double[3] {
                    4472.6435546875,
                    97.86649322509766,
                    -3154.583740234375
                }
            },
            {
                "15-7",
                new double[3] {
                    4199.365234375,
                    101.7031021118164,
                    -2062.15625
                }
            },
            {
                "15-6",
                new double[3] {
                    4181.24169921875,
                    100.48638916015625,
                    -1999.3677978515625
                }
            },
            {
                "15-8_",
                new double[3] {
                    3143.298583984375,
                    199.99327087402344,
                    -2208.426513671875
                }
            },
            {
                "15-6_",
                new double[3] {
                    3255.528076171875,
                    199.9345245361328,
                    -2352.402587890625
                }
            },
            {
                "16-10",
                new double[3] {
                    3465.406494140625,
                    121.92607879638672,
                    -2658.3076171875
                }
            },
            {
                "17-11",
                new double[3] {
                    2942.475830078125,
                    199.92799377441406,
                    -2808.220703125
                }
            },
            {
                "18-7",
                new double[3] {
                    1156.31005859375,
                    105.53359985351562,
                    -4631.71875
                }
            },
            {
                "18-8",
                new double[3] {
                    1314.765625,
                    112.1968002319336,
                    -4412.47119140625
                }
            },
            {
                "19-7",
                new double[3] {
                    2492.178955078125,
                    380.3725280761719,
                    -4158.70947265625
                }
            },
            {
                "19-12",
                new double[3] {
                    2080.827880859375,
                    250.62600708007812,
                    -4260.583984375
                }
            },
            {
                "19-6",
                new double[3] {
                    2388.054443359375,
                    193.60595703125,
                    -4381.07763671875
                }
            },
            {
                "20-2",
                new double[3] {
                    2095.999267578125,
                    30.419109344482422,
                    -4828.11767578125
                }
            },
            {
                "20-5",
                new double[3] {
                    2635.512939453125,
                    31.763891220092773,
                    -4765.5400390625
                }
            },
            {
                "21-11",
                new double[3] {
                    3528.43408203125,
                    252.74240112304688,
                    -3703.2431640625
                }
            },
            {
                "21-1",
                new double[3] {
                    3546.78466796875,
                    247.76829528808594,
                    -4268.35400390625
                }
            },
            {
                "21-7",
                new double[3] {
                    4038.472412109375,
                    308.2213134765625,
                    -3873.480712890625
                }
            },
            {
                "22-19",
                new double[3] {
                    4382.82421875,
                    318.7710876464844,
                    -3325.590576171875
                }
            },
            {
                "22-18",
                new double[3] {
                    4397.951171875,
                    294.8275146484375,
                    -3414.289306640625
                }
            },
            {
                "12-1",
                new double[3] {
                    1619.9002685546875,
                    151.42730712890625,
                    -3347.86181640625
                }
            },
            {
                "6-4",
                new double[3] {
                    793.8597412109375,
                    304.8221740722656,
                    -960.2905883789062
                }
            },
            {
                "19-4_",
                new double[3] {
                    3335.52734375,
                    290.1728820800781,
                    -4285.78271484375
                }
            },
            {
                "7-4",
                new double[3] {
                    3301.167724609375,
                    100.06945037841797,
                    -1386.5430908203125
                }
            },
            {
                "15-10",
                new double[3] {
                    3382.005126953125,
                    174.95901489257812,
                    -2026.360107421875
                }
            },
            {
                "4-10",
                new double[3] {
                    1813.855224609375,
                    136.8524169921875,
                    -1501.220947265625
                }
            },
            {
                "4-6",
                new double[3] {
                    1836.744140625,
                    118.98665618896484,
                    -1423.7471923828125
                }
            },
            {
                "4-12",
                new double[3] {
                    1983.7095947265625,
                    125.60093688964844,
                    -1595.840576171875
                }
            },
            {
                "5-15",
                new double[3] {
                    1759.078369140625,
                    119.88997650146484,
                    -1176.78466796875
                }
            },
            {
                "5-11",
                new double[3] {
                    1386.0947265625,
                    181.9459686279297,
                    -1205.365234375
                }
            },
            {
                "6-11",
                new double[3] {
                    619.6458740234375,
                    30.090402603149414,
                    -1322.65478515625
                }
            },
            {
                "7-6",
                new double[3] {
                    3612.028564453125,
                    313.45233154296875,
                    -1468.4107666015625
                }
            },
            {
                "7-7",
                new double[3] {
                    3639.058349609375,
                    295.4045104980469,
                    -1283.8338623046875
                }
            },
            {
                "8-11",
                new double[3] {
                    3043.97607421875,
                    153.6007537841797,
                    -1807.08203125
                }
            },
            {
                "9-6",
                new double[3] {
                    1268.7440185546875,
                    274.8403015136719,
                    -2192.03955078125
                }
            },
            {
                "10-7",
                new double[3] {
                    752.1790771484375,
                    150.0486297607422,
                    -2981.216796875
                }
            },
            {
                "11-2",
                new double[3] {
                    759.6878051757812,
                    97.10235595703125,
                    -3208.26318359375
                }
            },
            {
                "12-6",
                new double[3] {
                    1877.3553466796875,
                    150.120361328125,
                    -3345.380126953125
                }
            },
            {
                "14-11",
                new double[3] {
                    3500.316650390625,
                    139.98573303222656,
                    -3186.513671875
                }
            },
            {
                "14-3",
                new double[3] {
                    4255.0615234375,
                    98.47891235351562,
                    -3075.083740234375
                }
            },
            {
                "16-1",
                new double[3] {
                    3635.112060546875,
                    103.54830169677734,
                    -2250.980712890625
                }
            },
            {
                "15-10_",
                new double[3] {
                    4307.92919921875,
                    82.63484191894531,
                    -2250.12744140625
                }
            },
            {
                "16-2",
                new double[3] {
                    3914.484130859375,
                    86.94561767578125,
                    -2160.419677734375
                }
            },
            {
                "16-7",
                new double[3] {
                    3663.356689453125,
                    101.762939453125,
                    -2398.358154296875
                }
            },
            {
                "17-9",
                new double[3] {
                    3154.939453125,
                    549.3114624023438,
                    -3886.890625
                }
            },
            {
                "17-4",
                new double[3] {
                    2599.610107421875,
                    350.1400146484375,
                    -3410.1650390625
                }
            },
            {
                "17-3",
                new double[3] {
                    2593.080810546875,
                    350.79730224609375,
                    -3280.489013671875
                }
            },
            {
                "18-5",
                new double[3] {
                    787.514404296875,
                    30.536602020263672,
                    -4486.11376953125
                }
            },
            {
                "18-1",
                new double[3] {
                    583.0767211914062,
                    51.881324768066406,
                    -3724.06005859375
                }
            },
            {
                "19-11",
                new double[3] {
                    2637.7255859375,
                    473.9161376953125,
                    -4079.393310546875
                }
            },
            {
                "19-3_",
                new double[3] {
                    3214.6123046875,
                    249.58834838867188,
                    -4425.31689453125
                }
            },
            {
                "20-3",
                new double[3] {
                    2267.61376953125,
                    27.225128173828125,
                    -4887.87158203125
                }
            },
            {
                "22-2",
                new double[3] {
                    3948.685546875,
                    220.00645446777344,
                    -3739.37548828125
                }
            },
            {
                "22-17",
                new double[3] {
                    4200.57568359375,
                    250.41587829589844,
                    -3389.220947265625
                }
            },
            {
                "22-20",
                new double[3] {
                    4421.337890625,
                    250.142333984375,
                    -3663.361572265625
                }
            },
            {
                "9-11_",
                new double[3] {
                    1391.673828125,
                    241.65499877929688,
                    -2588.400146484375
                }
            },
            {
                "22-15",
                new double[3] {
                    4110.6279296875,
                    229.98350524902344,
                    -3351.466064453125
                }
            },
            {
                "22-5",
                new double[3] {
                    4148.85498046875,
                    249.84085083007812,
                    -3793.45947265625
                }
            },
            {
                "18-15",
                new double[3] {
                    855.2330322265625,
                    57.124969482421875,
                    -3998.38232421875
                }
            },
            {
                "18-6",
                new double[3] {
                    880.1807861328125,
                    96.9754638671875,
                    -4444.51416015625
                }
            },
            {
                "1-1",
                new double[3] {
                    2846.90869140625,
                    36.536312103271484,
                    -1038.77734375
                }
            },
            {
                "1-2",
                new double[3] {
                    2843.7724609375,
                    50.575477600097656,
                    -1154.3944091796875
                }
            },
            {
                "4-11",
                new double[3] {
                    1772.280517578125,
                    100.31053161621094,
                    -1352.475830078125
                }
            },
            {
                "5-13",
                new double[3] {
                    1570.7540283203125,
                    119.7640609741211,
                    -1122.13916015625
                }
            },
            {
                "7-11",
                new double[3] {
                    3097.009765625,
                    97.1158218383789,
                    -962.1580200195312
                }
            },
            {
                "8-14",
                new double[3] {
                    3329.21044921875,
                    159.39956665039062,
                    -1926.2843017578125
                }
            },
            {
                "8-2",
                new double[3] {
                    2704.015380859375,
                    100.1576919555664,
                    -1767.9693603515625
                }
            },
            {
                "9-10",
                new double[3] {
                    1852.8626708984375,
                    153.5908966064453,
                    -2017.8587646484375
                }
            },
            {
                "10-4",
                new double[3] {
                    1167.5147705078125,
                    165.62548828125,
                    -2808.93408203125
                }
            },
            {
                "11-1",
                new double[3] {
                    679.5067138671875,
                    72.32976531982422,
                    -3023.509765625
                }
            },
            {
                "14-13",
                new double[3] {
                    3424.524658203125,
                    154.60342407226562,
                    -2858.23681640625
                }
            },
            {
                "14-1",
                new double[3] {
                    4158.06396484375,
                    75.49700927734375,
                    -2839.81396484375
                }
            },
            {
                "15-3_",
                new double[3] {
                    3316.5654296875,
                    136.46728515625,
                    -2552.649169921875
                }
            },
            {
                "17-5",
                new double[3] {
                    2427.291748046875,
                    252.58656311035156,
                    -3460.5283203125
                }
            },
            {
                "19-1",
                new double[3] {
                    1903.984375,
                    301.61175537109375,
                    -4475.76953125
                }
            },
            {
                "21-13",
                new double[3] {
                    3968.943603515625,
                    206.36614990234375,
                    -4036.313232421875
                }
            },
            {
                "21-4",
                new double[3] {
                    3663.609619140625,
                    250.19764709472656,
                    -3851.781005859375
                }
            },
            {
                "9-19",
                new double[3] {
                    408.1226501464844,
                    345.7562561035156,
                    -2296.19287109375
                }
            },
            {
                "17-1",
                new double[3] {
                    2904.093017578125,
                    200.31735229492188,
                    -3093.521240234375
                }
            },
            {
                "1-10",
                new double[3] {
                    2757.242431640625,
                    28.23625373840332,
                    -886.7237548828125
                }
            },
            {
                "1-5",
                new double[3] {
                    3027.603515625,
                    47.93217849731445,
                    -826.7185668945312
                }
            },
            {
                "4-4",
                new double[3] {
                    1638.1390380859375,
                    150.91549682617188,
                    -1933.9835205078125
                }
            },
            {
                "5-14",
                new double[3] {
                    1658.1109619140625,
                    124.05718231201172,
                    -1121.5230712890625
                }
            },
            {
                "6-8",
                new double[3] {
                    1252.813232421875,
                    376.17828369140625,
                    -1132.8048095703125
                }
            },
            {
                "7-3",
                new double[3] {
                    2904.132080078125,
                    126.45990753173828,
                    -1408.8675537109375
                }
            },
            {
                "7-5",
                new double[3] {
                    3396.37939453125,
                    301.3248291015625,
                    -1422.8177490234375
                }
            },
            {
                "8-6",
                new double[3] {
                    2937.051025390625,
                    119.73970031738281,
                    -1624.453125
                }
            },
            {
                "8-3",
                new double[3] {
                    2828.6748046875,
                    110.54315185546875,
                    -1628.775390625
                }
            },
            {
                "9-20",
                new double[3] {
                    461.9130859375,
                    260.9601135253906,
                    -2431.696533203125
                }
            },
            {
                "9-2",
                new double[3] {
                    1197.802490234375,
                    160.38417053222656,
                    -1938.9688720703125
                }
            },
            {
                "10-10",
                new double[3] {
                    796.4733276367188,
                    206.46458435058594,
                    -2831.756591796875
                }
            },
            {
                "10-1",
                new double[3] {
                    1355.43115234375,
                    153.9207763671875,
                    -3069.1220703125
                }
            },
            {
                "11-7",
                new double[3] {
                    1079.5811767578125,
                    68.38652801513672,
                    -3505.07373046875
                }
            },
            {
                "12-17",
                new double[3] {
                    1519.1287841796875,
                    162.80059814453125,
                    -3264.474365234375
                }
            },
            {
                "12-5",
                new double[3] {
                    2054.184814453125,
                    150.55355834960938,
                    -3465.873046875
                }
            },
            {
                "12-9",
                new double[3] {
                    2256.93017578125,
                    181.12899780273438,
                    -3808.772216796875
                }
            },
            {
                "14-10",
                new double[3] {
                    3670.75439453125,
                    149.99673461914062,
                    -3300.8271484375
                }
            },
            {
                "16-9",
                new double[3] {
                    3579.82275390625,
                    133.68809509277344,
                    -2362.6630859375
                }
            },
            {
                "18-12",
                new double[3] {
                    2071.660400390625,
                    140.55845642089844,
                    -3939.57080078125
                }
            },
            {
                "18-2",
                new double[3] {
                    761.422119140625,
                    83.24877166748047,
                    -4136.70654296875
                }
            },
            {
                "19-4",
                new double[3] {
                    2355.997802734375,
                    292.99383544921875,
                    -4155.810546875
                }
            },
            {
                "21-6",
                new double[3] {
                    4237.70068359375,
                    199.9582977294922,
                    -4024.8779296875
                }
            },
            {
                "21-12",
                new double[3] {
                    3610.37109375,
                    302.0010681152344,
                    -4050.401611328125
                }
            },
            {
                "22-16",
                new double[3] {
                    4250.89501953125,
                    334.3514099121094,
                    -3341.34326171875
                }
            },
            {
                "22-13",
                new double[3] {
                    3944.524658203125,
                    250.09779357910156,
                    -3403.35546875
                }
            },
            {
                "6-3",
                new double[3] {
                    1011.1431274414062,
                    323.75274658203125,
                    -1119.1829833984375
                }
            },
            {
                "18-14",
                new double[3] {
                    1061.146728515625,
                    53.69176483154297,
                    -4229.298828125
                }
            },
            {
                "5-12",
                new double[3] {
                    1494.5037841796875,
                    137.06991577148438,
                    -1213.8289794921875
                }
            },
            {
                "5-21",
                new double[3] {
                    2309.393310546875,
                    102.54625701904297,
                    -868.9888916015625
                }
            },
            {
                "7-12",
                new double[3] {
                    3233.765869140625,
                    60.11518478393555,
                    -958.97509765625
                }
            },
            {
                "8-16",
                new double[3] {
                    3229.30126953125,
                    159.92724609375,
                    -1981.9730224609375
                }
            },
            {
                "8-9",
                new double[3] {
                    2945.63623046875,
                    144.4078369140625,
                    -1708.4173583984375
                }
            },
            {
                "11-10",
                new double[3] {
                    1405.4400634765625,
                    106.62788391113281,
                    -3595.18310546875
                }
            },
            {
                "12-4",
                new double[3] {
                    1841.7470703125,
                    100.40947723388672,
                    -3609.025146484375
                }
            },
            {
                "16-5",
                new double[3] {
                    4281.935546875,
                    27.89804458618164,
                    -2319.869140625
                }
            },
            {
                "17-12",
                new double[3] {
                    3019.524169921875,
                    199.9866943359375,
                    -2662.12060546875
                }
            },
            {
                "21-3",
                new double[3] {
                    3714.67236328125,
                    30.077495574951172,
                    -4395.58544921875
                }
            },
            {
                "22-14",
                new double[3] {
                    4038.675537109375,
                    302.3912353515625,
                    -3358.335693359375
                }
            },
            {
                "1-4",
                new double[3] {
                    3026.280517578125,
                    59.074859619140625,
                    -861.8172607421875
                }
            },
            {
                "5-18",
                new double[3] {
                    1816.613037109375,
                    50.68669509887695,
                    -1020.5359497070312
                }
            },
            {
                "5-20",
                new double[3] {
                    1740.3387451171875,
                    27.364282608032227,
                    -908.1990356445312
                }
            },
            {
                "5-9",
                new double[3] {
                    1355.391357421875,
                    125.78621673583984,
                    -1445.5191650390625
                }
            },
            {
                "7-10",
                new double[3] {
                    3134.3642578125,
                    84.23871612548828,
                    -1133.4775390625
                }
            },
            {
                "8-18",
                new double[3] {
                    3601.846923828125,
                    159.89344787597656,
                    -1845.0460205078125
                }
            },
            {
                "8-1",
                new double[3] {
                    2788.20556640625,
                    118.9881362915039,
                    -1641.921630859375
                }
            },
            {
                "9-3",
                new double[3] {
                    806.8272705078125,
                    261.9290466308594,
                    -2293.163818359375
                }
            },
            {
                "15-4",
                new double[3] {
                    3994.764404296875,
                    106.66231536865234,
                    -2073.998291015625
                }
            },
            {
                "16-3",
                new double[3] {
                    3989.941650390625,
                    68.23043823242188,
                    -2367.8095703125
                }
            },
            {
                "19-9",
                new double[3] {
                    2710.0390625,
                    543.1600952148438,
                    -3814.742919921875
                }
            },
            {
                "20-1",
                new double[3] {
                    2157.9658203125,
                    76.08732604980469,
                    -4623.33251953125
                }
            },
            {
                "22-8",
                new double[3] {
                    4208.36279296875,
                    229.985107421875,
                    -3633.688720703125
                }
            },
            {
                "5-16",
                new double[3] {
                    1896.23779296875,
                    77.3264389038086,
                    -1011.1201782226562
                }
            },
            {
                "11-8",
                new double[3] {
                    979.668212890625,
                    100.10125732421875,
                    -3297.00439453125
                }
            },
            {
                "8-13",
                new double[3] {
                    3156.23583984375,
                    100.039306640625,
                    -1761.2510986328125
                }
            },
            {
                "8-12",
                new double[3] {
                    3112.006591796875,
                    132.38430786132812,
                    -1870.888427734375
                }
            },
            {
                "14-14",
                new double[3] {
                    3198.945068359375,
                    139.9900360107422,
                    -2965.341552734375
                }
            },
            {
                "15-1",
                new double[3] {
                    3948.162841796875,
                    128.161376953125,
                    -1752.7305908203125
                }
            },
            {
                "19-7_",
                new double[3] {
                    2926.599853515625,
                    37.64940643310547,
                    -4820.283203125
                }
            },
            {
                "19-8",
                new double[3] {
                    2537.696044921875,
                    314.4533996582031,
                    -3934.822998046875
                }
            },
            {
                "21-2",
                new double[3] {
                    3514.838134765625,
                    27.494876861572266,
                    -4552.2451171875
                }
            },
            {
                "8-17",
                new double[3] {
                    3450.452392578125,
                    156.8411865234375,
                    -1866.84375
                }
            },
            {
                "12-11",
                new double[3] {
                    2104.7041015625,
                    199.9827880859375,
                    -3110.7939453125
                }
            },
            {
                "11-9",
                new double[3] {
                    1279.9873046875,
                    72.72835540771484,
                    -3442.228515625
                }
            },
            {
                "4-8",
                new double[3] {
                    1641.9818115234375,
                    122.40616607666016,
                    -1662.822021484375
                }
            },
            {
                "5-7",
                new double[3] {
                    1379.0257568359375,
                    99.87844848632812,
                    -1413.515625
                }
            },
            {
                "9-21",
                new double[3] {
                    614.1968994140625,
                    204.35049438476562,
                    -2568.22705078125
                }
            },
            {
                "9-18",
                new double[3] {
                    516.776123046875,
                    258.8527526855469,
                    -1989.3729248046875
                }
            },
            {
                "18-4",
                new double[3] {
                    840.140869140625,
                    72.20166778564453,
                    -4295.62158203125
                }
            },
            {
                "14-4",
                new double[3] {
                    4555.35498046875,
                    131.98587036132812,
                    -3100.103515625
                }
            },
            {
                "15-9_",
                new double[3] {
                    3482.981689453125,
                    149.99407958984375,
                    -2138.993896484375
                }
            },
            {
                "19-3",
                new double[3] {
                    2175.38720703125,
                    258.82196044921875,
                    -3963.779296875
                }
            },
            {
                "19-1_",
                new double[3] {
                    2979.329833984375,
                    468.2066345214844,
                    -4081.130615234375
                }
            },
            {
                "8-10",
                new double[3] {
                    2778.900390625,
                    108.85709381103516,
                    -1814.2647705078125
                }
            },
            {
                "1-3",
                new double[3] {
                    2994.230224609375,
                    50.014434814453125,
                    -1044.3048095703125
                }
            },
            {
                "6-6",
                new double[3] {
                    850.1961669921875,
                    300.372802734375,
                    -1259.2109375
                }
            },
            {
                "9-13",
                new double[3] {
                    739.5475463867188,
                    276.4090881347656,
                    -1975.1114501953125
                }
            },
            {
                "12-14",
                new double[3] {
                    1789.4888916015625,
                    198.5312042236328,
                    -2888.823486328125
                }
            },
            {
                "12-7",
                new double[3] {
                    2242.154541015625,
                    163.0216064453125,
                    -3495.465576171875
                }
            },
            {
                "4-2",
                new double[3] {
                    1770.86181640625,
                    170.50949096679688,
                    -1850.381591796875
                }
            },
            {
                "5-19",
                new double[3] {
                    1712.217529296875,
                    51.87508010864258,
                    -982.8257446289062
                }
            },
            {
                "9-4_",
                new double[3] {
                    726.6002197265625,
                    250.4230499267578,
                    -2315.256103515625
                }
            },
            {
                "14-12",
                new double[3] {
                    3495.6943359375,
                    171.3454132080078,
                    -2946.034423828125
                }
            },
            {
                "15-7_",
                new double[3] {
                    3043.49462890625,
                    200.01553344726562,
                    -2326.065673828125
                }
            },
            {
                "17-2",
                new double[3] {
                    2721.36376953125,
                    280.00830078125,
                    -3200.578125
                }
            },
            {
                "18-11",
                new double[3] {
                    2003.955810546875,
                    144.0991668701172,
                    -4005.120361328125
                }
            },
            {
                "22-21",
                new double[3] {
                    4656.81005859375,
                    202.27635192871094,
                    -3247.201171875
                }
            },
            {
                "22-3",
                new double[3] {
                    3998.7294921875,
                    220.01324462890625,
                    -3643.839599609375
                }
            },
            {
                "8-19",
                new double[3] {
                    3543.3095703125,
                    124.87462615966797,
                    -1750.4053955078125
                }
            },
            {
                "10-3",
                new double[3] {
                    1089.6732177734375,
                    150.0560302734375,
                    -3038.316162109375
                }
            },
            {
                "12-8",
                new double[3] {
                    2156.33203125,
                    153.7804412841797,
                    -3911.78271484375
                }
            },
            {
                "22-6",
                new double[3] {
                    4110.6044921875,
                    229.98350524902344,
                    -3351.48583984375
                }
            },
            {
                "5-10",
                new double[3] {
                    1119.561767578125,
                    124.97010040283203,
                    -1513.80224609375
                }
            },
            {
                "8-20",
                new double[3] {
                    3439.577880859375,
                    114.8779296875,
                    -1717.965087890625
                }
            },
            {
                "14-2",
                new double[3] {
                    4236.2958984375,
                    113.01073455810547,
                    -2889.38037109375
                }
            },
            {
                "22-11",
                new double[3] {
                    3751.060791015625,
                    250.20762634277344,
                    -3441.369384765625
                }
            },
            {
                "5-4",
                new double[3] {
                    1913.548583984375,
                    102.94244384765625,
                    -1264.60009765625
                }
            },
            {
                "5-6",
                new double[3] {
                    1512.9560546875,
                    102.26972961425781,
                    -1338.2242431640625
                }
            },
            {
                "9-15",
                new double[3] {
                    903.5161743164062,
                    201.90554809570312,
                    -1487.36865234375
                }
            },
            {
                "9-8_",
                new double[3] {
                    1696.9686279296875,
                    299.386962890625,
                    -2469.46923828125
                }
            },
            {
                "22-22",
                new double[3] {
                    4378.17919921875,
                    250.0360107421875,
                    -3477.567138671875
                }
            },
            {
                "22-12",
                new double[3] {
                    3868.648681640625,
                    249.9816436767578,
                    -3418.941650390625
                }
            },
            {
                "17-7",
                new double[3] {
                    2467.7353515625,
                    307.21368408203125,
                    -3186.029541015625
                }
            },
            {
                "15-3",
                new double[3] {
                    4221.93896484375,
                    100.15505981445312,
                    -1787.1536865234375
                }
            },
            {
                "12-10",
                new double[3] {
                    2205.8232421875,
                    199.98956298828125,
                    -3211.67333984375
                }
            },
            {
                "9-2_",
                new double[3] {
                    903.4185180664062,
                    280.3360595703125,
                    -2197.499267578125
                }
            },
            {
                "9-10_",
                new double[3] {
                    1136.1429443359375,
                    200.31402587890625,
                    -2655.60400390625
                }
            },
            {
                "15-2_",
                new double[3] {
                    1137.9058837890625,
                    150.47738647460938,
                    -2015.20068359375
                }
            },
            {
                "18-13",
                new double[3] {
                    1628.474609375,
                    52.370384216308594,
                    -3998.650146484375
                }
            },
            {
                "19-6_",
                new double[3] {
                    3183.7041015625,
                    124.51063537597656,
                    -4673.658203125
                }
            },
            {
                "22-7",
                new double[3] {
                    4274.79345703125,
                    299.6488342285156,
                    -3729.359130859375
                }
            },
            {
                "9-1_",
                new double[3] {
                    1138.105712890625,
                    150.4706573486328,
                    -2015.4853515625
                }
            },
            {
                "9-12",
                new double[3] {
                    1049.3533935546875,
                    219.7825927734375,
                    -1842.792724609375
                }
            },
            {
                "19-10_",
                new double[3] {
                    2566.956298828125,
                    113.32160949707031,
                    -4669.66845703125
                }
            },
            {
                "22-1",
                new double[3] {
                    3862.605712890625,
                    220.00645446777344,
                    -3740.615234375
                }
            },
            {
                "9-17",
                new double[3] {
                    527.1552124023438,
                    257.4854736328125,
                    -1979.5361328125
                }
            },
            {
                "21-8",
                new double[3] {
                    3804.7041015625,
                    249.48233032226562,
                    -3745.94140625
                }
            },
            {
                "15-5_",
                new double[3] {
                    3210.9892578125,
                    199.86392211914062,
                    -2637.64990234375
                }
            },
            {
                "9-5_",
                new double[3] {
                    1540.74755859375,
                    254.6824493408203,
                    -2678.585693359375
                }
            },
            {
                "19-2_",
                new double[3] {
                    3065.919677734375,
                    335.0486145019531,
                    -4265.58642578125
                }
            },
            {
                "9-9_",
                new double[3] {
                    1400.65966796875,
                    154.99993896484375,
                    -1985.52294921875
                }
            },
            {
                "9-3_",
                new double[3] {
                    1160.859375,
                    156.00291442871094,
                    -1684.067138671875,
                }
            }
        };
    }
}
    public class raidsprite
    {
    public static string[] Area = new string[] {
           "South Province (Area 1)",
           "", // Unused
           "", // Unused
           "South Province (Area 2)",
           "South Province (Area 4)",
           "South Province (Area 6)",
           "South Province (Area 5)",
           "South Province (Area 3)",
           "West Province (Area 1)",
           "Asado Desert",
           "West Province (Area 2)",
           "West Province (Area 3)",
           "Tagtree Thicket",
           "East Province (Area 3)",
           "East Province (Area 1)",
           "East Province (Area 2)",
           "Dalizapa Passage",
           "Casseroya Lake",
           "Glaseado Mountain",
           "North Province (Area 3)",
           "North Province (Area 1)",
           "North Province (Area 2)",
        };
        public raidsprite(PK9 pk9, TeraRaidDetail raid)
        {
            var found = RaidViewer.denloc.TryGetValue($"{raid.AreaID}-{raid.SpawnPointID}", out var result);
            if (found)
            {
            coords = result;
            }
        
       
            location = raid.AreaID == 0 ? Area[1] : Area[raid.AreaID - 1];
       
            bgcolor = RaidViewer.GetTypeSpriteColor((byte)pk9.TeraTypeOriginal);
            Raid = raid;
            pkm = pk9;

            if (pk9.Species == 0)
                url = $"https://raw.githubusercontent.com/santacrab2/Resources/main/gen9sprites/{pk9.Species:0000}{(pk9.Form != 0 ? $"-{pk9.Form:00}" : "")}.png";
            else if (pk9.IsShiny)
                url = $"https://www.serebii.net/Shiny/SV/{pk9.Species:000}.png";
            else if (pk9.Form != 0)
                url = $"https://raw.githubusercontent.com/santacrab2/Resources/main/gen9sprites/{pk9.Species:0000}{(pk9.Form != 0 ? $"-{pk9.Form:00}" : "")}.png";
            else
                url = $"https://www.serebii.net/scarletviolet/pokemon/{pk9.Species:000}.png";

            var starxoro = RaidViewer.getxoronextint(raid.Seed, 100);
            var stars = starxoro switch
            {
                <= 30 => 3,
                <= 70 => 4,
                > 70 => 5,
            };
            if (raid.Content == TeraRaidContentType.Black6)
                stars = 6;
            Stars = stars.ToString();
        }
        public string Stars { get; set; }
        public string location { get; set; }
        public Color bgcolor { get; set; }
        public PKM pkm { get; set; }
        public string url { get; set; }
        public double[] coords { get; set; }
        public TeraRaidDetail Raid { get; set; }

    }

