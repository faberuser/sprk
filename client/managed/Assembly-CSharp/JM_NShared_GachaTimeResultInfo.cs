using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_GachaTimeResultInfo
{
	public static GachaTimeResultInfo Parse(string s)
	{
		return Parse(s, new GachaTimeResultInfo());
	}

	public static GachaTimeResultInfo Parse(string s, GachaTimeResultInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static GachaTimeResultInfo Parse(IDictionary d)
	{
		return Parse(d, new GachaTimeResultInfo());
	}

	public static GachaTimeResultInfo Parse(IDictionary d, GachaTimeResultInfo o)
	{
		o.GachaIndex = BaseJsonMarshaler.ParseValueInt(d, "GachaIndex");
		o.NextFreeRemainTime = BaseJsonMarshaler.ParseValueInt(d, "NextFreeRemainTime");
		o.ResetRemainTime = BaseJsonMarshaler.ParseValueInt(d, "ResetRemainTime");
		return o;
	}

	public static string EncodeJsonString(GachaTimeResultInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(GachaTimeResultInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("GachaIndex", o.GachaIndex.ToString());
		dictionary.Add("NextFreeRemainTime", o.NextFreeRemainTime.ToString());
		dictionary.Add("ResetRemainTime", o.ResetRemainTime.ToString());
		return dictionary;
	}
}
