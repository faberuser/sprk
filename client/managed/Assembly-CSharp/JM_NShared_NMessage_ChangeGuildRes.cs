using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NMessage;

public static class JM_NShared_NMessage_ChangeGuildRes
{
	public static ChangeGuildRes Parse(string s)
	{
		return Parse(s, new ChangeGuildRes());
	}

	public static ChangeGuildRes Parse(string s, ChangeGuildRes o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static ChangeGuildRes Parse(IDictionary d)
	{
		return Parse(d, new ChangeGuildRes());
	}

	public static ChangeGuildRes Parse(IDictionary d, ChangeGuildRes o)
	{
		o.Result = BaseJsonMarshaler.ParseEnum<ChangeGuildResType>(d, "Result");
		o.RequestId = BaseJsonMarshaler.ParseValueInt(d, "RequestId");
		return o;
	}

	public static string EncodeJsonString(ChangeGuildRes o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(ChangeGuildRes o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("Result", o.Result.ToString());
		dictionary.Add("RequestId", o.RequestId.ToString());
		return dictionary;
	}
}
