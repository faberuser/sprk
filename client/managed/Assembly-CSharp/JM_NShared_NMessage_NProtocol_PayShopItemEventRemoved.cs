using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NMessage.NProtocol;

public static class JM_NShared_NMessage_NProtocol_PayShopItemEventRemoved
{
	public static PayShopItemEventRemoved Parse(string s)
	{
		return Parse(s, new PayShopItemEventRemoved());
	}

	public static PayShopItemEventRemoved Parse(string s, PayShopItemEventRemoved o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static PayShopItemEventRemoved Parse(IDictionary d)
	{
		return Parse(d, new PayShopItemEventRemoved());
	}

	public static PayShopItemEventRemoved Parse(IDictionary d, PayShopItemEventRemoved o)
	{
		o.Id = BaseJsonMarshaler.ParseValueLong(d, "Id");
		return o;
	}

	public static string EncodeJsonString(PayShopItemEventRemoved o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(PayShopItemEventRemoved o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("Id", o.Id.ToString());
		return dictionary;
	}
}
