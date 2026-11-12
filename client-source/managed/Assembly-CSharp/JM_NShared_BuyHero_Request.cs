using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.BuyHero;

public static class JM_NShared_BuyHero_Request
{
	public static NShared.BuyHero.Request Parse(string s)
	{
		return Parse(s, new NShared.BuyHero.Request());
	}

	public static NShared.BuyHero.Request Parse(string s, NShared.BuyHero.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.BuyHero.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.BuyHero.Request());
	}

	public static NShared.BuyHero.Request Parse(IDictionary d, NShared.BuyHero.Request o)
	{
		o.HeroIndex = BaseJsonMarshaler.ParseValueInt(d, "HeroIndex");
		o.ItemIndex = BaseJsonMarshaler.ParseValueInt(d, "ItemIndex");
		o.BuyGem = BaseJsonMarshaler.ParseValueInt(d, "BuyGem");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.BuyHero.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.BuyHero.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("HeroIndex", o.HeroIndex.ToString());
		dictionary.Add("ItemIndex", o.ItemIndex.ToString());
		dictionary.Add("BuyGem", o.BuyGem.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
