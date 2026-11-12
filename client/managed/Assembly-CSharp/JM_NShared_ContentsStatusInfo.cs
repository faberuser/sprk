using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_ContentsStatusInfo
{
	public static ContentsStatusInfo Parse(string s)
	{
		return Parse(s, new ContentsStatusInfo());
	}

	public static ContentsStatusInfo Parse(string s, ContentsStatusInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static ContentsStatusInfo Parse(IDictionary d)
	{
		return Parse(d, new ContentsStatusInfo());
	}

	public static ContentsStatusInfo Parse(IDictionary d, ContentsStatusInfo o)
	{
		o.ContentsType = BaseJsonMarshaler.ParseValueString(d, "ContentsType");
		o.ContentsSubType = BaseJsonMarshaler.ParseValueString(d, "ContentsSubType");
		o.Status = BaseJsonMarshaler.ParseValueString(d, "Status");
		return o;
	}

	public static string EncodeJsonString(ContentsStatusInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(ContentsStatusInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		if (o.ContentsType != null)
		{
			dictionary.Add("ContentsType", o.ContentsType);
		}
		if (o.ContentsSubType != null)
		{
			dictionary.Add("ContentsSubType", o.ContentsSubType);
		}
		if (o.Status != null)
		{
			dictionary.Add("Status", o.Status);
		}
		return dictionary;
	}
}
