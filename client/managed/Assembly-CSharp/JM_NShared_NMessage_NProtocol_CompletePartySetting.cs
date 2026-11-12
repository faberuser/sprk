using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NMessage.NProtocol;

public static class JM_NShared_NMessage_NProtocol_CompletePartySetting
{
	public static CompletePartySetting Parse(string s)
	{
		return Parse(s, new CompletePartySetting());
	}

	public static CompletePartySetting Parse(string s, CompletePartySetting o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static CompletePartySetting Parse(IDictionary d)
	{
		return Parse(d, new CompletePartySetting());
	}

	public static CompletePartySetting Parse(IDictionary d, CompletePartySetting o)
	{
		o.GroupId = BaseJsonMarshaler.ParseValueLong(d, "GroupId");
		return o;
	}

	public static string EncodeJsonString(CompletePartySetting o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(CompletePartySetting o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("GroupId", o.GroupId.ToString());
		return dictionary;
	}
}
