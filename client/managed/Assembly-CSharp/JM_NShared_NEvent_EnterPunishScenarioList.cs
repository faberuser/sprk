using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NEvent;

public static class JM_NShared_NEvent_EnterPunishScenarioList
{
	public static EnterPunishScenarioList Parse(string s)
	{
		return Parse(s, new EnterPunishScenarioList());
	}

	public static EnterPunishScenarioList Parse(string s, EnterPunishScenarioList o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static EnterPunishScenarioList Parse(IDictionary d)
	{
		return Parse(d, new EnterPunishScenarioList());
	}

	public static EnterPunishScenarioList Parse(IDictionary d, EnterPunishScenarioList o)
	{
		o.GroupIndex = BaseJsonMarshaler.ParseValueInt(d, "GroupIndex");
		return o;
	}

	public static string EncodeJsonString(EnterPunishScenarioList o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(EnterPunishScenarioList o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("GroupIndex", o.GroupIndex.ToString());
		return dictionary;
	}
}
