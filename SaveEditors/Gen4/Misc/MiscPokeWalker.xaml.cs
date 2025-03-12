using PKHeX.Core;
using System.Collections.ObjectModel;

namespace PKHeXMAUI;

public partial class MiscPokeWalker : ContentPage
{
    SAV4HGSS SAV;
    public MiscPokeWalker(SAV4HGSS sav)
	{
		InitializeComponent();
        SAV = sav;
        CV_WalkerCourses.ItemTemplate = new DataTemplate(() =>
        {
            Grid grid = [];
            CheckBox cb = new();
            cb.SetBinding(CheckBox.IsCheckedProperty, ".Item1",BindingMode.TwoWay);
            Label lab = new();
            lab.SetBinding(Label.TextProperty, ".Item2");
            grid.Add(cb);
            grid.Add(lab);
            return grid;
        });
        ReadWalker(sav);
    }
    public ObservableCollection<Tuple<bool, string>> CourseList = [];
    private void ReadWalker(SAV4HGSS s)
    {
        ReadWalkerCourseUnlockFlags(s);

        NUD_Watts.Number = s.PokewalkerWatts;
        NUD_Steps.Number = s.PokewalkerSteps;
    }

    private void ReadWalkerCourseUnlockFlags(SAV4HGSS s)
    {
        ReadOnlySpan<string> walkercourses = GameInfo.Sources.Strings.walkercourses;
        Span<bool> courses = stackalloc bool[SAV4HGSS.PokewalkerCourseFlagCount];
        s.GetPokewalkerCoursesUnlocked(courses);
        for (int i = 0; i < walkercourses.Length; i++)
            CourseList.Add(Tuple.Create(courses[i], walkercourses[i]));
        CV_WalkerCourses.ItemsSource = CourseList;
    }

    private void SaveWalker(SAV4HGSS s)
    {
        Span<bool> courses = stackalloc bool[SAV4HGSS.PokewalkerCourseFlagCount];
        for (int i = 0; i < CourseList.Count; i++)
            courses[i] = CourseList[i].Item1;
        s.SetPokewalkerCoursesUnlocked(courses);

        s.PokewalkerWatts = (uint)NUD_Watts.Number;
        s.PokewalkerSteps = (uint)NUD_Steps.Number;
    }

    private void B_AllWalkerCourses_Click(object sender, EventArgs e)
    {
        if (SAV is not SAV4HGSS s)
            throw new Exception("Invalid SAV type");
        s.PokewalkerCoursesUnlockAll();
        ReadWalkerCourseUnlockFlags(s);
    }
}