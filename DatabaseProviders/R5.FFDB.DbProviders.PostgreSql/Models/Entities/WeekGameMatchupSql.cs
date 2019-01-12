﻿using R5.FFDB.Core.Models;
using R5.FFDB.DbProviders.PostgreSql.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace R5.FFDB.DbProviders.PostgreSql.Models.Entities
{
	[TableName(Table.WeekGameMatchup)]
	[CompositePrimaryKeys("season", "week", "home_team_id", "away_team_id")]
	public class WeekGameMatchupSql : SqlEntity
	{
		[NotNull]
		[Column("season", PostgresDataType.INT)]
		public int Season { get; set; }

		[NotNull]
		[Column("week", PostgresDataType.INT)]
		public int Week { get; set; }

		[NotNull]
		[ForeignKey(typeof(TeamSql), "id")]
		[Column("home_team_id", PostgresDataType.INT)]
		public int HomeTeamId { get; set; }

		[NotNull]
		[ForeignKey(typeof(TeamSql), "id")]
		[Column("away_team_id", PostgresDataType.INT)]
		public int AwayTeamId { get; set; }

		[NotNull]
		[Column("nfl_game_id", PostgresDataType.TEXT)]
		public string NflGameId { get; set; }

		[NotNull]
		[Column("gsis_game_id", PostgresDataType.TEXT)]
		public string GsisGameId { get; set; }

		public static WeekGameMatchupSql FromCoreEntity(WeekGameMatchup entity)
		{
			return new WeekGameMatchupSql
			{
				Season = entity.Season,
				Week = entity.Week,
				HomeTeamId = entity.HomeTeamId,
				AwayTeamId = entity.AwayTeamId,
				NflGameId = entity.NflGameId,
				GsisGameId = entity.GsisGameId
			};
		}

		public override string PrimaryKeyMatchCondition()
		{
			return $"season = {Season} AND week = {Week} AND home_team_id = '{HomeTeamId}' AND away_team_id = '{AwayTeamId}'";
		}
	}
}