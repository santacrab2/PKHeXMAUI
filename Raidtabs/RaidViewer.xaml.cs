
using PKHeX.Core;
using PKHeX.Core.AutoMod;
using System.Reflection;
using System.Text;
using System.Windows.Input;
using static PKHeX.Core.Encounters9;


using static pk9reader.MainPage;
using System.Security.Cryptography;
using System;
using PKHeX.Drawing.PokeSprite;

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


            Image image = new Image() { HeightRequest = 100, WidthRequest = 100, HorizontalOptions = LayoutOptions.Start,Margin=new Thickness(50,0,0,0) };
            image.SetBinding(Image.SourceProperty, "url");
            image.SetBinding(Image.BackgroundColorProperty, "bgcolor");
            grid.Add(image);
            Label teratype = new Label() { HorizontalTextAlignment = TextAlignment.Start };
            teratype.SetBinding(Label.TextProperty, "pkm.TeraTypeOriginal");
            grid.Add(teratype);
            Label loc = new Label() { HorizontalTextAlignment = TextAlignment.End };
            loc.SetBinding(Label.TextProperty, "location");
            grid.Add(loc);
            return grid;
        });

    }
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
    public int getxoronextint(uint seed, ulong rand)
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

            switch (raid.Content) {
                case TeraRaidContentType.Distribution:
                    
                    var game = GameVersion.SL;




                    
                    //var dist = EncounterDist9.GetArray( tempsave.Blocks.GetBlock(0x520A1B0).Data);
                    foreach (var theencounter in Dist)
                    {
                        var maxd = game is GameVersion.SL ? theencounter.GetRandRateTotalScarlet(3) : theencounter.GetRandRateTotalViolet(3);
                        var min = game is GameVersion.SL ? theencounter.GetRandRateMinScarlet(3) : theencounter.GetRandRateMinViolet(3);
                        if (min >= 0 && maxd > 0)
                        {

                            var rateRandd = getxororandrate(raid.Seed, maxd,5);
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

            Encounter9RNG.GenerateData(pk5, param5, EncounterCriteria.Unrestricted, raid.Seed, true);

            returnlist.Add(new raidsprite(pk5, raid));break;
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

                    Encounter9RNG.GenerateData(pk6, param6, EncounterCriteria.Unrestricted, raid.Seed, true);

                    returnlist.Add(new raidsprite(pk6, raid)); break;
                case TeraRaidContentType.Might7:
                  var  game7 = GameVersion.SL;





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
                  var  param7 = new GenerateParam9()
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

                    Encounter9RNG.GenerateData(pk7, param7, EncounterCriteria.Unrestricted, raid.Seed, true);

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
            switch (mainpk.Raid.Content) {
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

                            var rateRandd = getxororandrate((uint)seed, maxd, 5);
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
            location = raid.AreaID == 0?Area[1]: Area[raid.AreaID - 1];
            bgcolor =RaidViewer.GetTypeSpriteColor((byte)pk9.TeraTypeOriginal);
            Raid = raid;
            pkm = pk9;
            species = $"{(Species)pk9.Species}";
            if (pk9.Species == 0)
                url = $"https://raw.githubusercontent.com/santacrab2/Resources/main/gen9sprites/{pk9.Species:0000}{(pk9.Form != 0 ? $"-{pk9.Form:00}" : "")}.png";
            else if (pk9.IsShiny)
                url = $"https://www.serebii.net/Shiny/SV/{pk9.Species}.png";
            else if (pk9.Form != 0)
                url = $"https://raw.githubusercontent.com/santacrab2/Resources/main/gen9sprites/{pk9.Species:0000}{(pk9.Form != 0 ? $"-{pk9.Form:00}" : "")}.png";
            else
                url = $"https://www.serebii.net/scarletviolet/pokemon/{pk9.Species}.png";
        }
        public string location { get; set; }
        public Color bgcolor { get; set; }
        public PKM pkm { get; set; }
        public string url { get; set; }
        public string species { get; set; }
        public TeraRaidDetail Raid { get; set; }

}
