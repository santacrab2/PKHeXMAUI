
using PKHeX.Core;
using static PKHeXMAUI.MainPage;
namespace PKHeXMAUI;

public partial class SaveEditors : ContentPage
{
	public SaveEditors()
	{
		InitializeComponent();
        ToggleControls();
	}

    private void OpenItems(object sender, EventArgs e)
    {
        Navigation.PushModalAsync(new Items());
    }

    private void OpenBlockEditor(object sender, EventArgs e)
    {
        switch (MainPage.sav)
        {
            case SAV1 s: Navigation.PushModalAsync(new SavAccessorGUI(s, null)); break;
            case SAV2 s: Navigation.PushModalAsync(new SavAccessorGUI(s, null)); break;
            case SAV3 s: Navigation.PushModalAsync(new SavAccessorGUI(s, null)); break;
            case SAV4 s: Navigation.PushModalAsync(new SavAccessorGUI(s, null)); break;
            case SAV5BW s: Navigation.PushModalAsync(new SavAccessorGUI(s, s.Blocks)); break;
            case SAV5B2W2 s: Navigation.PushModalAsync(new SavAccessorGUI(s, s.Blocks)); break;
            case SAV6XY s: Navigation.PushModalAsync(new SavAccessorGUI(s, s.Blocks)); break;
            case SAV6AO s: Navigation.PushModalAsync(new SavAccessorGUI(s, s.Blocks)); break;
            case SAV6AODemo s: Navigation.PushModalAsync(new SavAccessorGUI(s, s.Blocks)); break;
            case SAV7SM s: Navigation.PushModalAsync(new SavAccessorGUI(s, s.Blocks)); break;
            case SAV7USUM s: Navigation.PushModalAsync(new SavAccessorGUI(s, s.Blocks)); break;
            case SAV7b s: Navigation.PushModalAsync(new SavAccessorGUI(s, s.Blocks)); break;
            case ISCBlockArray: Navigation.PushModalAsync(new BlockDataTab()); break;
            default: Navigation.PushModalAsync(GetPropertyForm(sav)); break;

        }
    }
    private static ContentPage GetPropertyForm(object sav)
    {
        var form = new ContentPage
        {
            Title = "Simple Editor",
        };
        var pg = new propertyGrid(sav);
        ScrollView scroll = new() { Content = pg, Orientation = ScrollOrientation.Both };
        form.Content = scroll;
        return form;
    }
    private void ToggleControls()
    {
        if (!sav.State.Exportable || sav is BulkStorage)
            return;
        Button_BlockData.IsVisible = true;

        TrainerInfoButton.IsVisible = (sav is not SAV8BS || sav is not SAV8SWSH || sav is not SAV7b);
        Button_EventFlags1.IsVisible = (sav is SAV1 or SAV2 or SAV3 or SAV4);
        
        Button_Pokedex1.IsVisible = (sav is SAV1 or SAV2 or SAV3);
        if (sav is SAV2 sav2)
        {
            GSBallButton.IsVisible = sav.Version is GameVersion.C;
            GSBallButton.IsEnabled = !sav2.IsEnabledGSBallMobileEvent;
        }
        Button_RTCEditor.IsVisible = (sav is SAV2 or SAV3);
        B_Misc.IsVisible = (sav is SAV3 or SAV4);
        B_Roamer.IsVisible = (sav is SAV3);
        B_Chatter.IsVisible = (sav is SAV4 or SAV5);
        B_Geonet.IsVisible = (sav is SAV4);
        B_WonderCard.IsVisible = (sav is SAV4 or SAV5 or SAV6 or SAV7);
        B_HoneyTree.IsVisible = (sav is SAV4Sinnoh);
        MailBoxButton.IsVisible = (sav is SAV2 or SAV3 or SAV4 or SAV5);
        B_Underground.IsVisible = (sav is SAV4Sinnoh);
    }

    private void OpenTrainerEditor(object sender, EventArgs e)
    {
        switch (MainPage.sav)
        {
            case SAV1:
            case SAV2:
            case SAV3:
            case SAV3XD:
            case SAV3Colosseum:
            case SAV4:
            case SAV5: Navigation.PushModalAsync(new TrainerEditor1()); break;
            case SAV6: Navigation.PushModalAsync(new TrainerTab6()); break;
            case SAV7: Navigation.PushModalAsync(new TrainerTab7()); break;
            case SAV8LA: Navigation.PushModalAsync(new TrainerTab8a()); break;
            case SAV9SV: Navigation.PushModalAsync(new TrainerTab9()); break;
        }
    }

    private void OpenEventFlagEditor(object sender, EventArgs e)
    {
        Navigation.PushModalAsync(sav switch
        {
            SAV1 => new EventReset1((SAV1)sav),
            SAV2 => new EventFlags2Tab(),
            IEventFlag37 => new EventFlagsTab((IEventFlag37)sav,sav.Version),
            IEventFlagProvider37 => new EventFlagsTab(((IEventFlagProvider37)sav).EventWork,sav.Version),
            _ => throw new Exception()
        });
    }

    private void OpenSimplePokedex(object sender, EventArgs e)
    {
        Navigation.PushModalAsync(new Pokedex1(MainPage.sav));
    }

    private async void OpenRTCEditor(object sender, EventArgs e)
    {
        switch (sav.Generation)
        {
            case 2:
                var sav2 = ((SAV2)sav);
                var msg = MessageStrings.MsgSaveGen2RTCResetBitflag;
                if (!sav2.Japanese) // show Reset Key for non-Japanese saves
                    msg = string.Format(MessageStrings.MsgSaveGen2RTCResetPassword, sav2.ResetKey) + Environment.NewLine + Environment.NewLine + msg;
                var dr = await DisplayAlertAsync("Reset RTC", msg, "Yes", "cancel");
                if (dr)
                    sav2.ResetRTC();
                break;
            case 3:
                Navigation.PushModalAsync(new RTC3Editor(sav));
                break;
        }
    }

    private void OpenMailBoxEditor(object sender, EventArgs e)
    {
        Navigation.PushModalAsync(new MailBox(sav));
    }

    private void EnableGSBallEvent(object sender, EventArgs e)
    {
        var sav2 = sav as SAV2??((SAV2)BlankSaveFile.Get(EntityContext.Gen2,""));
        sav2.EnableGSBallMobileEvent();
        GSBallButton.IsEnabled = false;
    }

    private void OpenMiscEditor(object sender, EventArgs e)
    {
        switch (sav) 
        { 
            case SAV3: Navigation.PushModalAsync(new MiscTab()); break; 
            case SAV4: Navigation.PushModalAsync(new MiscTab4()); break; 
        }
       
    }

    private void OpenRoamerEditor(object sender, EventArgs e)
    {
        Navigation.PushModalAsync(new RoamerEditor3((SAV3)sav));
    }

    private void OpenChatterEditor(object sender, EventArgs e)
    {
        Navigation.PushModalAsync(new ChatterEditor());
    }

    private void B_Geonet_Clicked(object sender, EventArgs e)
    {
        Navigation.PushModalAsync(new Geonet4Editor((SAV4)sav));
    }

    private void B_HoneyTree_Clicked(object sender, EventArgs e)
    {
        Navigation.PushModalAsync(new HoneyTreeEditor((SAV4Sinnoh)sav));
    }

    private void B_WonderCard_Clicked(object sender, EventArgs e)
    {
        Navigation.PushModalAsync(new WondercardEditor(sav));
    }

    private void B_Underground_Clicked(object sender, EventArgs e)
    {
        Navigation.PushModalAsync(new UndergroundTab());
    }
}