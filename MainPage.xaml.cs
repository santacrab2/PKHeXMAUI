using PKHeX.Core;


namespace pk9reader;

public partial class MainPage : ContentPage
{
	

	public MainPage()
	{
		InitializeComponent();
		pecies.ItemsSource = Enum.GetValues(typeof(Species));
	}

	public void pokepicchange(object sender, EventArgs e)
	{
		
        var pkm = EntityBlank.GetBlank(8);
        pkm.Species = (ushort)pecies.SelectedIndex;
        var img = PokeImg(pkm, false, false);
        pokepic.Source = img;

	}
    public static string PokeImg(PKM pkm, bool canGmax, bool fullSize)
    {
        bool md = false;
        bool fd = false;
        string[] baseLink;
        if (fullSize)
            baseLink = "https://raw.githubusercontent.com/Koi-3088/HomeImages/master/512x512/poke_capture_0001_000_mf_n_00000000_f_n.png".Split('_');
        else baseLink = "https://raw.githubusercontent.com/Koi-3088/HomeImages/master/128x128/poke_capture_0001_000_mf_n_00000000_f_n.png".Split('_');

        if (Enum.IsDefined(typeof(GenderDependent), pkm.Species))
        {
            if (pkm.Gender == 0 && pkm.Species != (int)Species.Torchic)
                md = true;
            else fd = true;
        }

        int form = pkm.Species switch
        {
            (int)Species.Sinistea or (int)Species.Polteageist or (int)Species.Rockruff or (int)Species.Mothim => 0,
            (int)Species.Alcremie when pkm.IsShiny || canGmax => 0,
            _ => pkm.Form,

        };

        baseLink[2] = pkm.Species < 10 ? $"000{pkm.Species}" : pkm.Species < 100 && pkm.Species > 9 ? $"00{pkm.Species}" : $"0{pkm.Species}";
        baseLink[3] = pkm.Form < 10 ? $"00{form}" : $"0{form}";
        baseLink[4] = pkm.PersonalInfo.OnlyFemale ? "fo" : pkm.PersonalInfo.OnlyMale ? "mo" : pkm.PersonalInfo.Genderless ? "uk" : fd ? "fd" : md ? "md" : "mf";
        baseLink[5] = canGmax ? "g" : "n";
        baseLink[6] = "0000000" + (pkm.Species == (int)Species.Alcremie && !canGmax ? pkm.Data[0xE4] : 0);
        baseLink[8] = pkm.IsShiny ? "r.png" : "n.png";
        return string.Join("_", baseLink);
    }
    public enum GenderDependent : ushort
    {
        Venusaur = 3,
        Butterfree = 12,
        Rattata = 19,
        Raticate = 20,
        Pikachu = 25,
        Raichu = 26,
        Zubat = 41,
        Golbat = 42,
        Gloom = 44,
        Vileplume = 45,
        Kadabra = 64,
        Alakazam = 65,
        Doduo = 84,
        Dodrio = 85,
        Hypno = 97,
        Rhyhorn = 111,
        Rhydon = 112,
        Goldeen = 118,
        Seaking = 119,
        Scyther = 123,
        Magikarp = 129,
        Gyarados = 130,
        Eevee = 133,
        Meganium = 154,
        Ledyba = 165,
        Ledian = 166,
        Xatu = 178,
        Sudowoodo = 185,
        Politoed = 186,
        Aipom = 190,
        Wooper = 194,
        Quagsire = 195,
        Murkrow = 198,
        Wobbuffet = 202,
        Girafarig = 203,
        Gligar = 207,
        Steelix = 208,
        Scizor = 212,
        Heracross = 214,
        Sneasel = 215,
        Ursaring = 217,
        Piloswine = 221,
        Octillery = 224,
        Houndoom = 229,
        Donphan = 232,
        Torchic = 255,
        Combusken = 256,
        Blaziken = 257,
        Beautifly = 267,
        Dustox = 269,
        Ludicolo = 272,
        Nuzleaf = 274,
        Shiftry = 275,
        Meditite = 307,
        Medicham = 308,
        Roselia = 315,
        Gulpin = 316,
        Swalot = 317,
        Numel = 322,
        Camerupt = 323,
        Cacturne = 332,
        Milotic = 350,
        Relicanth = 369,
        Starly = 396,
        Staravia = 397,
        Staraptor = 398,
        Bidoof = 399,
        Bibarel = 400,
        Kricketot = 401,
        Kricketune = 402,
        Shinx = 403,
        Luxio = 404,
        Luxray = 405,
        Roserade = 407,
        Combee = 415,
        Pachirisu = 417,
        Floatzel = 418,
        Buizel = 419,
        Ambipom = 424,
        Gible = 443,
        Gabite = 444,
        Garchomp = 445,
        Hippopotas = 449,
        Hippowdon = 450,
        Croagunk = 453,
        Toxicroak = 454,
        Finneon = 456,
        Lumineon = 457,
        Snover = 459,
        Abomasnow = 460,
        Weavile = 461,
        Rhyperior = 464,
        Tangrowth = 465,
        Mamoswine = 473,
        Unfezant = 521,
        Frillish = 592,
        Jellicent = 593,
        Pyroar = 668,
    }
}

