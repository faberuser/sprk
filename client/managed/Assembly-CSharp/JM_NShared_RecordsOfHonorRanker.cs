using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_RecordsOfHonorRanker
{
	public static RecordsOfHonorRanker Parse(string s)
	{
		return Parse(s, new RecordsOfHonorRanker());
	}

	public static RecordsOfHonorRanker Parse(string s, RecordsOfHonorRanker o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static RecordsOfHonorRanker Parse(IDictionary d)
	{
		return Parse(d, new RecordsOfHonorRanker());
	}

	public static RecordsOfHonorRanker Parse(IDictionary d, RecordsOfHonorRanker o)
	{
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
		o.Deck = BaseJsonMarshaler.ParseValueString(d, "Deck");
		o.Index = BaseJsonMarshaler.ParseValueInt(d, "Index");
		return o;
	}

	public static string EncodeJsonString(RecordsOfHonorRanker o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(RecordsOfHonorRanker o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
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
		if (o.Deck != null)
		{
			dictionary.Add("Deck", o.Deck);
		}
		dictionary.Add("Index", o.Index.ToString());
		return dictionary;
	}
}
