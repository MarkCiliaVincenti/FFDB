﻿using R5.FFDB.Components.CoreData.Dynamic.Rosters.Sources.V1.Models;
using R5.FFDB.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace R5.FFDB.Components.CoreData.Dynamic.Rosters.Sources.V1.Mappers
{
	public interface IToCoreDataMapper : IAsyncMapper<RosterVersioned, Roster, Team> { }

	public class ToCoreDataMapper : IToCoreDataMapper
	{
		public Task<Roster> MapAsync(RosterVersioned versionedModel, Team team)
		{
			return Task.FromResult(
				RosterVersioned.ToCoreEntity(versionedModel));
		}
	}
}
