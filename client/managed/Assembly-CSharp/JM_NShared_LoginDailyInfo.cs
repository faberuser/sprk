using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_LoginDailyInfo
{
	public static LoginDailyInfo Parse(string s)
	{
		return Parse(s, new LoginDailyInfo());
	}

	public static LoginDailyInfo Parse(string s, LoginDailyInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static LoginDailyInfo Parse(IDictionary d)
	{
		return Parse(d, new LoginDailyInfo());
	}

	public static LoginDailyInfo Parse(IDictionary d, LoginDailyInfo o)
	{
		o.LoginDailyIndex = BaseJsonMarshaler.ParseValueInt(d, "LoginDailyIndex");
		o.CompletedCount = BaseJsonMarshaler.ParseValueInt(d, "CompletedCount");
		o.CompletedTime = BaseJsonMarshaler.ParseValueString(d, "CompletedTime");
		return o;
	}

	public static string EncodeJsonString(LoginDailyInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(LoginDailyInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("LoginDailyIndex", o.LoginDailyIndex.ToString());
		dictionary.Add("CompletedCount", o.CompletedCount.ToString());
		if (o.CompletedTime != null)
		{
			dictionary.Add("CompletedTime", o.CompletedTime);
		}
		return dictionary;
	}
}
