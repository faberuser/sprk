using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_PlayerItemTimeDurationInfo
{
	public static PlayerItemTimeDurationInfo Parse(string s)
	{
		return Parse(s, new PlayerItemTimeDurationInfo());
	}

	public static PlayerItemTimeDurationInfo Parse(string s, PlayerItemTimeDurationInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static PlayerItemTimeDurationInfo Parse(IDictionary d)
	{
		return Parse(d, new PlayerItemTimeDurationInfo());
	}

	public static PlayerItemTimeDurationInfo Parse(IDictionary d, PlayerItemTimeDurationInfo o)
	{
		o.ItemIndex = BaseJsonMarshaler.ParseValueInt(d, "ItemIndex");
		o.StartTime = BaseJsonMarshaler.ParseValueString(d, "StartTime");
		o.EndTime = BaseJsonMarshaler.ParseValueString(d, "EndTime");
		return o;
	}

	public static string EncodeJsonString(PlayerItemTimeDurationInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(PlayerItemTimeDurationInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("ItemIndex", o.ItemIndex.ToString());
		if (o.StartTime != null)
		{
			dictionary.Add("StartTime", o.StartTime);
		}
		if (o.EndTime != null)
		{
			dictionary.Add("EndTime", o.EndTime);
		}
		return dictionary;
	}
}
