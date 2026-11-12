using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_TowerInfo
{
	public static TowerInfo Parse(string s)
	{
		return Parse(s, new TowerInfo());
	}

	public static TowerInfo Parse(string s, TowerInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static TowerInfo Parse(IDictionary d)
	{
		return Parse(d, new TowerInfo());
	}

	public static TowerInfo Parse(IDictionary d, TowerInfo o)
	{
		o.TowerIndex = BaseJsonMarshaler.ParseValueInt(d, "TowerIndex");
		o.CompletedFloor = BaseJsonMarshaler.ParseValueInt(d, "CompletedFloor");
		o.CompletedTime = BaseJsonMarshaler.ParseValueString(d, "CompletedTime");
		o.ResetCount = BaseJsonMarshaler.ParseValueInt(d, "ResetCount");
		o.ResetTime = BaseJsonMarshaler.ParseValueString(d, "ResetTime");
		o.ReceiveRewardTime = BaseJsonMarshaler.ParseValueString(d, "ReceiveRewardTime");
		return o;
	}

	public static string EncodeJsonString(TowerInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(TowerInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("TowerIndex", o.TowerIndex.ToString());
		dictionary.Add("CompletedFloor", o.CompletedFloor.ToString());
		if (o.CompletedTime != null)
		{
			dictionary.Add("CompletedTime", o.CompletedTime);
		}
		dictionary.Add("ResetCount", o.ResetCount.ToString());
		if (o.ResetTime != null)
		{
			dictionary.Add("ResetTime", o.ResetTime);
		}
		if (o.ReceiveRewardTime != null)
		{
			dictionary.Add("ReceiveRewardTime", o.ReceiveRewardTime);
		}
		return dictionary;
	}
}
