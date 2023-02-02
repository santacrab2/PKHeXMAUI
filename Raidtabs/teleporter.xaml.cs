
using PKHeX.Core;

using System.Windows.Input;
using static PKHeX.Core.Encounters9;
using static pk9reader.RaidViewer;


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
        try
        {
            var telporterpointer = new long[] { 0x43A75B0, 0x2A8, 0x0, 0x0, 0x80 };
            var teleporteroff = await botBase.PointerRelative(telporterpointer);
            var co = await botBase.ReadBytesAsync((uint)teleporteroff, 12);
            recentcoords = co;
            xlab.Text = $"{BitConverter.ToString(co, 0, 4)}";
            ylab.Text = $"{BitConverter.ToString(co, 4, 4)}";
            zlab.Text = $"{BitConverter.ToString(co, 8, 4)}";
        }
        catch (Exception)
        {
            if (SwitchConnection.Connected)
            {
                await SwitchConnection.DisconnectAsync(true);
            }
            if (!SwitchConnection.Connected)
            {

                await SwitchConnection.ConnectAsync(ipaddy, 6000);
            }
            readloc(sender, e);
        }
    }

    private async void teleloc(object sender, EventArgs e)
    {
        try
        {
            var telporterpointer = new long[] { 0x43A75B0, 0x2A8, 0x0, 0x0, 0x80 };
            var teleporteroff = await botBase.PointerRelative(telporterpointer);
            var x = BitConverter.ToSingle(recentcoords, 0);
            var xbit = BitConverter.GetBytes(x);
            var y = BitConverter.ToSingle(recentcoords, 4) + 30;
            var ybit = BitConverter.GetBytes(y);
            var z = BitConverter.ToSingle(recentcoords, 8);
            var zbit = BitConverter.GetBytes(z);
            var xyz = xbit.Concat(ybit).ToArray();
            xyz = xyz.Concat(zbit).ToArray();
            await botBase.WriteBytesAsync(xyz, (uint)teleporteroff);
        }
        catch (Exception)
        {
            if (SwitchConnection.Connected)
            {
                await SwitchConnection.DisconnectAsync(true);
            }
            if (!SwitchConnection.Connected)
            {

                await SwitchConnection.ConnectAsync(ipaddy, 6000);
            }
            teleloc(sender, e);
        
        }
    }

    private async void Shinifyall(object sender, EventArgs e)
    {
        SAV9SV tempsave = (SAV9SV)sav;
        int v = 0;
        allshinybutton.Text = $"loading {v}";
        await Task.Delay(500);
        var raidlists = await raidlist();
        foreach (raidsprite raid in raidlists)
        {
            allshinybutton.Text = $"loading {v}";
            await Task.Delay(500);
            if (raid.pkm.IsShiny)
            {
                v++;
                continue;
            }
            ulong seed = raid.Raid.Seed;
            while (!raid.pkm.IsShiny)
            {

                seed = getxoronextseed((uint)seed);
                switch (raid.Raid.Content)
                {
                    case TeraRaidContentType.Base05:
                        int starxoro = getxoronextint((uint)seed, 100);
                        var stars = starxoro switch
                        {
                            <= 30 => 3,
                            <= 70 => 4,
                            > 70 => 5,
                        };
                        if (raid.Raid.Content == TeraRaidContentType.Black6)
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
                                if ((Species)theencounter.Species == (Species)raid.pkm.Species)
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
                        raid.pkm.SetIsShiny(xor < 16); break;

                    case TeraRaidContentType.Black6:
                        starxoro = getxoronextint((uint)seed, 100);
                        stars = starxoro switch
                        {
                            <= 30 => 3,
                            <= 70 => 4,
                            > 70 => 5,
                        };
                        if (raid.Raid.Content == TeraRaidContentType.Black6)
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
                                if ((Species)theencounter.Species == (Species)raid.pkm.Species)
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
                        raid.pkm.SetIsShiny(xor < 16); break;
                    case TeraRaidContentType.Distribution:


                        game = GameVersion.SL;




                        EncounterRaid9 dencounter = null;
                        //var dist = EncounterDist9.GetArray( tempsave.Blocks.GetBlock(0x520A1B0).Data);
                        foreach (var theencounter in dist)
                        {
                            var maxd = game is GameVersion.SL ? theencounter.GetRandRateTotalScarlet(3) : theencounter.GetRandRateTotalViolet(3);
                            var min = game is GameVersion.SL ? theencounter.GetRandRateMinScarlet(3) : theencounter.GetRandRateMinViolet(3);
                            if (min >= 0 && maxd > 0)
                            {

                                var rateRandd = getxororandrate((uint)seed, maxd, theencounter.Stars);
                                if ((uint)(rateRandd - min) < theencounter.RandRate)
                                {
                                    if ((Species)theencounter.Species == (Species)raid.pkm.Species)
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
                        raid.pkm.SetIsShiny(xor < 16); break;
                    case TeraRaidContentType.Might7:

                        game = GameVersion.SL;




                        EncounterRaid9 mencounter = null;
                        //var dist = EncounterDist9.GetArray( tempsave.Blocks.GetBlock(0x520A1B0).Data);
                        foreach (var theencounter in might)
                        {
                            var maxd = game is GameVersion.SL ? theencounter.GetRandRateTotalScarlet(3) : theencounter.GetRandRateTotalViolet(3);
                            var min = game is GameVersion.SL ? theencounter.GetRandRateMinScarlet(3) : theencounter.GetRandRateMinViolet(3);
                            if (min >= 0 && maxd > 0)
                            {

                                var rateRandd = getxororandrate((uint)seed, maxd, 5);
                                if ((uint)(rateRandd - min) < theencounter.RandRate)
                                {
                                    if ((Species)theencounter.Species == (Species)raid.pkm.Species)
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
                        raid.pkm.SetIsShiny(xor < 16); break;
                }

            }
            raid.Raid.Seed = (uint)seed;
            v++;
        }
            var raidspawn = tempsave.Raid;
            var allraids = raidspawn.GetAllRaids();
            int j = 0;
            foreach (var raidi in allraids)
            {
             
                raidi.Seed = raidlists[j].Raid.Seed;
                j++;
                if (j > 67)
                    break;

            }
        try
        {
            var raidpointer = new List<long> { 0x4384B18, 0x180, 0x40 };
            var raidoff = await botBase.PointerRelative(raidpointer);
            await botBase.WriteBytesAsync(tempsave.Raid.Data, (uint)raidoff);
        }
        catch (Exception)
        {
            if (SwitchConnection.Connected)
            {
                await SwitchConnection.DisconnectAsync(true);
            }
            if (!SwitchConnection.Connected)
            {

                await SwitchConnection.ConnectAsync(ipaddy, 6000);
            }
            var raidpointer = new List<long> { 0x4384B18, 0x180, 0x40 };
            var raidoff = await botBase.PointerRelative(raidpointer);
            await botBase.WriteBytesAsync(tempsave.Raid.Data, (uint)raidoff);
        }
        allshinybutton.Text = "Shinify All";
    }
    public async Task<List<raidsprite>> raidlist()
    {
        
        List<raidsprite> returnlist = new();
        SAV9SV tempsave = (SAV9SV)sav;
        byte[] raiddata = Array.Empty<byte>();
        try
        {
            var raidpointer = new List<long> { 0x4384B18, 0x180, 0x40 };
            var raidoff = await botBase.PointerRelative(raidpointer);
            raiddata = await botBase.ReadBytesAsync((uint)raidoff, 0x910);
        }
        catch(Exception)
        {
            if (SwitchConnection.Connected)
            {
                await SwitchConnection.DisconnectAsync(true);
            }
            if (!SwitchConnection.Connected)
            {

                await SwitchConnection.ConnectAsync(ipaddy, 6000);
            }
            var raidpointer = new List<long> { 0x4384B18, 0x180, 0x40 };
            var raidoff = await botBase.PointerRelative(raidpointer);
            raiddata = await botBase.ReadBytesAsync((uint)raidoff, 0x910);
        }

        var accessor = tempsave.Blocks;
        var raidblock = accessor.GetBlock(0xCAAC8800);
        raiddata.CopyTo(raidblock.Data, 0);
        var raidspawn = tempsave.Raid;
        var allraids = raidspawn.GetAllRaids();
        EncounterRaid9 mencounter = null;
        EncounterRaid9 dencounter = null;
        EncounterTera9 encounter = null;
        int i = 0;
        foreach (var raid in allraids)
        {

            switch (raid.Content)
            {
                case TeraRaidContentType.Distribution:

                    var game = GameVersion.SL;





                    // var dist = EncounterDist9.GetArray( tempsave.Blocks.GetBlock(0x520A1B0).Data);
                    foreach (var theencounter in dist)
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
                        dencounter = dist[1];
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
                        TrainerTID7 = sav.TrainerTID7,
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
                        TrainerTID7 = sav.TrainerTID7,
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
                        TrainerTID7 = sav.TrainerTID7,
                        TrainerSID7 = sav.TrainerSID7,
                        TeraTypeOriginal = (MoveType)Tera9RNG.GetTeraType(raid.Seed, encounter.TeraType, encounter.Species, encounter.Form),
                    };

                    Encounter9RNG.GenerateData(pk6, param6, EncounterCriteria.Unrestricted, raid.Seed);

                    returnlist.Add(new raidsprite(pk6, raid)); break;
                case TeraRaidContentType.Might7:
                    var game7 = GameVersion.SL;

                    foreach (var theencounter in might)
                    {
                        var maxm = game7 is GameVersion.SL ? theencounter.GetRandRateTotalScarlet(3) : theencounter.GetRandRateTotalViolet(3);
                        var min = game7 is GameVersion.SL ? theencounter.GetRandRateMinScarlet(3) : theencounter.GetRandRateMinViolet(3);
                        if (min >= 0 && maxm > 0)
                        {

                            var rateRandd = getxororandrate(raid.Seed, maxm, 5);
                            if ((uint)(rateRandd - min) < theencounter.RandRate)
                            {
                                mencounter = theencounter; break;
                            }

                        }
                    }
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
                        TrainerTID7 = sav.TrainerTID7,
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
}