using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_GloryPointResultInfo
{
	public static GloryPointResultInfo Parse(string s)
	{
		return Parse(s, new GloryPointResultInfo());
	}

	public static GloryPointResultInfo Parse(string s, GloryPointResultInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static GloryPointResultInfo Parse(IDictionary d)
	{
		return Parse(d, new GloryPointResultInfo());
	}

	public static GloryPointResultInfo Parse(IDictionary d, GloryPointResultInfo o)
	{
		o.Type = BaseJsonMarshaler.ParseEnum<GloryPointType>(d, "Type");
		o.AddValue = BaseJsonMarshaler.ParseValueLong(d, "AddValue");
		o.NewValue = BaseJsonMarshaler.ParseValueLong(d, "NewValue");
		return o;
	}

	public static string EncodeJsonString(GloryPointResultInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(GloryPointResultInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("Type", o.Type.ToString());
		dictionary.Add("AddValue", o.AddValue.ToString());
		dictionary.Add("NewValue", o.NewValue.ToString());
		return dictionary;
	}
}
