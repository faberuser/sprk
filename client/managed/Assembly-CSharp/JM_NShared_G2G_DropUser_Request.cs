using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.G2G_DropUser;

public static class JM_NShared_G2G_DropUser_Request
{
	public static NShared.G2G_DropUser.Request Parse(string s)
	{
		return Parse(s, new NShared.G2G_DropUser.Request());
	}

	public static NShared.G2G_DropUser.Request Parse(string s, NShared.G2G_DropUser.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.G2G_DropUser.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.G2G_DropUser.Request());
	}

	public static NShared.G2G_DropUser.Request Parse(IDictionary d, NShared.G2G_DropUser.Request o)
	{
		o.AccountId = BaseJsonMarshaler.ParseValueLong(d, "AccountId");
		return o;
	}

	public static string EncodeJsonString(NShared.G2G_DropUser.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.G2G_DropUser.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("AccountId", o.AccountId.ToString());
		return dictionary;
	}
}
