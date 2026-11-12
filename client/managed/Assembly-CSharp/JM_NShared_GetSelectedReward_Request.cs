using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.GetSelectedReward;

public static class JM_NShared_GetSelectedReward_Request
{
	public static NShared.GetSelectedReward.Request Parse(string s)
	{
		return Parse(s, new NShared.GetSelectedReward.Request());
	}

	public static NShared.GetSelectedReward.Request Parse(string s, NShared.GetSelectedReward.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.GetSelectedReward.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.GetSelectedReward.Request());
	}

	public static NShared.GetSelectedReward.Request Parse(IDictionary d, NShared.GetSelectedReward.Request o)
	{
		o.ChapterIndex = BaseJsonMarshaler.ParseValueInt(d, "ChapterIndex");
		o.DungeonIndex = BaseJsonMarshaler.ParseValueInt(d, "DungeonIndex");
		o.ItemCodes = BaseJsonMarshaler.ParseArrayCast(d, "ItemCodes", BaseJsonMarshaler.CastToString);
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.GetSelectedReward.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.GetSelectedReward.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("ChapterIndex", o.ChapterIndex.ToString());
		dictionary.Add("DungeonIndex", o.DungeonIndex.ToString());
		dictionary.Add("ItemCodes", BaseJsonMarshaler.EncodeArray(o.ItemCodes));
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
