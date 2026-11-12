using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared.NWorldMap.NEventSubData;

public static class JM_NShared_NWorldMap_NEventSubData_TreasureBox
{
	public static TreasureBox Parse(string s)
	{
		return Parse(s, new TreasureBox());
	}

	public static TreasureBox Parse(string s, TreasureBox o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static TreasureBox Parse(IDictionary d)
	{
		return Parse(d, new TreasureBox());
	}

	public static TreasureBox Parse(IDictionary d, TreasureBox o)
	{
		return o;
	}

	public static string EncodeJsonString(TreasureBox o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(TreasureBox o)
	{
		return new Dictionary<string, object>();
	}
}
