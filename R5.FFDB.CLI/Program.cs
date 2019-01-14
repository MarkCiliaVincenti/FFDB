﻿using HtmlAgilityPack;
using Newtonsoft.Json;
using R5.FFDB.CLI.Configuration;
using R5.FFDB.Components.CoreData.WeekStats.Models;
using R5.FFDB.DbProviders.PostgreSql.DatabaseProvider;
using R5.FFDB.Engine;
using Serilog;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;
using CM = R5.FFDB.CLI.ConsoleManager;

namespace R5.FFDB.CLI
{
	public class Program
	{
		public static async Task Main(string[] args)
		{
			try
			{
				args = new string[] { "setup", "--force" };//
				RunInfoBase runInfo = GetRunInfoFromArgs(args);

				FfdbConfig config = FileConfigResolver.FromFile(runInfo.ConfigFilePath);

				FfdbEngine engine = GetConfiguredEngine(config);



				return;
			}
			catch (Exception ex)
			{
				CM.WriteError(ex.Message);
				return;
			}

			Console.ReadKey();
		}

		private static RunInfoBase GetRunInfoFromArgs(string[] args)
		{
			RunInfoBuilder.RunInfoBuilder builder = ConfigureBuilder.Get();

			var runInfoObject = builder.Build(args);
			if (runInfoObject == null)
			{
				throw new InvalidOperationException("There was an error parsing program args.");
			}

			var runInfo = runInfoObject as RunInfoBase;
			if (runInfo == null)
			{
				throw new InvalidOperationException("There was an error parsing program args.");
			}

			return runInfo;
		}

		private static FfdbEngine GetConfiguredEngine(FfdbConfig config)
		{
			var setup = new EngineSetup();

			setup.SetRootDataDirectoryPath(config.RootDataPath);

			if (config.WebRequest.RandomizedThrottle != null)
			{
				setup.WebRequest.SetRandomizedThrottle(
					config.WebRequest.RandomizedThrottle.Min,
					config.WebRequest.RandomizedThrottle.Max);
			}
			else
			{
				setup.WebRequest.SetThrottle(config.WebRequest.ThrottleMilliseconds);
			}

			var rollingInterval = Enum.Parse<RollingInterval>(config.Logging.RollingInterval);
			var logLevel = Enum.Parse<LogEventLevel>(config.Logging.LogLevel);

			setup.Logging
				.SetLogDirectory(config.Logging.Directory)
				.SetRollingInterval(rollingInterval)
				.SetLogLevel(logLevel);

			if (config.Logging.MaxBytes.HasValue)
			{
				setup.Logging.SetMaxBytes(config.Logging.MaxBytes.Value);
			}
			if (config.Logging.RollOnFileSizeLimit)
			{
				setup.Logging.RollOnFileSizeLimit();
			}
				
			if (config.PostgreSql != null)
			{
				setup.UsePostgreSql(config.PostgreSql);
			}
			else if (config.Mongo != null)
			{
				setup.UseMongo(config.Mongo);
			}

			return setup.Create();
		}
	}
}
