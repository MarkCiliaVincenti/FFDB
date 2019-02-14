﻿using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using R5.FFDB.Core.Database;
using R5.FFDB.Core.Entities;
using R5.FFDB.DbProviders.Mongo.Collections;
using R5.FFDB.DbProviders.Mongo.Documents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R5.FFDB.DbProviders.Mongo.DatabaseContext
{
	public class TeamDbContext : DbContextBase, ITeamDbContext
	{
		public TeamDbContext(
			Func<IMongoDatabase> getDatabase,
			ILoggerFactory loggerFactory)
			: base(getDatabase, loggerFactory)
		{
		}

		public async Task AddAsync(List<Team> teams)
		{
			if (teams == null)
			{
				throw new ArgumentNullException(nameof(teams), "Teams must be provided.");
			}

			ILogger<TeamDbContext> logger = GetLogger<TeamDbContext>();
			var collectionName = CollectionNames.GetForType<TeamDocument>();

			logger.LogDebug($"Adding {teams.Count} teams to '{collectionName}' collection..");

			MongoDbContext mongoDbContext = GetMongoDbContext();

			HashSet<int> existing = await GetExistingTeamIdsAsync(mongoDbContext);

			List<TeamDocument> missing = teams
				.Where(t => !existing.Contains(t.Id))
				.Select(TeamDocument.FromCoreEntity)
				.ToList();
			
			await mongoDbContext.InsertManyAsync(missing);

			logger.LogInformation($"Added teams to '{collectionName}' collection.");
		}

		private async Task<HashSet<int>> GetExistingTeamIdsAsync(MongoDbContext mongoDbContext)
		{
			var findOptions = new FindOptions<TeamDocument, int>
			{
				Projection = Builders<TeamDocument>.Projection
					.Expression(t => t.Id)
			};

			List<int> ids = await mongoDbContext.FindAsync(findOptions: findOptions) ?? new List<int>();

			return ids.ToHashSet();
		}

		// first set all player's team ids to null, then update
		public async Task UpdateRosterMappingsAsync(List<Roster> rosters)
		{
			if (rosters == null)
			{
				throw new ArgumentNullException(nameof(rosters), "Rosters must be provided.");
			}

			ILogger<TeamDbContext> logger = GetLogger<TeamDbContext>();
			var collectionName = CollectionNames.GetForType<PlayerDocument>();

			logger.LogDebug($"Updating roster mappings..");

			MongoDbContext mongoDbContext = GetMongoDbContext();
			
			await ClearRosterMappingsAsync(mongoDbContext);
			
			Dictionary<string, Guid> nflIdMap = await GetNflIdMapAsync(mongoDbContext);

			foreach (Roster roster in rosters)
			{
				await UpdateForRosterAsync(roster, nflIdMap, mongoDbContext);
			}

			logger.LogInformation($"Updated roster mappings for players in '{collectionName}' collection.");

			throw new NotImplementedException();
		}

		private Task ClearRosterMappingsAsync(MongoDbContext mongoDbContext)
		{
			var clearUpdate = Builders<PlayerDocument>.Update.Set(p => p.TeamId, null);

			return mongoDbContext.UpdateAsync(clearUpdate);
		}

		private async Task<Dictionary<string, Guid>> GetNflIdMapAsync(MongoDbContext mongoDbContext)
		{
			var findOptions = new FindOptions<PlayerDocument>
			{
				Projection = Builders<PlayerDocument>.Projection
					.Include(p => p.Id)
					.Include(p => p.NflId)
			};

			List<PlayerDocument> players = await mongoDbContext.FindAsync(findOptions: findOptions);

			return players.ToDictionary(p => p.NflId, p => p.Id, StringComparer.OrdinalIgnoreCase);
		}

		private async Task UpdateForRosterAsync(Roster roster, 
			Dictionary<string, Guid> nflIdMap, MongoDbContext mongoDbContext)
		{
			var playerIds = roster.Players
				.Where(p => nflIdMap.ContainsKey(p.NflId))
				.Select(p => nflIdMap[p.NflId]);

			var update = Builders<PlayerDocument>.Update.Set(p => p.TeamId, roster.TeamId);
			var filter = Builders<PlayerDocument>.Filter.In(p => p.Id, playerIds);

			UpdateResult result = await mongoDbContext.UpdateAsync(update, filter);
			if (result.MatchedCount == 0)
			{
				throw new InvalidOperationException($"Updating roster mappings failed for team '{roster.TeamAbbreviation}'.");
			}
		}
	}
}
