using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.GetGuildBasicInfo;

public static class JM_NShared_GetGuildBasicInfo_Request
{
	public static NShared.GetGuildBasicInfo.Request Parse(string s)
	{
		return Parse(s, new NShared.GetGuildBasicInfo.Request());
	}

	public static NShared.GetGuildBasicInfo.Request Parse(string s, NShared.GetGuildBasicInfo.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.GetGuildBasicInfo.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.GetGuildBasicInfo.Request());
	}

	public static NShared.GetGuildBasicInfo.Request Parse(IDictionary d, NShared.GetGuildBasicInfo.Request o)
	{
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.GetGuildBasicInfo.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.GetGuildBasicInfo.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
