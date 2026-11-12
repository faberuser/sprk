using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.ConfirmSoulStone;

public static class JM_NShared_ConfirmSoulStone_Request
{
	public static NShared.ConfirmSoulStone.Request Parse(string s)
	{
		return Parse(s, new NShared.ConfirmSoulStone.Request());
	}

	public static NShared.ConfirmSoulStone.Request Parse(string s, NShared.ConfirmSoulStone.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.ConfirmSoulStone.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.ConfirmSoulStone.Request());
	}

	public static NShared.ConfirmSoulStone.Request Parse(IDictionary d, NShared.ConfirmSoulStone.Request o)
	{
		o.SoulStoneItemIndex = BaseJsonMarshaler.ParseValueInt(d, "SoulStoneItemIndex");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.ConfirmSoulStone.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.ConfirmSoulStone.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("SoulStoneItemIndex", o.SoulStoneItemIndex.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
