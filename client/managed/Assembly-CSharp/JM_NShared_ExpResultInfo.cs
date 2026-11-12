using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_ExpResultInfo
{
	public static ExpResultInfo Parse(string s)
	{
		return Parse(s, new ExpResultInfo());
	}

	public static ExpResultInfo Parse(string s, ExpResultInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static ExpResultInfo Parse(IDictionary d)
	{
		return Parse(d, new ExpResultInfo());
	}

	public static ExpResultInfo Parse(IDictionary d, ExpResultInfo o)
	{
		o.AddValue = BaseJsonMarshaler.ParseValueInt(d, "AddValue");
		o.NewValue = BaseJsonMarshaler.ParseValueInt(d, "NewValue");
		o.OldLevel = BaseJsonMarshaler.ParseValueInt(d, "OldLevel");
		o.NewLevel = BaseJsonMarshaler.ParseValueInt(d, "NewLevel");
		return o;
	}

	public static string EncodeJsonString(ExpResultInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(ExpResultInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("AddValue", o.AddValue.ToString());
		dictionary.Add("NewValue", o.NewValue.ToString());
		dictionary.Add("OldLevel", o.OldLevel.ToString());
		dictionary.Add("NewLevel", o.NewLevel.ToString());
		return dictionary;
	}
}
