using PKHeX.Core;

namespace PKHeXMAUI;

public partial class App : Application
{
	public App()
	{
        Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Mgo+DSMBaFt+QHJqXU1hXk5Hd0BLVGpAblJ3T2ZQdVt5ZDU7a15RRnVfRF1mSXZTckBgXH1eeQ==;Mgo+DSMBPh8sVXJ1S0R+VFpFdEBBXHxAd1p/VWJYdVt5flBPcDwsT3RfQF5jT35Wdk1jXXpddXdWTw==;ORg4AjUWIQA/Gnt2VFhiQllPd11dXmJWd1p/THNYflR1fV9DaUwxOX1dQl9gSXhTc0VrWXtbc3FRQWk=;MjMxMzI3NUAzMjMxMmUzMDJlMzBYTVYvOFlEWTVidkxaWi90OEFkMHlIU0lYUlpHWi9HV0NkWkVDd3BKQ1pnPQ==;MjMxMzI3NkAzMjMxMmUzMDJlMzBLYVpzbm9WYlM4V1VZUGY3L1dkVm1oTXZrZE42OVhIcVVNK29vYnV3NVhzPQ==;NRAiBiAaIQQuGjN/V0d+Xk9MfV5AQmBIYVp/TGpJfl96cVxMZVVBJAtUQF1hSn5VdkBjUX9adXdTRWJV;MjMxMzI3OEAzMjMxMmUzMDJlMzBWSVBWUTlCbzAwNklMY3Vvc1paYTkvdE9RSmRPUGRoa05SbzRTQ1cwcHQ4PQ==;MjMxMzI3OUAzMjMxMmUzMDJlMzBoUzNQSjM5MWpqZ1FBSmRpQ1JXeWh5WklXWHRlSUNPM2FHRVU1VjM2Wm9vPQ==;Mgo+DSMBMAY9C3t2VFhiQllPd11dXmJWd1p/THNYflR1fV9DaUwxOX1dQl9gSXhTc0VrWXtbc3NVRmk=;MjMxMzI4MUAzMjMxMmUzMDJlMzBJTmNLM1hnNnJuNzBaM1VLTUR3RThOUy9hN2dRa3F4a3Nyamk0b2lIdjBnPQ==;MjMxMzI4MkAzMjMxMmUzMDJlMzBocVc3cVNiRFgvTDR3bzJtbE4zZVJtYWZZeVVUN3YyRkZsV2RzdGVBVmhBPQ==;MjMxMzI4M0AzMjMxMmUzMDJlMzBWSVBWUTlCbzAwNklMY3Vvc1paYTkvdE9RSmRPUGRoa05SbzRTQ1cwcHQ4PQ==");
        InitializeComponent();

        var Version = Preferences.Default.Get("SaveFile", 50);
        if (PSettings.RememberLastSave)
            MainPage = new AppShell(SaveUtil.GetBlankSAV((GameVersion)Version, "PKHeX"));
        else
            MainPage = new AppShell(SaveUtil.GetBlankSAV((GameVersion)50, "PKHeX"));
    }
}
