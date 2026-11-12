using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_QuickCompleteCollectResultInfo
{
	public static QuickCompleteCollectResultInfo Parse(string s)
	{
		return Parse(s, new QuickCompleteCollectResultInfo());
	}

	public static QuickCompleteCollectResultInfo Parse(string s, QuickCompleteCollectResultInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static QuickCompleteCollectResultInfo Parse(IDictionary d)
	{
		return Parse(d, new QuickCompleteCollectResultInfo());
	}

	public static QuickCompleteCollectResultInfo Parse(IDictionary d, QuickCompleteCollectResultInfo o)
	{
		o.AddValue = BaseJsonMarshaler.ParseValueInt(d, "AddValue");
		o.NewValue = BaseJsonMarshaler.ParseValueInt(d, "NewValue");
		return o;
	}

	public static string EncodeJsonString(QuickCompleteCollectResultInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(QuickCompleteCollectResultInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("AddValue", o.AddValue.ToString());
		dictionary.Add("NewValue", o.NewValue.ToString());
		return dictionary;
	}
}
