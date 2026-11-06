namespace PKHeXMAUI;

/// <summary>
/// Gen3 合规优先级设置页。
///
/// Gen3 的 PID 与 IV 同源（Method 1），闪光/性格/个体值/性别四项无法全部同时成立。
/// 冲突时该牺牲哪一项取决于用户在意什么，因此这里让用户自己排定顺序：
/// 越靠前的项目优先保留，靠后的先被让步。
/// </summary>
public partial class Gen3PrioritySettings : ContentPage
{
    public Gen3PrioritySettings()
    {
        InitializeComponent();
        RebuildList();
    }

    /// <summary>按当前配置重建优先级列表。</summary>
    private void RebuildList()
    {
        Stack_Order.Children.Clear();
        var order = Gen3Priority.GetOrder();
        for (int i = 0; i < order.Count; i++)
        {
            var field = order[i];
            var index = i;

            var label = new Label
            {
                Text = $"{index + 1}. {Gen3Priority.DisplayName(field)}",
                FontSize = 15,
                FontAttributes = FontAttributes.Bold,
                VerticalOptions = LayoutOptions.Center,
            };

            var up = new Button
            {
                Text = "↑",
                WidthRequest = 44,
                HeightRequest = 34,
                Padding = new Thickness(0),
                IsEnabled = index > 0,
            };
            up.Clicked += (_, _) =>
            {
                if (Gen3Priority.MoveUp(field))
                    RebuildList();
            };

            var down = new Button
            {
                Text = "↓",
                WidthRequest = 44,
                HeightRequest = 34,
                Padding = new Thickness(0),
                IsEnabled = index < order.Count - 1,
            };
            down.Clicked += (_, _) =>
            {
                if (Gen3Priority.MoveDown(field))
                    RebuildList();
            };

            var row = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                    new ColumnDefinition { Width = GridLength.Auto },
                    new ColumnDefinition { Width = GridLength.Auto },
                },
                Padding = new Thickness(8, 2),
            };
            row.Add(label, 0, 0);
            row.Add(up, 1, 0);
            row.Add(down, 2, 0);

            Stack_Order.Children.Add(row);
        }
    }

    private void OnReset(object? sender, EventArgs e)
    {
        Gen3Priority.SetOrder(new List<Gen3Priority.Field>
        {
            Gen3Priority.Field.Nature,
            Gen3Priority.Field.Iv,
            Gen3Priority.Field.Shiny,
            Gen3Priority.Field.Gender,
        });
        RebuildList();
    }
}
