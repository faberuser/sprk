using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_BattleInstanceCreateInfo
{
	public static BattleInstanceCreateInfo Parse(string s)
	{
		return Parse(s, new BattleInstanceCreateInfo());
	}

	public static BattleInstanceCreateInfo Parse(string s, BattleInstanceCreateInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static BattleInstanceCreateInfo Parse(IDictionary d)
	{
		return Parse(d, new BattleInstanceCreateInfo());
	}

	public static BattleInstanceCreateInfo Parse(IDictionary d, BattleInstanceCreateInfo o)
	{
		return o;
	}

	public static string EncodeJsonString(BattleInstanceCreateInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(BattleInstanceCreateInfo o)
	{
		return new Dictionary<string, object>();
	}
}
