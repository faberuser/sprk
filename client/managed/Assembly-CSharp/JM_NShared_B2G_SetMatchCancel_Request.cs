using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.B2G_SetMatchCancel;

public static class JM_NShared_B2G_SetMatchCancel_Request
{
	public static NShared.B2G_SetMatchCancel.Request Parse(string s)
	{
		return Parse(s, new NShared.B2G_SetMatchCancel.Request());
	}

	public static NShared.B2G_SetMatchCancel.Request Parse(string s, NShared.B2G_SetMatchCancel.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.B2G_SetMatchCancel.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.B2G_SetMatchCancel.Request());
	}

	public static NShared.B2G_SetMatchCancel.Request Parse(IDictionary d, NShared.B2G_SetMatchCancel.Request o)
	{
		o.SeasonIndex = BaseJsonMarshaler.ParseValueInt(d, "SeasonIndex");
		o.MatchUid = BaseJsonMarshaler.ParseValueLong(d, "MatchUid");
		o.AccountId = BaseJsonMarshaler.ParseValueLong(d, "AccountId");
		o.ArenaType = BaseJsonMarshaler.ParseEnum<MatchArenaType>(d, "ArenaType");
		return o;
	}

	public static string EncodeJsonString(NShared.B2G_SetMatchCancel.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.B2G_SetMatchCancel.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("SeasonIndex", o.SeasonIndex.ToString());
		dictionary.Add("MatchUid", o.MatchUid.ToString());
		dictionary.Add("AccountId", o.AccountId.ToString());
		dictionary.Add("ArenaType", o.ArenaType.ToString());
		return dictionary;
	}
}
