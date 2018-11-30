﻿using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using R5.FFDB.Components;
using R5.FFDB.Components.PlayerProfile;
using R5.FFDB.Components.Roster.Sources.NFLWebTeam;
using R5.FFDB.Components.Roster.Sources.NFLWebTeam.Models;
using R5.FFDB.Core.Data;
using R5.FFDB.Core.Models;
using R5.FFDB.Core.Sources;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevTester.Testers
{
	public interface IPlayerProfileTester
	{
		Task DownloadRosterPagesAsync();
		Task FetchSavePlayerProfilesAsync(bool downloadRosterPages);
	}

	public class PlayerProfileTester : IPlayerProfileTester
	{
		private ILogger<PlayerProfileTester> _logger { get; }
		private IWebRequestClient _webRequestClient { get; }
		private IPlayerProfileSource _playerProfileSource { get; }
		private DataDirectoryPath _dataPath { get; }

		public PlayerProfileTester(
			ILogger<PlayerProfileTester> logger,
			IWebRequestClient webRequestClient,
			IPlayerProfileSource playerProfileSource,
			DataDirectoryPath dataPath)
		{
			_logger = logger;
			_webRequestClient = webRequestClient;
			_playerProfileSource = playerProfileSource;
			_dataPath = dataPath;
		}

		public async Task DownloadRosterPagesAsync()
		{
			List<Team> teams = Teams.Get();

			foreach (Team team in teams.Where(t => t.RosterSourceUris.ContainsKey(RosterSourceKeys.NFLWebTeam)))
			{
				string page = await _webRequestClient.GetStringAsync(team.RosterSourceUris[RosterSourceKeys.NFLWebTeam], throttle: true);
				await File.WriteAllTextAsync(_dataPath.Temp.RosterPages + $"{team.Abbreviation}.html", page);
			}
		}

		public async Task FetchSavePlayerProfilesAsync(bool downloadRosterPages)
		{
			List<Roster> rosters = GetRosters();

			//List<string> nflIds = rosters
			//	.SelectMany(r => r.Players)
			//	.Select(p => p.NflId)
			//	.Distinct()
			//	.ToList();

			List<string> playerIds = rosters
				.SelectMany(r => r.Players)
				.Select(p => p.NflId)
				.Distinct()
				.ToList();

			_logger.LogDebug($"Found '{playerIds.Count}' players to fetch profile data for.");

			await _playerProfileSource.FetchAndSavePlayerDataFilesAsync(playerIds);

			_logger.LogDebug("Finished fetching player profile data by rosters.");
		}

		private List<Roster> GetRosters()
		{
			var rosters = new List<Roster>();

			List<Team> teams = Teams.Get();//.GetRange(0, 1);

			foreach (var team in teams)
			{
				string pagePath = _dataPath.Temp.RosterPages + $"{team.Abbreviation}.html";
				var pageHtml = File.ReadAllText(pagePath);

				var page = new HtmlDocument();
				page.LoadHtml(pageHtml);

				List<RosterPlayer> players = RosterScraper.ExtractPlayers(page)
					.Select(NFLWebRosterPlayer.ToCoreEntity)
					.ToList();

				rosters.Add(new Roster
				{
					TeamId = team.Id,
					TeamAbbreviation = team.Abbreviation,
					Players = players
				});
			}

			return rosters;
		}
	}
}
