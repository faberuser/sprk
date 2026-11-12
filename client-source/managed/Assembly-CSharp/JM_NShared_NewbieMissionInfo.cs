using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_NewbieMissionInfo
{
	public static NewbieMissionInfo Parse(string s)
	{
		return Parse(s, new NewbieMissionInfo());
	}

	public static NewbieMissionInfo Parse(string s, NewbieMissionInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NewbieMissionInfo Parse(IDictionary d)
	{
		return Parse(d, new NewbieMissionInfo());
	}

	public static NewbieMissionInfo Parse(IDictionary d, NewbieMissionInfo o)
	{
		o.MissionIndex = BaseJsonMarshaler.ParseValueInt(d, "MissionIndex");
		o.UpdateTime = BaseJsonMarshaler.ParseValueString(d, "UpdateTime");
		return o;
	}

	public static string EncodeJsonString(NewbieMissionInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NewbieMissionInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("MissionIndex", o.MissionIndex.ToString());
		if (o.UpdateTime != null)
		{
			dictionary.Add("UpdateTime", o.UpdateTime);
		}
		return dictionary;
	}
}
