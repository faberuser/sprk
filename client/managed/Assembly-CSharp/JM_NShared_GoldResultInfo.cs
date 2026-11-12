using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_GoldResultInfo
{
	public static GoldResultInfo Parse(string s)
	{
		return Parse(s, new GoldResultInfo());
	}

	public static GoldResultInfo Parse(string s, GoldResultInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static GoldResultInfo Parse(IDictionary d)
	{
		return Parse(d, new GoldResultInfo());
	}

	public static GoldResultInfo Parse(IDictionary d, GoldResultInfo o)
	{
		o.AddValue = BaseJsonMarshaler.ParseValueLong(d, "AddValue");
		o.AddBoosterValue = BaseJsonMarshaler.ParseValueLong(d, "AddBoosterValue");
		o.NewValue = BaseJsonMarshaler.ParseValueLong(d, "NewValue");
		o.Sequence = BaseJsonMarshaler.ParseValueLong(d, "Sequence");
		return o;
	}

	public static string EncodeJsonString(GoldResultInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(GoldResultInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("AddValue", o.AddValue.ToString());
		dictionary.Add("AddBoosterValue", o.AddBoosterValue.ToString());
		dictionary.Add("NewValue", o.NewValue.ToString());
		dictionary.Add("Sequence", o.Sequence.ToString());
		return dictionary;
	}
}
