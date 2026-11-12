using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_HeroTranscendResultInfo
{
	public static HeroTranscendResultInfo Parse(string s)
	{
		return Parse(s, new HeroTranscendResultInfo());
	}

	public static HeroTranscendResultInfo Parse(string s, HeroTranscendResultInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static HeroTranscendResultInfo Parse(IDictionary d)
	{
		return Parse(d, new HeroTranscendResultInfo());
	}

	public static HeroTranscendResultInfo Parse(IDictionary d, HeroTranscendResultInfo o)
	{
		o.HeroIndex = BaseJsonMarshaler.ParseValueInt(d, "HeroIndex");
		o.AddPointValue = BaseJsonMarshaler.ParseValueInt(d, "AddPointValue");
		o.NewPointValue = BaseJsonMarshaler.ParseValueInt(d, "NewPointValue");
		return o;
	}

	public static string EncodeJsonString(HeroTranscendResultInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(HeroTranscendResultInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("HeroIndex", o.HeroIndex.ToString());
		dictionary.Add("AddPointValue", o.AddPointValue.ToString());
		dictionary.Add("NewPointValue", o.NewPointValue.ToString());
		return dictionary;
	}
}
