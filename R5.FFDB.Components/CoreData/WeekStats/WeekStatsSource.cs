﻿using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using R5.FFDB.Components.CoreData.TeamGameHistory;
using R5.FFDB.Components.CoreData.WeekStats.Models;
using R5.FFDB.Components.Http;
using R5.FFDB.Components.Resolvers;
using R5.FFDB.Components.ValueProviders;
using R5.FFDB.Core.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R5.FFDB.Components.CoreData.WeekStats
{
	public interface IWeekStatsSource : ICoreDataSource
	{
		Task FetchForWeekAsync(WeekInfo week);
		Task FetchAllAsync();
		Task FetchForWeeksAsync(List<WeekInfo> weeks);
	}

	public class WeekStatsSource : IWeekStatsSource
	{
		public string Label => "Week Stats";

		private ILogger<WeekStatsSource> _logger { get; }
		private DataDirectoryPath _dataPath { get; }
		private IWebRequestClient _webRequestClient { get; }
		private AvailableWeeksValue _availableWeeks { get; }

		public WeekStatsSource(
			ILogger<WeekStatsSource> logger,
			DataDirectoryPath dataPath,
			IWebRequestClient webRequestClient,
			AvailableWeeksValue availableWeeks)
		{
			_logger = logger;
			_dataPath = dataPath;
			_webRequestClient = webRequestClient;
			_availableWeeks = availableWeeks;
		}

		public async Task FetchForWeekAsync(WeekInfo week)
		{
			_logger.LogInformation($"Beginning fetching of week stats for {week}.");

			string filePath = _dataPath.Static.WeekStats + $"{week.Season}-{week.Week}.json";
			if (File.Exists(filePath))
			{
				_logger.LogInformation($"Week stats file already exists for {week}. Will not fetch.");
				return;
			}

			string stats = await FetchWeekAsync(week);

			_logger.LogTrace($"Saving week stats JSON response for {week} to '{filePath}'.");
			File.WriteAllText(filePath, stats);

			_logger.LogInformation($"Finished saving week stats for {week}.");

			_logger.LogInformation("Finished fetching week stats.");
		}

		public async Task FetchAllAsync()
		{
			_logger.LogInformation("Beginning fetching of week stats for all available weeks.");

			List<WeekInfo> availableWeeks = await _availableWeeks.GetAsync();

			await FetchForWeeksAsync(availableWeeks);

			_logger.LogInformation("Finished fetching week stats for all available weeks.");
		}

		public async Task FetchForWeeksAsync(List<WeekInfo> weeks)
		{
			_logger.LogInformation($"Beginning fetching of week stats for {weeks.Count} week(s).");
			_logger.LogTrace($"Fetching for weeks: {string.Join(", ", weeks)}");

			foreach (WeekInfo week in weeks)
			{
				string filePath = _dataPath.Static.WeekStats + $"{week.Season}-{week.Week}.json";
				if (File.Exists(filePath))
				{
					_logger.LogInformation($"Week stats file already exists for {week}. Will not fetch.");
					return;
				}

				string stats = await FetchWeekAsync(week);

				_logger.LogTrace($"Saving week stats JSON response for {week} to '{filePath}'.");
				File.WriteAllText(filePath, stats);

				_logger.LogInformation($"Finished saving week stats for {week}.");
			}

			_logger.LogInformation("Finished fetching week stats.");
		}

		private async Task<string> FetchWeekAsync(WeekInfo week)
		{
			string uri = Endpoints.Api.WeekStats(week.Season, week.Week);
			_logger.LogDebug($"Beginning week stats fetch for {week} from '{uri}'.");

			try
			{
				string stats = await _webRequestClient.GetStringAsync(uri);
				_logger.LogInformation($"Finished fetching week stats for {week}.");
				return stats;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"Failed to fetch week stats for {week} from '{uri}'.");
				throw;
			}
		}

		public async Task CheckHealthAsync()
		{
			var testWeeks = new List<WeekInfo>
			{
				new WeekInfo(2010, 1),
				new WeekInfo(2018, 1)
			};

			_logger.LogInformation($"Beginning health check for '{Label}' source. "
				+ $"Will perform checks on weeks: {string.Join(", ", testWeeks)}");
			
			foreach(var week in testWeeks)
			{
				_logger.LogDebug($"Checking health using week {week}.");

				await CheckHealthForWeekAsync(week);

				_logger.LogInformation($"Health check passed for week {week}.");
			}

			_logger.LogInformation($"Health check successfully passed for '{Label}' source.");
		}

		private async Task CheckHealthForWeekAsync(WeekInfo week)
		{
			string jsonResponse = null;
			try
			{
				string uri = Endpoints.Api.WeekStats(week.Season, week.Week);
				jsonResponse = await _webRequestClient.GetStringAsync(uri);
				
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"API request for week stats for {week} failed.");
				throw;
			}
			
			try
			{
				JsonConvert.DeserializeObject<WeekStatsJson>(jsonResponse);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"Failed to deserialize week stats API response to expected model.");
				throw;
			}
		}
	}
}
