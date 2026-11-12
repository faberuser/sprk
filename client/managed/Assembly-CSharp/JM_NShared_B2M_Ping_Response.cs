using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.B2M_Ping;

public static class JM_NShared_B2M_Ping_Response
{
	public static NShared.B2M_Ping.Response Parse(string s)
	{
		return Parse(s, new NShared.B2M_Ping.Response());
	}

	public static NShared.B2M_Ping.Response Parse(string s, NShared.B2M_Ping.Response o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.B2M_Ping.Response Parse(IDictionary d)
	{
		return Parse(d, new NShared.B2M_Ping.Response());
	}

	public static NShared.B2M_Ping.Response Parse(IDictionary d, NShared.B2M_Ping.Response o)
	{
		o.Result = BaseJsonMarshaler.ParseEnum<ResultType>(d, "Result");
		o.ErrorMessage = BaseJsonMarshaler.ParseValueString(d, "ErrorMessage");
		o.BattleIntegrityCheckInfos = BaseJsonMarshaler.ParseArray(d, "BattleIntegrityCheckInfos", JM_NShared_B2M_Ping_BattleIntegrityCheckInfo.Parse);
		o.ContentsValues = BaseJsonMarshaler.ParseArray(d, "ContentsValues", JM_NShared_ContentsValueInfo.Parse);
		o.ServerValues = BaseJsonMarshaler.ParseArray(d, "ServerValues", JM_NShared_ContentsValueInfo.Parse);
		o.ServerConstantValues = BaseJsonMarshaler.ParseArray(d, "ServerConstantValues", JM_NShared_ConstantData.Parse);
		return o;
	}

	public static string EncodeJsonString(NShared.B2M_Ping.Response o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.B2M_Ping.Response o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("Result", o.Result.ToString());
		if (o.ErrorMessage != null)
		{
			dictionary.Add("ErrorMessage", o.ErrorMessage);
		}
		dictionary.Add("BattleIntegrityCheckInfos", BaseJsonMarshaler.EncodeArray(o.BattleIntegrityCheckInfos, JM_NShared_B2M_Ping_BattleIntegrityCheckInfo.EncodeJsonObject));
		dictionary.Add("ContentsValues", BaseJsonMarshaler.EncodeArray(o.ContentsValues, JM_NShared_ContentsValueInfo.EncodeJsonObject));
		dictionary.Add("ServerValues", BaseJsonMarshaler.EncodeArray(o.ServerValues, JM_NShared_ContentsValueInfo.EncodeJsonObject));
		dictionary.Add("ServerConstantValues", BaseJsonMarshaler.EncodeArray(o.ServerConstantValues, JM_NShared_ConstantData.EncodeJsonObject));
		return dictionary;
	}
}
