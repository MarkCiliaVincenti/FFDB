﻿using R5.FFDB.Core.Models;
using R5.FFDB.DbProviders.PostgreSql.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace R5.FFDB.DbProviders.PostgreSql.Models.Entities.WeekStats
{
	[TableName("ffdb.week_stats_pass")]
	[CompositePrimaryKeys("player_id", "season", "week")]
	public class WeekStatsPassSql : WeekStatsPlayerSql
	{
		[NotNull]
		[ForeignKey(typeof(PlayerSql), "id")]
		[Column("player_id", PostgresDataType.UUID)]
		public override Guid PlayerId { get; set; }
		
		[ForeignKey(typeof(TeamSql), "id")]
		[Column("team_id", PostgresDataType.INT)]
		public override int? TeamId { get; set; }

		[NotNull]
		[Column("season", PostgresDataType.INT)]
		public override int Season { get; set; }

		[NotNull]
		[Column("week", PostgresDataType.INT)]
		public override int Week { get; set; }

		[WeekStatColumn("pass_attempts", WeekStatType.Pass_Attempts)]
		public double? PassAttempts { get; set; }

		[WeekStatColumn("pass_completions", WeekStatType.Pass_Completions)]
		public double? PassCompletions { get; set; }

		[WeekStatColumn("pass_yards", WeekStatType.Pass_Yards)]
		public double? PassYards { get; set; }

		[WeekStatColumn("pass_touchdowns", WeekStatType.Pass_Touchdowns)]
		public double? PassTouchdowns { get; set; }

		[WeekStatColumn("pass_interceptions", WeekStatType.Pass_Interceptions)]
		public double? PassInterceptions { get; set; }

		[WeekStatColumn("pass_sacked", WeekStatType.Pass_Sacked)]
		public double? PassSacked { get; set; }
	}
}