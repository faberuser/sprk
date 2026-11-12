using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.UnlockEquip;

public static class JM_NShared_UnlockEquip_Request
{
	public static NShared.UnlockEquip.Request Parse(string s)
	{
		return Parse(s, new NShared.UnlockEquip.Request());
	}

	public static NShared.UnlockEquip.Request Parse(string s, NShared.UnlockEquip.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.UnlockEquip.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.UnlockEquip.Request());
	}

	public static NShared.UnlockEquip.Request Parse(IDictionary d, NShared.UnlockEquip.Request o)
	{
		o.HeroIndex = BaseJsonMarshaler.ParseValueInt(d, "HeroIndex");
		o.HeroPartIndex = BaseJsonMarshaler.ParseValueInt(d, "HeroPartIndex");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.UnlockEquip.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.UnlockEquip.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("HeroIndex", o.HeroIndex.ToString());
		dictionary.Add("HeroPartIndex", o.HeroPartIndex.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
