﻿using Newtonsoft.Json.Linq;
using R5.FFDB.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R5.FFDB.Components.ValueProviders
{
	public class LatestWeekValue : AsyncValueProvider<WeekInfo>
	{
		private IWebRequestClient _webRequestClient { get; }

		public LatestWeekValue(IWebRequestClient webRequestClient)
		{
			_webRequestClient = webRequestClient;
		}

		protected override async Task<WeekInfo> ResolveValueAsync()
		{
			// doesn't matter which week we choose, it'll always return
			// the NFL's current state info
			JObject weekStats = await getWeekStatsAsync();

			(int currentSeason, int currentWeek) = getCurrentWeekInfo(weekStats);

			return new WeekInfo(currentSeason, currentWeek);

			// local functions
			async Task<JObject> getWeekStatsAsync()
			{
				string uri = Endpoints.Api.WeekStats(2018, 1);

				string weekStatsJson = await _webRequestClient.GetStringAsync(uri, throttle: false);

				return JObject.Parse(weekStatsJson);
			}

			(int season, int week) getCurrentWeekInfo(JObject stats)
			{
				JObject games = stats["games"].ToObject<JObject>();

				string gameId = games.Properties().Select(p => p.Name).First();

				int season = games[gameId]["season"].ToObject<int>();
				int week = games[gameId]["state"]["week"].ToObject<int>();

				bool isCompleted = games[gameId]["state"]["isWeekGamesCompleted"].ToObject<bool>();
				if (!isCompleted)
				{
					week = week - 1;
				}

				return (season, week);
			}
		}
	}
}