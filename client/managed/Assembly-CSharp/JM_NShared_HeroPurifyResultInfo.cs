using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_HeroPurifyResultInfo
{
	public static HeroPurifyResultInfo Parse(string s)
	{
		return Parse(s, new HeroPurifyResultInfo());
	}

	public static HeroPurifyResultInfo Parse(string s, HeroPurifyResultInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static HeroPurifyResultInfo Parse(IDictionary d)
	{
		return Parse(d, new HeroPurifyResultInfo());
	}

	public static HeroPurifyResultInfo Parse(IDictionary d, HeroPurifyResultInfo o)
	{
		o.HeroIndex = BaseJsonMarshaler.ParseValueInt(d, "HeroIndex");
		o.AddValue = BaseJsonMarshaler.ParseValueShort(d, "AddValue");
		o.NewValue = BaseJsonMarshaler.ParseValueShort(d, "NewValue");
		return o;
	}

	public static string EncodeJsonString(HeroPurifyResultInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(HeroPurifyResultInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("HeroIndex", o.HeroIndex.ToString());
		dictionary.Add("AddValue", o.AddValue.ToString());
		dictionary.Add("NewValue", o.NewValue.ToString());
		return dictionary;
	}
}
