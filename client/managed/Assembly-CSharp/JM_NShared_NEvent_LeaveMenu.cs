using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NEvent;

public static class JM_NShared_NEvent_LeaveMenu
{
	public static LeaveMenu Parse(string s)
	{
		return Parse(s, new LeaveMenu());
	}

	public static LeaveMenu Parse(string s, LeaveMenu o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static LeaveMenu Parse(IDictionary d)
	{
		return Parse(d, new LeaveMenu());
	}

	public static LeaveMenu Parse(IDictionary d, LeaveMenu o)
	{
		o.Window = BaseJsonMarshaler.ParseValueString(d, "Window");
		return o;
	}

	public static string EncodeJsonString(LeaveMenu o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(LeaveMenu o)
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
		return dictionary;
	}
}
