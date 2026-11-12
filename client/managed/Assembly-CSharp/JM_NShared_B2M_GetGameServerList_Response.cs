using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.B2M_GetGameServerList;

public static class JM_NShared_B2M_GetGameServerList_Response
{
	public static NShared.B2M_GetGameServerList.Response Parse(string s)
	{
		return Parse(s, new NShared.B2M_GetGameServerList.Response());
	}

	public static NShared.B2M_GetGameServerList.Response Parse(string s, NShared.B2M_GetGameServerList.Response o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.B2M_GetGameServerList.Response Parse(IDictionary d)
	{
		return Parse(d, new NShared.B2M_GetGameServerList.Response());
	}

	public static NShared.B2M_GetGameServerList.Response Parse(IDictionary d, NShared.B2M_GetGameServerList.Response o)
	{
		o.Result = BaseJsonMarshaler.ParseEnum<ResultType>(d, "Result");
		o.GameServerHosts = BaseJsonMarshaler.ParseArrayCast(d, "GameServerHosts", BaseJsonMarshaler.CastToString);
		o.IsBlocked = BaseJsonMarshaler.ParseValueBool(d, "IsBlocked");
		o.MultithreadEnabled = BaseJsonMarshaler.ParseValueBool(d, "MultithreadEnabled");
		return o;
	}

	public static string EncodeJsonString(NShared.B2M_GetGameServerList.Response o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.B2M_GetGameServerList.Response o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("Result", o.Result.ToString());
		dictionary.Add("GameServerHosts", BaseJsonMarshaler.EncodeArray(o.GameServerHosts));
		dictionary.Add("IsBlocked", o.IsBlocked.ToString());
		dictionary.Add("MultithreadEnabled", o.MultithreadEnabled.ToString());
		return dictionary;
	}
}
