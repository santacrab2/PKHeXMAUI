
using PKHeX.Core;

namespace PKHeXMAUI;

public partial class Geonet4Editor : ContentPage
{
    private readonly SaveFile Origin;
    private readonly SAV4 SAV;
    private readonly Geonet4 Geonet;

    private readonly List<ComboItem> countryList;
    private readonly List<ComboItem> subregionListDefault;
    private readonly List<GeonetItem> geonetItems = new();
    private readonly List<ComboItem> pointList;
    public Geonet4Editor(SAV4 sav)
	{
		InitializeComponent();
        SAV = (SAV4)(Origin = sav).Clone();

        Geonet = new Geonet4(SAV);

        countryList = Util.GetCountryRegionList("gen4_countries", "en");
        subregionListDefault = Util.GetCountryRegionList("gen4_sr_default", "en");
        pointList = Util.GetGeonetPointList();
        InitializeDGVGeonet();

        CHK_GlobalFlag.IsChecked = Geonet.GlobalFlag;
    }
    private void InitializeDGVGeonet()
    {
        CV_Geonet.ItemTemplate = new(() =>
        {
            Grid grid = new();
            Label lbl = new();
            lbl.SetBinding(Label.TextProperty, new Binding("CountryName"));
            Label lbl2 = new();
            lbl2.SetBinding(Label.TextProperty, new Binding("SubRegionName"));
            comboBox box = new();
            box.SetBinding(comboBox.ItemSourceProperty, new Binding("pointList"));
            box.DisplayMemberPath = "Text";
            box.SetBinding(comboBox.SelectedIndexProperty, new Binding("Point"));
            grid.Add(lbl);
            grid.Add(lbl2,1);
            grid.Add(box, 2);
            return grid;
        });
        if (geonetItems.Any())
            geonetItems.Clear();
        for (int i = 1; i <= Geonet4.CountryCount; i++)
        {
            var country = countryList[i].Value;
            var countryName = countryList[i].Text;
            var subregionCount = Geonet4.GetSubregionCount((byte)country);
            var subregionList = (subregionCount == 0) ? subregionListDefault : Util.GetCountryRegionList($"gen4_sr_{country:000}", "en");
            if (subregionCount == 0)
            {
                var subregion = subregionList[0].Value;
                var subregionName = subregionList[0].Text;
                var point = Geonet.GetCountrySubregion((byte)country, (byte)subregion);
                geonetItems.Add(new(country, subregion, countryName, subregionName,pointList,(int)point));
            }
            for (int j = 1; j <= subregionCount; j++)
            {
                var subregion = subregionList[j].Value;
                var subregionName = subregionList[j].Text;
                var point = Geonet.GetCountrySubregion((byte)country, (byte)subregion);
                geonetItems.Add(new(country, subregion, countryName, subregionName, pointList, (int)point));
            }
        }
        CV_Geonet.ItemsSource = geonetItems;
    }

    private void B_Cancel_Clicked(object sender, EventArgs e)
    {
        Navigation.PopModalAsync();
    }

    private void B_SetAllLegalLocations_Clicked(object sender, EventArgs e)
    {
        Geonet.SetAllLegal();
        InitializeDGVGeonet();
        CHK_GlobalFlag.IsChecked = Geonet.GlobalFlag;
    }

    private void B_SetAllLocations_Clicked(object sender, EventArgs e)
    {
        Geonet.SetAll();
        InitializeDGVGeonet();
        CHK_GlobalFlag.IsChecked = Geonet.GlobalFlag;
    }

    private void B_ClearAllLocations_Clicked(object sender, EventArgs e)
    {
        Geonet.ClearAll();
        InitializeDGVGeonet();
        CHK_GlobalFlag.IsChecked = Geonet.GlobalFlag;
    }

    private void B_Save_Clicked(object sender, EventArgs e)
    {
        Geonet.ClearAll();
        for (int i = 0; i < geonetItems.Count; i++)
        {
            var row = geonetItems[i];
            var country = (int)row.Country!;
            var subregion = (int)row.SubRegion!;
            var point = (GeonetPoint)row.Point!;
            if (country > 0)
                Geonet.SetCountrySubregion((byte)country, (byte)subregion, point);
        }
        Geonet.SetSAVCountry();
        Geonet.Save();

        Geonet.GlobalFlag = CHK_GlobalFlag.IsChecked;
        Origin.CopyChangesFrom(SAV);
        Navigation.PopModalAsync();
    }
}

public class GeonetItem(int country, int subregion, string countryName, string subregionName, List<ComboItem> PointList, int point)
{
    public int Country { get; set; } = country;
    public int SubRegion { get; set; } = subregion;
    public string? CountryName { get; set; } = countryName;
    public string? SubRegionName { get; set; } = subregionName;
    public int Point { get; set; } = point;
    public List<ComboItem> pointList { get; set; } = PointList;

}