using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_BattleBanPickStartReq
{
	public static BattleBanPickStartReq Parse(string s)
	{
		return Parse(s, new BattleBanPickStartReq());
	}

	public static BattleBanPickStartReq Parse(string s, BattleBanPickStartReq o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static BattleBanPickStartReq Parse(IDictionary d)
	{
		return Parse(d, new BattleBanPickStartReq());
	}

	public static BattleBanPickStartReq Parse(IDictionary d, BattleBanPickStartReq o)
	{
		o.RequestId = BaseJsonMarshaler.ParseValueInt(d, "RequestId");
		return o;
	}

	public static string EncodeJsonString(BattleBanPickStartReq o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(BattleBanPickStartReq o)
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
