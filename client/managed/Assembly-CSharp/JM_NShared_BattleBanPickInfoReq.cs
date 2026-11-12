using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_BattleBanPickInfoReq
{
	public static BattleBanPickInfoReq Parse(string s)
	{
		return Parse(s, new BattleBanPickInfoReq());
	}

	public static BattleBanPickInfoReq Parse(string s, BattleBanPickInfoReq o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static BattleBanPickInfoReq Parse(IDictionary d)
	{
		return Parse(d, new BattleBanPickInfoReq());
	}

	public static BattleBanPickInfoReq Parse(IDictionary d, BattleBanPickInfoReq o)
	{
		o.RequestId = BaseJsonMarshaler.ParseValueInt(d, "RequestId");
		return o;
	}

	public static string EncodeJsonString(BattleBanPickInfoReq o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(BattleBanPickInfoReq o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("RequestId", o.RequestId.ToString());
		return dictionary;
	}
}
