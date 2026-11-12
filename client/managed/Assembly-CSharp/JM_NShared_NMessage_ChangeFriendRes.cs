using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NMessage;

public static class JM_NShared_NMessage_ChangeFriendRes
{
	public static ChangeFriendRes Parse(string s)
	{
		return Parse(s, new ChangeFriendRes());
	}

	public static ChangeFriendRes Parse(string s, ChangeFriendRes o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static ChangeFriendRes Parse(IDictionary d)
	{
		return Parse(d, new ChangeFriendRes());
	}

	public static ChangeFriendRes Parse(IDictionary d, ChangeFriendRes o)
	{
		o.Result = BaseJsonMarshaler.ParseEnum<ChangeFriendResType>(d, "Result");
		o.RequestId = BaseJsonMarshaler.ParseValueInt(d, "RequestId");
		return o;
	}

	public static string EncodeJsonString(ChangeFriendRes o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(ChangeFriendRes o)
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
