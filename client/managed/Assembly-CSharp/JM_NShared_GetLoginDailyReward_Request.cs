using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.GetLoginDailyReward;

public static class JM_NShared_GetLoginDailyReward_Request
{
	public static NShared.GetLoginDailyReward.Request Parse(string s)
	{
		return Parse(s, new NShared.GetLoginDailyReward.Request());
	}

	public static NShared.GetLoginDailyReward.Request Parse(string s, NShared.GetLoginDailyReward.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.GetLoginDailyReward.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.GetLoginDailyReward.Request());
	}

	public static NShared.GetLoginDailyReward.Request Parse(IDictionary d, NShared.GetLoginDailyReward.Request o)
	{
		o.LoginDailyIndex = BaseJsonMarshaler.ParseValueInt(d, "LoginDailyIndex");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.GetLoginDailyReward.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.GetLoginDailyReward.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("LoginDailyIndex", o.LoginDailyIndex.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
