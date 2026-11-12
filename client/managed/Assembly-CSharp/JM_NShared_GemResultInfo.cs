using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_GemResultInfo
{
	public static GemResultInfo Parse(string s)
	{
		return Parse(s, new GemResultInfo());
	}

	public static GemResultInfo Parse(string s, GemResultInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static GemResultInfo Parse(IDictionary d)
	{
		return Parse(d, new GemResultInfo());
	}

	public static GemResultInfo Parse(IDictionary d, GemResultInfo o)
	{
		o.AddValue = BaseJsonMarshaler.ParseValueLong(d, "AddValue");
		o.FirstDiffClear = BaseJsonMarshaler.ParseValueByte(d, "FirstDiffClear");
		o.NewSysGem = BaseJsonMarshaler.ParseValueLong(d, "NewSysGem");
		o.NewPayGem = BaseJsonMarshaler.ParseValueLong(d, "NewPayGem");
		o.Sequence = BaseJsonMarshaler.ParseValueLong(d, "Sequence");
		return o;
	}

	public static string EncodeJsonString(GemResultInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(GemResultInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("AddValue", o.AddValue.ToString());
		dictionary.Add("FirstDiffClear", o.FirstDiffClear.ToString());
		dictionary.Add("NewSysGem", o.NewSysGem.ToString());
		dictionary.Add("NewPayGem", o.NewPayGem.ToString());
		dictionary.Add("Sequence", o.Sequence.ToString());
		return dictionary;
	}
}
