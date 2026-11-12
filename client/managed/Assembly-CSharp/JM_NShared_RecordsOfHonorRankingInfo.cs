using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_RecordsOfHonorRankingInfo
{
	public static RecordsOfHonorRankingInfo Parse(string s)
	{
		return Parse(s, new RecordsOfHonorRankingInfo());
	}

	public static RecordsOfHonorRankingInfo Parse(string s, RecordsOfHonorRankingInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static RecordsOfHonorRankingInfo Parse(IDictionary d)
	{
		return Parse(d, new RecordsOfHonorRankingInfo());
	}

	public static RecordsOfHonorRankingInfo Parse(IDictionary d, RecordsOfHonorRankingInfo o)
	{
		o.ContentType = BaseJsonMarshaler.ParseEnum<RecordsOfHonorContentType>(d, "ContentType");
		o.RankingType = BaseJsonMarshaler.ParseEnum<RecordsOfHonorRankingType>(d, "RankingType");
		o.Season = BaseJsonMarshaler.ParseValueInt(d, "Season");
		o.Rank = BaseJsonMarshaler.ParseValueInt(d, "Rank");
		o.ServerGroup = BaseJsonMarshaler.ParseValueString(d, "ServerGroup");
		o.Id = BaseJsonMarshaler.ParseValueLong(d, "Id");
		o.Name = BaseJsonMarshaler.ParseValueString(d, "Name");
		o.Level = BaseJsonMarshaler.ParseValueInt(d, "Level");
		o.PictureIndex = BaseJsonMarshaler.ParseValueInt(d, "PictureIndex");
		o.CountryCode = BaseJsonMarshaler.ParseValueString(d, "CountryCode");
		o.Score = BaseJsonMarshaler.ParseValueLong(d, "Score");
		o.MasterAvatarIndex = BaseJsonMarshaler.ParseValueString(d, "MasterAvatarIndex");
		o.Win = BaseJsonMarshaler.ParseValueInt(d, "Win");
		o.Lose = BaseJsonMarshaler.ParseValueInt(d, "Lose");
		o.SuccessiveWin = BaseJsonMarshaler.ParseValueInt(d, "SuccessiveWin");
		o.Deck = BaseJsonMarshaler.ParseArray(d, "Deck", JM_NShared_DeckHeroInfo.Parse);
		o.Index = BaseJsonMarshaler.ParseValueInt(d, "Index");
		return o;
	}

	public static string EncodeJsonString(RecordsOfHonorRankingInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(RecordsOfHonorRankingInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("ContentType", o.ContentType.ToString());
		dictionary.Add("RankingType", o.RankingType.ToString());
		dictionary.Add("Season", o.Season.ToString());
		dictionary.Add("Rank", o.Rank.ToString());
		if (o.ServerGroup != null)
		{
			dictionary.Add("ServerGroup", o.ServerGroup);
		}
		dictionary.Add("Id", o.Id.ToString());
		if (o.Name != null)
		{
			dictionary.Add("Name", o.Name);
		}
		dictionary.Add("Level", o.Level.ToString());
		dictionary.Add("PictureIndex", o.PictureIndex.ToString());
		if (o.CountryCode != null)
		{
			dictionary.Add("CountryCode", o.CountryCode);
		}
		dictionary.Add("Score", o.Score.ToString());
		if (o.MasterAvatarIndex != null)
		{
			dictionary.Add("MasterAvatarIndex", o.MasterAvatarIndex);
		}
		dictionary.Add("Win", o.Win.ToString());
		dictionary.Add("Lose", o.Lose.ToString());
		dictionary.Add("SuccessiveWin", o.SuccessiveWin.ToString());
		dictionary.Add("Deck", BaseJsonMarshaler.EncodeArray(o.Deck, JM_NShared_DeckHeroInfo.EncodeJsonObject));
		dictionary.Add("Index", o.Index.ToString());
		return dictionary;
	}
}
