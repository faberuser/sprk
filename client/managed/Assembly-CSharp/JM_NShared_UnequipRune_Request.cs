using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.UnequipRune;

public static class JM_NShared_UnequipRune_Request
{
	public static NShared.UnequipRune.Request Parse(string s)
	{
		return Parse(s, new NShared.UnequipRune.Request());
	}

	public static NShared.UnequipRune.Request Parse(string s, NShared.UnequipRune.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.UnequipRune.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.UnequipRune.Request());
	}

	public static NShared.UnequipRune.Request Parse(IDictionary d, NShared.UnequipRune.Request o)
	{
		o.HeroIndex = BaseJsonMarshaler.ParseValueInt(d, "HeroIndex");
		o.SlotNum = BaseJsonMarshaler.ParseValueInt(d, "SlotNum");
		o.RunePage = BaseJsonMarshaler.ParseValueInt(d, "RunePage");
		o.IsPreserve = BaseJsonMarshaler.ParseValueBool(d, "IsPreserve");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.UnequipRune.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.UnequipRune.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("HeroIndex", o.HeroIndex.ToString());
		dictionary.Add("SlotNum", o.SlotNum.ToString());
		dictionary.Add("RunePage", o.RunePage.ToString());
		dictionary.Add("IsPreserve", o.IsPreserve.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
