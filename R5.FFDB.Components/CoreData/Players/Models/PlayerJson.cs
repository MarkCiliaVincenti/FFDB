﻿//using Newtonsoft.Json;
//using R5.FFDB.Core.Entities;
//using System;
//using System.Collections.Generic;
//using System.Text;

//namespace R5.FFDB.Components.CoreData.Players.Models
//{
//	// static data retrieved from api and scraping
//	public class PlayerJson
//	{
//		[JsonProperty("nflId")]
//		public string NflId { get; set; }

//		[JsonProperty("esbId")]
//		public string EsbId { get; set; }

//		[JsonProperty("gsisId")]
//		public string GsisId { get; set; }

//		[JsonProperty("pictureUri")]
//		public string PictureUri { get; set; }

//		[JsonProperty("firstName")]
//		public string FirstName { get; set; }

//		[JsonProperty("lastName")]
//		public string LastName { get; set; }

//		[JsonProperty("height")]
//		public int Height { get; set; }

//		[JsonProperty("weight")]
//		public int Weight { get; set; }

//		[JsonProperty("dateOfBirth")]
//		public DateTime DateOfBirth { get; set; }

//		[JsonProperty("college")]
//		public string College { get; set; }

//		public static Player ToCoreEntity(PlayerJson json)
//		{
//			return new Player
//			{
//				NflId = json.NflId,
//				EsbId = json.EsbId,
//				GsisId = json.GsisId,
//				FirstName = json.FirstName,
//				LastName = json.LastName,
//				Height = json.Height,
//				Weight = json.Weight,
//				DateOfBirth = json.DateOfBirth,
//				College = json.College
//			};
//		}
//	}
//}
