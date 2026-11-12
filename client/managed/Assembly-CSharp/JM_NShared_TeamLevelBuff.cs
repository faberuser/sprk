using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_TeamLevelBuff
{
	public static TeamLevelBuff Parse(string s)
	{
		return Parse(s, new TeamLevelBuff());
	}

	public static TeamLevelBuff Parse(string s, TeamLevelBuff o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static TeamLevelBuff Parse(IDictionary d)
	{
		return Parse(d, new TeamLevelBuff());
	}

	public static TeamLevelBuff Parse(IDictionary d, TeamLevelBuff o)
	{
		o.OpenBuffLevel = BaseJsonMarshaler.ParseValueInt(d, "OpenBuffLevel");
		o.BuffReinforce = BaseJsonMarshaler.ParseValueInt(d, "BuffReinforce");
		o.UpdatedTime = BaseJsonMarshaler.ParseValueString(d, "UpdatedTime");
		return o;
	}

	public static string EncodeJsonString(TeamLevelBuff o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(TeamLevelBuff o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("OpenBuffLevel", o.OpenBuffLevel.ToString());
		dictionary.Add("BuffReinforce", o.BuffReinforce.ToString());
		if (o.UpdatedTime != null)
		{
			dictionary.Add("UpdatedTime", o.UpdatedTime);
		}
		return dictionary;
	}
}
