using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_MileageResultInfo
{
	public static MileageResultInfo Parse(string s)
	{
		return Parse(s, new MileageResultInfo());
	}

	public static MileageResultInfo Parse(string s, MileageResultInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static MileageResultInfo Parse(IDictionary d)
	{
		return Parse(d, new MileageResultInfo());
	}

	public static MileageResultInfo Parse(IDictionary d, MileageResultInfo o)
	{
		o.AddValue = BaseJsonMarshaler.ParseValueLong(d, "AddValue");
		o.NewValue = BaseJsonMarshaler.ParseValueLong(d, "NewValue");
		return o;
	}

	public static string EncodeJsonString(MileageResultInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(MileageResultInfo o)
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
