﻿using PKHeX.Core;

namespace pk9reader;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();
		
		MainPage = new AppShell(new SAV9SV());
	}
}
