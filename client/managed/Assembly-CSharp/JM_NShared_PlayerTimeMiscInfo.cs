using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_PlayerTimeMiscInfo
{
	public static PlayerTimeMiscInfo Parse(string s)
	{
		return Parse(s, new PlayerTimeMiscInfo());
	}

	public static PlayerTimeMiscInfo Parse(string s, PlayerTimeMiscInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static PlayerTimeMiscInfo Parse(IDictionary d)
	{
		return Parse(d, new PlayerTimeMiscInfo());
	}

	public static PlayerTimeMiscInfo Parse(IDictionary d, PlayerTimeMiscInfo o)
	{
		o.DailyPushReceivedTime = BaseJsonMarshaler.ParseValueString(d, "DailyPushReceivedTime");
		return o;
	}

	public static string EncodeJsonString(PlayerTimeMiscInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(PlayerTimeMiscInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		if (o.DailyPushReceivedTime != null)
		{
			dictionary.Add("DailyPushReceivedTime", o.DailyPushReceivedTime);
		}
		return dictionary;
	}
}
