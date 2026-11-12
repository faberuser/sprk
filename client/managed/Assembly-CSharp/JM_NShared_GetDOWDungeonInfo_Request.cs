using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.GetDOWDungeonInfo;

public static class JM_NShared_GetDOWDungeonInfo_Request
{
	public static NShared.GetDOWDungeonInfo.Request Parse(string s)
	{
		return Parse(s, new NShared.GetDOWDungeonInfo.Request());
	}

	public static NShared.GetDOWDungeonInfo.Request Parse(string s, NShared.GetDOWDungeonInfo.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.GetDOWDungeonInfo.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.GetDOWDungeonInfo.Request());
	}

	public static NShared.GetDOWDungeonInfo.Request Parse(IDictionary d, NShared.GetDOWDungeonInfo.Request o)
	{
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.GetDOWDungeonInfo.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.GetDOWDungeonInfo.Request o)
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
