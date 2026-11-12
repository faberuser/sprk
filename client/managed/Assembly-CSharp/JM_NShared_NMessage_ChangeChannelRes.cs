using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NMessage;

public static class JM_NShared_NMessage_ChangeChannelRes
{
	public static ChangeChannelRes Parse(string s)
	{
		return Parse(s, new ChangeChannelRes());
	}

	public static ChangeChannelRes Parse(string s, ChangeChannelRes o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static ChangeChannelRes Parse(IDictionary d)
	{
		return Parse(d, new ChangeChannelRes());
	}

	public static ChangeChannelRes Parse(IDictionary d, ChangeChannelRes o)
	{
		o.Changed = BaseJsonMarshaler.ParseValueBool(d, "Changed");
		o.RequestId = BaseJsonMarshaler.ParseValueInt(d, "RequestId");
		return o;
	}

	public static string EncodeJsonString(ChangeChannelRes o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(ChangeChannelRes o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("Changed", o.Changed.ToString());
		dictionary.Add("RequestId", o.RequestId.ToString());
		return dictionary;
	}
}
