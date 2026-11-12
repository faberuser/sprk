using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NMessage.NProtocol;

public static class JM_NShared_NMessage_NProtocol_BattleBanPickInfoChanged
{
	public static BattleBanPickInfoChanged Parse(string s)
	{
		return Parse(s, new BattleBanPickInfoChanged());
	}

	public static BattleBanPickInfoChanged Parse(string s, BattleBanPickInfoChanged o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static BattleBanPickInfoChanged Parse(IDictionary d)
	{
		return Parse(d, new BattleBanPickInfoChanged());
	}

	public static BattleBanPickInfoChanged Parse(IDictionary d, BattleBanPickInfoChanged o)
	{
		o.BattleBanPickInfo = BaseJsonMarshaler.ParseObject(d, "BattleBanPickInfo", JM_NShared_PlayerBattleBanPickInfo.Parse);
		return o;
	}

	public static string EncodeJsonString(BattleBanPickInfoChanged o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(BattleBanPickInfoChanged o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("BattleBanPickInfo", JM_NShared_PlayerBattleBanPickInfo.EncodeJsonObject(o.BattleBanPickInfo));
		return dictionary;
	}
}
