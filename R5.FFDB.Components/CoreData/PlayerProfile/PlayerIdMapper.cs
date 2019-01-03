﻿using R5.FFDB.Components.CoreData.PlayerProfile;
using R5.FFDB.Components.CoreData.PlayerProfile.Models;
using R5.FFDB.Components.CoreData.PlayerProfile.Values;
using R5.FFDB.Components.ValueProviders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace R5.FFDB.Components.CoreData.PlayerProfile
{
	public interface IPlayerIdMapper
	{
		string NflFromGsis(string gsisId);
		string NflFromEsb(string esbId);
		bool TryGetNflFromGsis(string gsisId, out string nflId);
		bool TryGetNflFromEsb(string esbId, out string nflId);
	}
	public class PlayerIdMapper : IPlayerIdMapper
	{
		private PlayerProfilesValue _playerProfiles { get; }
		private Mappings _mappings { get; set; }

		public PlayerIdMapper(PlayerProfilesValue playerProfiles)
		{
			_playerProfiles = playerProfiles;
		}

		public string NflFromGsis(string gsisId)
		{
			InitializeMapsIfNotSet();

			if (!_mappings.GsisNflIdMap.TryGetValue(gsisId, out string nflId))
			{
				throw new InvalidOperationException($"Gsis id '{gsisId}' was not found in mappings.");
			}

			return nflId;
		}

		public string NflFromEsb(string esbId)
		{
			InitializeMapsIfNotSet();

			if (!_mappings.EsbNflIdMap.TryGetValue(esbId, out string nflId))
			{
				throw new InvalidOperationException($"Esb id '{esbId}' was not found in mappings.");
			}

			return nflId;
		}

		public bool TryGetNflFromGsis(string gsisId, out string nflId)
		{
			InitializeMapsIfNotSet();

			if (_mappings.GsisNflIdMap.TryGetValue(gsisId, out string id))
			{
				nflId = id;
				return true;
			}

			nflId = null;
			return false;
		}

		public bool TryGetNflFromEsb(string esbId, out string nflId)
		{
			InitializeMapsIfNotSet();

			if (_mappings.EsbNflIdMap.TryGetValue(esbId, out string id))
			{
				nflId = id;
				return true;
			}

			nflId = null;
			return false;
		}

		private void InitializeMapsIfNotSet()
		{
			if (_mappings != null)
			{
				return;
			}

			var mappings = new Mappings
			{
				GsisNflIdMap = new Dictionary<string, string>(),
				EsbNflIdMap = new Dictionary<string, string>()
			};
			
			_playerProfiles.Get()
				.ForEach(p =>
				{
					mappings.GsisNflIdMap[p.GsisId] = p.NflId;
					mappings.EsbNflIdMap[p.EsbId] = p.NflId;
				});

			_mappings = mappings;
		}

		private class Mappings
		{
			public Dictionary<string, string> GsisNflIdMap { get; set; }
			public Dictionary<string, string> EsbNflIdMap { get; set; }
		}
	}
}