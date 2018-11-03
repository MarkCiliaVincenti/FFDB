﻿using System;
using System.Collections.Generic;
using System.Text;

namespace R5.FFDB.Core.Components.FantasyApi
{
	public class FantasyApiConfig
	{
		public string WeekStatsDownloadPath { get; set; } = @"D:\Repos\ffdb_weekstat_downloads\"; // temp hardcoded
		public int RequestDelayMilliseconds { get; set; } = 1000; // for safety, dont wanna be banned from the API
	}
}
