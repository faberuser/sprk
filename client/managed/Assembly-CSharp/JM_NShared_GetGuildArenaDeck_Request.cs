using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.GetGuildArenaDeck;

public static class JM_NShared_GetGuildArenaDeck_Request
{
	public static NShared.GetGuildArenaDeck.Request Parse(string s)
	{
		return Parse(s, new NShared.GetGuildArenaDeck.Request());
	}

	public static NShared.GetGuildArenaDeck.Request Parse(string s, NShared.GetGuildArenaDeck.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.GetGuildArenaDeck.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.GetGuildArenaDeck.Request());
	}

	public static NShared.GetGuildArenaDeck.Request Parse(IDictionary d, NShared.GetGuildArenaDeck.Request o)
	{
		o.SeasonIndex = BaseJsonMarshaler.ParseValueInt(d, "SeasonIndex");
		o.MemberId = BaseJsonMarshaler.ParseValueLong(d, "MemberId");
		o.EnemyServerGroup = BaseJsonMarshaler.ParseValueString(d, "EnemyServerGroup");
		o.EnemyGuildId = BaseJsonMarshaler.ParseValueLong(d, "EnemyGuildId");
		o.EnemyAccountId = BaseJsonMarshaler.ParseValueLong(d, "EnemyAccountId");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.GetGuildArenaDeck.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.GetGuildArenaDeck.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("SeasonIndex", o.SeasonIndex.ToString());
		dictionary.Add("MemberId", o.MemberId.ToString());
		if (o.EnemyServerGroup != null)
		{
			dictionary.Add("EnemyServerGroup", o.EnemyServerGroup);
		}
		dictionary.Add("EnemyGuildId", o.EnemyGuildId.ToString());
		dictionary.Add("EnemyAccountId", o.EnemyAccountId.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
