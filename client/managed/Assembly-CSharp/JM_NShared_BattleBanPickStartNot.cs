using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_BattleBanPickStartNot
{
	public static BattleBanPickStartNot Parse(string s)
	{
		return Parse(s, new BattleBanPickStartNot());
	}

	public static BattleBanPickStartNot Parse(string s, BattleBanPickStartNot o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static BattleBanPickStartNot Parse(IDictionary d)
	{
		return Parse(d, new BattleBanPickStartNot());
	}

	public static BattleBanPickStartNot Parse(IDictionary d, BattleBanPickStartNot o)
	{
		return o;
	}

	public static string EncodeJsonString(BattleBanPickStartNot o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(BattleBanPickStartNot o)
	{
		return new Dictionary<string, object>();
	}
}
