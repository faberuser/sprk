using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_KingsPassSeaonInfo
{
	public static KingsPassSeaonInfo Parse(string s)
	{
		return Parse(s, new KingsPassSeaonInfo());
	}

	public static KingsPassSeaonInfo Parse(string s, KingsPassSeaonInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static KingsPassSeaonInfo Parse(IDictionary d)
	{
		return Parse(d, new KingsPassSeaonInfo());
	}

	public static KingsPassSeaonInfo Parse(IDictionary d, KingsPassSeaonInfo o)
	{
		o.SeasonIndex = BaseJsonMarshaler.ParseValueInt(d, "SeasonIndex");
		o.BeginTime = BaseJsonMarshaler.ParseValueString(d, "BeginTime");
		o.EndTime = BaseJsonMarshaler.ParseValueString(d, "EndTime");
		o.ShowYN = BaseJsonMarshaler.ParseValueBool(d, "ShowYN");
		o.CurrentLevel = BaseJsonMarshaler.ParseValueInt(d, "CurrentLevel");
		o.LastLevel = BaseJsonMarshaler.ParseValueInt(d, "LastLevel");
		return o;
	}

	public static string EncodeJsonString(KingsPassSeaonInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(KingsPassSeaonInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("SeasonIndex", o.SeasonIndex.ToString());
		if (o.BeginTime != null)
		{
			dictionary.Add("BeginTime", o.BeginTime);
		}
		if (o.EndTime != null)
		{
			dictionary.Add("EndTime", o.EndTime);
		}
		dictionary.Add("ShowYN", o.ShowYN.ToString());
		dictionary.Add("CurrentLevel", o.CurrentLevel.ToString());
		dictionary.Add("LastLevel", o.LastLevel.ToString());
		return dictionary;
	}
}
