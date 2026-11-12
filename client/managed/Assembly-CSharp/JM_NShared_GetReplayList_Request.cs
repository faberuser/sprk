using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.GetReplayList;

public static class JM_NShared_GetReplayList_Request
{
	public static NShared.GetReplayList.Request Parse(string s)
	{
		return Parse(s, new NShared.GetReplayList.Request());
	}

	public static NShared.GetReplayList.Request Parse(string s, NShared.GetReplayList.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.GetReplayList.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.GetReplayList.Request());
	}

	public static NShared.GetReplayList.Request Parse(IDictionary d, NShared.GetReplayList.Request o)
	{
		o.Type = BaseJsonMarshaler.ParseEnum<ReplayType>(d, "Type");
		o.Count = BaseJsonMarshaler.ParseValueInt(d, "Count");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.GetReplayList.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.GetReplayList.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("Type", o.Type.ToString());
		dictionary.Add("Count", o.Count.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
