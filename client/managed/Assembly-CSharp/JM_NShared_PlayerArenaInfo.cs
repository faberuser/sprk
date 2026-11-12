using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_PlayerArenaInfo
{
	public static PlayerArenaInfo Parse(string s)
	{
		return Parse(s, new PlayerArenaInfo());
	}

	public static PlayerArenaInfo Parse(string s, PlayerArenaInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static PlayerArenaInfo Parse(IDictionary d)
	{
		return Parse(d, new PlayerArenaInfo());
	}

	public static PlayerArenaInfo Parse(IDictionary d, PlayerArenaInfo o)
	{
		o.ArenaType = BaseJsonMarshaler.ParseEnum<MatchArenaType>(d, "ArenaType");
		o.SeasonIndex = BaseJsonMarshaler.ParseValueInt(d, "SeasonIndex");
		o.BestTier = BaseJsonMarshaler.ParseValueInt(d, "BestTier");
		o.CurrentTier = BaseJsonMarshaler.ParseValueInt(d, "CurrentTier");
		o.CachedAccountInfo = BaseJsonMarshaler.ParseValueString(d, "CachedAccountInfo");
		o.UpdatedTime = BaseJsonMarshaler.ParseValueString(d, "UpdatedTime");
		return o;
	}

	public static string EncodeJsonString(PlayerArenaInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(PlayerArenaInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("ArenaType", o.ArenaType.ToString());
		dictionary.Add("SeasonIndex", o.SeasonIndex.ToString());
		dictionary.Add("BestTier", o.BestTier.ToString());
		dictionary.Add("CurrentTier", o.CurrentTier.ToString());
		if (o.CachedAccountInfo != null)
		{
			dictionary.Add("CachedAccountInfo", o.CachedAccountInfo);
		}
		if (o.UpdatedTime != null)
		{
			dictionary.Add("UpdatedTime", o.UpdatedTime);
		}
		return dictionary;
	}
}
