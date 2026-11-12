using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_ChattingActionData
{
	public static ChattingActionData Parse(string s)
	{
		return Parse(s, new ChattingActionData());
	}

	public static ChattingActionData Parse(string s, ChattingActionData o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static ChattingActionData Parse(IDictionary d)
	{
		return Parse(d, new ChattingActionData());
	}

	public static ChattingActionData Parse(IDictionary d, ChattingActionData o)
	{
		o.Name = BaseJsonMarshaler.ParseValueString(d, "Name");
		o.Data = BaseJsonMarshaler.ParseValueString(d, "Data");
		return o;
	}

	public static string EncodeJsonString(ChattingActionData o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(ChattingActionData o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		if (o.Name != null)
		{
			dictionary.Add("Name", o.Name);
		}
		if (o.Data != null)
		{
			dictionary.Add("Data", o.Data);
		}
		return dictionary;
	}
}
