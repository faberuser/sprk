using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_BattleBanPickStartCancelNot
{
	public static BattleBanPickStartCancelNot Parse(string s)
	{
		return Parse(s, new BattleBanPickStartCancelNot());
	}

	public static BattleBanPickStartCancelNot Parse(string s, BattleBanPickStartCancelNot o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static BattleBanPickStartCancelNot Parse(IDictionary d)
	{
		return Parse(d, new BattleBanPickStartCancelNot());
	}

	public static BattleBanPickStartCancelNot Parse(IDictionary d, BattleBanPickStartCancelNot o)
	{
		return o;
	}

	public static string EncodeJsonString(BattleBanPickStartCancelNot o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(BattleBanPickStartCancelNot o)
	{
		return new Dictionary<string, object>();
	}
}
