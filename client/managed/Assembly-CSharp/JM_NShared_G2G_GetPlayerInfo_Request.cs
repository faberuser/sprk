using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.G2G_GetPlayerInfo;

public static class JM_NShared_G2G_GetPlayerInfo_Request
{
	public static NShared.G2G_GetPlayerInfo.Request Parse(string s)
	{
		return Parse(s, new NShared.G2G_GetPlayerInfo.Request());
	}

	public static NShared.G2G_GetPlayerInfo.Request Parse(string s, NShared.G2G_GetPlayerInfo.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.G2G_GetPlayerInfo.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.G2G_GetPlayerInfo.Request());
	}

	public static NShared.G2G_GetPlayerInfo.Request Parse(IDictionary d, NShared.G2G_GetPlayerInfo.Request o)
	{
		o.AccountId = BaseJsonMarshaler.ParseValueLong(d, "AccountId");
		o.Name = BaseJsonMarshaler.ParseValueString(d, "Name");
		return o;
	}

	public static string EncodeJsonString(NShared.G2G_GetPlayerInfo.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.G2G_GetPlayerInfo.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("AccountId", o.AccountId.ToString());
		if (o.Name != null)
		{
			dictionary.Add("Name", o.Name);
		}
		return dictionary;
	}
}
