using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_EclipseContinuousBattleReq
{
	public static EclipseContinuousBattleReq Parse(string s)
	{
		return Parse(s, new EclipseContinuousBattleReq());
	}

	public static EclipseContinuousBattleReq Parse(string s, EclipseContinuousBattleReq o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static EclipseContinuousBattleReq Parse(IDictionary d)
	{
		return Parse(d, new EclipseContinuousBattleReq());
	}

	public static EclipseContinuousBattleReq Parse(IDictionary d, EclipseContinuousBattleReq o)
	{
		o.IsContinuousBattle = BaseJsonMarshaler.ParseValueBool(d, "IsContinuousBattle");
		o.RequestId = BaseJsonMarshaler.ParseValueInt(d, "RequestId");
		return o;
	}

	public static string EncodeJsonString(EclipseContinuousBattleReq o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(EclipseContinuousBattleReq o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("IsContinuousBattle", o.IsContinuousBattle.ToString());
		dictionary.Add("RequestId", o.RequestId.ToString());
		return dictionary;
	}
}
