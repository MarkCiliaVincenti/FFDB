﻿using System.Collections.Generic;

namespace R5.FFDB.Components.PlayerData
{
	public interface IPlayerDataSource
	{
		Core.Models.PlayerData GetPlayerData(string nflId);
		List<Core.Models.PlayerData> GetPlayerData(List<string> nflIds);
	}
}
