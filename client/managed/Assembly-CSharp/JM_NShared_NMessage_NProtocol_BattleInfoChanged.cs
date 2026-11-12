using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NMessage.NProtocol;

public static class JM_NShared_NMessage_NProtocol_BattleInfoChanged
{
	public static BattleInfoChanged Parse(string s)
	{
		return Parse(s, new BattleInfoChanged());
	}

	public static BattleInfoChanged Parse(string s, BattleInfoChanged o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static BattleInfoChanged Parse(IDictionary d)
	{
		return Parse(d, new BattleInfoChanged());
	}

	public static BattleInfoChanged Parse(IDictionary d, BattleInfoChanged o)
	{
		o.BattleInfo = BaseJsonMarshaler.ParseObject(d, "BattleInfo", JM_NShared_PlayerBattleInfo.Parse);
		return o;
	}

	public static string EncodeJsonString(BattleInfoChanged o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(BattleInfoChanged o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("BattleInfo", JM_NShared_PlayerBattleInfo.EncodeJsonObject(o.BattleInfo));
		return dictionary;
	}
}
