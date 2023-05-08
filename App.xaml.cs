using PKHeX.Core;

namespace PKHeXMAUI;

public partial class App : Application
{
	public App()
	{
        Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Mgo+DSMBaFt+QHJqVk1hXk5Hd0BLVGpAblJ3T2ZQdVt5ZDU7a15RRnVfR11rSXZWf0ZkUXddcw==;Mgo+DSMBPh8sVXJ1S0R+X1pFdEBBXHxAd1p/VWJYdVt5flBPcDwsT3RfQF5jTH5bdk1mUHxZeXRWRQ==;ORg4AjUWIQA/Gnt2VFhiQlJPd11dXmJWd1p/THNYflR1fV9DaUwxOX1dQl9gSXtTfkVrXHZdd31XRmM=;MTk2NjA5MUAzMjMxMmUzMjJlMzNBNFJXaWxVeEJ2VFFjWGNHVitkU3FhZjltb0gxQk1pYzc5bnFvMUJ2RmZBPQ==;MTk2NjA5MkAzMjMxMmUzMjJlMzNiZjZnSmhzVkI0bkNPeGlJZlNFdHB5eUFSL2hkUHlHVWROc05zb1picjE4PQ==;NRAiBiAaIQQuGjN/V0d+Xk9HfV5AQmBIYVp/TGpJfl96cVxMZVVBJAtUQF1hSn5Wdk1jUXpXc3xVRGhf;MTk2NjA5NEAzMjMxMmUzMjJlMzNMNTZnd0d3UmZyVnNSelhMT0xUL2dlTmdSRDcyUThyQnRwOWdjZkRKbHFvPQ==;MTk2NjA5NUAzMjMxMmUzMjJlMzNOQTVYblo1L0ZoT1lqMmdCT0V1KzhOT1JQbU5JL3VPRXE0R3EzNnMyOFRzPQ==;Mgo+DSMBMAY9C3t2VFhiQlJPd11dXmJWd1p/THNYflR1fV9DaUwxOX1dQl9gSXtTfkVrXHZdeHVUR2M=;MTk2NjA5N0AzMjMxMmUzMjJlMzNPaUZRa2g1Nk1QV2I2TlNJenlqK1hZaGJkM2FXYXQ1eHd4bVBiL1gwMU9jPQ==;MTk2NjA5OEAzMjMxMmUzMjJlMzNDdlVHejR3NTEvdHozd01TQnhaS0RMZWI2ZmwrdGYxVXZMOG90SDVuSGh3PQ==;MTk2NjA5OUAzMjMxMmUzMjJlMzNMNTZnd0d3UmZyVnNSelhMT0xUL2dlTmdSRDcyUThyQnRwOWdjZkRKbHFvPQ==");
        InitializeComponent();

        var Version = Preferences.Default.Get("SaveFile", 50);
        if (PSettings.RememberLastSave)
            MainPage = new AppShell(SaveUtil.GetBlankSAV((GameVersion)Version, "PKHeX"));
        else
            MainPage = new AppShell(SaveUtil.GetBlankSAV((GameVersion)50, "PKHeX"));
    }
}
