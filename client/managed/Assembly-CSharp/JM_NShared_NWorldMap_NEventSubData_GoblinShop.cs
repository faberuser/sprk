using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared.NWorldMap.NEventSubData;

public static class JM_NShared_NWorldMap_NEventSubData_GoblinShop
{
	public static GoblinShop Parse(string s)
	{
		return Parse(s, new GoblinShop());
	}

	public static GoblinShop Parse(string s, GoblinShop o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static GoblinShop Parse(IDictionary d)
	{
		return Parse(d, new GoblinShop());
	}

	public static GoblinShop Parse(IDictionary d, GoblinShop o)
	{
		return o;
	}

	public static string EncodeJsonString(GoblinShop o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(GoblinShop o)
	{
		return new Dictionary<string, object>();
	}
}
