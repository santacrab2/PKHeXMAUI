using PKHeX.Core;
using System.Collections.ObjectModel;

namespace PKHeXMAUI;

public partial class MiscDecorations : ContentPage
{
	public MiscDecorations(ISaveBlock3LargeHoenn sav)
	{
		InitializeComponent();
		ObservableCollection<Tuple<DecorationCategory3,Decoration3[]>> decoarray = [Tuple.Create(DecorationCategory3.Desk, sav.Decorations.Desk.ToArray()), Tuple.Create(DecorationCategory3.Chair, sav.Decorations.Chair.ToArray()), Tuple.Create(DecorationCategory3.Plant, sav.Decorations.Plant.ToArray()), Tuple.Create(DecorationCategory3.Ornament, sav.Decorations.Ornament.ToArray()), Tuple.Create(DecorationCategory3.Mat, sav.Decorations.Mat.ToArray()), Tuple.Create(DecorationCategory3.Poster, sav.Decorations.Poster.ToArray()), Tuple.Create(DecorationCategory3.Doll, sav.Decorations.Doll.ToArray()), Tuple.Create(DecorationCategory3.Cushion, sav.Decorations.Cushion.ToArray())];
        Decorationsview.ItemTemplate = new DataTemplate(() =>
        {
            var CV_Desk = new CollectionView();
            var decorations = decoarray[0].Item2[0].GetType().GetEnumNames();
            var list = Util.GetCBList(decorations);
            CV_Desk.ItemTemplate = new DataTemplate(() =>
            {
                Grid grid = [];
                comboBox deskbox = new()
                {
                    ItemSource = list,
                    DisplayMemberPath = "Text"
                };
                deskbox.SetBinding(comboBox.SelectedItemProperty, ".",BindingMode.TwoWay);
                grid.Add(deskbox);
                return grid;
            });

            CV_Desk.SetBinding(CollectionView.ItemsSourceProperty, "Item2");
            Label labs = new();
            labs.SetBinding(Label.TextProperty, "Item1");
            StackLayout stack = [labs,CV_Desk];
            return stack;
        });
        Decorationsview.ItemsSource = decoarray;
	}
}
public static class lazyext
{
    public static DecorationCategory3 GetCategory(this Decoration3 deco) => deco switch
    {
        <= Decoration3.HARD_DESK => DecorationCategory3.Desk,
        <= Decoration3.HARD_CHAIR => DecorationCategory3.Chair,
        <= Decoration3.GORGEOUS_PLANT => DecorationCategory3.Plant,
        <= Decoration3.CUTE_TV => DecorationCategory3.Ornament,
        <= Decoration3.SPIKES_MAT => DecorationCategory3.Mat,
        <= Decoration3.KISS_POSTER => DecorationCategory3.Poster,
        <= Decoration3.SEEDOT_DOLL => DecorationCategory3.Doll,
        <= Decoration3.WATER_CUSHION => DecorationCategory3.Cushion,
        <= Decoration3.REGISTEEL_DOLL => DecorationCategory3.Doll,
        _ => throw new ArgumentOutOfRangeException(nameof(deco)),
    };
}