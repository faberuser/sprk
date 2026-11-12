using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.InitClassBuff;

public static class JM_NShared_InitClassBuff_Request
{
	public static NShared.InitClassBuff.Request Parse(string s)
	{
		return Parse(s, new NShared.InitClassBuff.Request());
	}

	public static NShared.InitClassBuff.Request Parse(string s, NShared.InitClassBuff.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.InitClassBuff.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.InitClassBuff.Request());
	}

	public static NShared.InitClassBuff.Request Parse(IDictionary d, NShared.InitClassBuff.Request o)
	{
		o.TagType = BaseJsonMarshaler.ParseValueInt(d, "TagType");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.InitClassBuff.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.InitClassBuff.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("TagType", o.TagType.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
