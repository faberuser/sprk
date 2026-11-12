using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NEvent;

public static class JM_NShared_NEvent_EnterMenu
{
	public static EnterMenu Parse(string s)
	{
		return Parse(s, new EnterMenu());
	}

	public static EnterMenu Parse(string s, EnterMenu o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static EnterMenu Parse(IDictionary d)
	{
		return Parse(d, new EnterMenu());
	}

	public static EnterMenu Parse(IDictionary d, EnterMenu o)
	{
		o.Window = BaseJsonMarshaler.ParseValueString(d, "Window");
		o.SceneTriggerSequance = BaseJsonMarshaler.ParseValueInt(d, "SceneTriggerSequance");
		return o;
	}

	public static string EncodeJsonString(EnterMenu o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(EnterMenu o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		if (o.Window != null)
		{
			dictionary.Add("Window", o.Window);
		}
		dictionary.Add("SceneTriggerSequance", o.SceneTriggerSequance.ToString());
		return dictionary;
	}
}
