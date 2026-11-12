using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.InsertUserData;

public static class JM_NShared_InsertUserData_Request
{
	public static NShared.InsertUserData.Request Parse(string s)
	{
		return Parse(s, new NShared.InsertUserData.Request());
	}

	public static NShared.InsertUserData.Request Parse(string s, NShared.InsertUserData.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.InsertUserData.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.InsertUserData.Request());
	}

	public static NShared.InsertUserData.Request Parse(IDictionary d, NShared.InsertUserData.Request o)
	{
		o.Region = BaseJsonMarshaler.ParseValueString(d, "Region");
		o.AccountId = BaseJsonMarshaler.ParseValueInt(d, "AccountId");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.InsertUserData.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.InsertUserData.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		if (o.Region != null)
		{
			dictionary.Add("Region", o.Region);
		}
		dictionary.Add("AccountId", o.AccountId.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
