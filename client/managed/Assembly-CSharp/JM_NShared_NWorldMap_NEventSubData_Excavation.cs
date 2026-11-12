using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared.NWorldMap.NEventSubData;

public static class JM_NShared_NWorldMap_NEventSubData_Excavation
{
	public static Excavation Parse(string s)
	{
		return Parse(s, new Excavation());
	}

	public static Excavation Parse(string s, Excavation o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static Excavation Parse(IDictionary d)
	{
		return Parse(d, new Excavation());
	}

	public static Excavation Parse(IDictionary d, Excavation o)
	{
		return o;
	}

	public static string EncodeJsonString(Excavation o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(Excavation o)
	{
		return new Dictionary<string, object>();
	}
}
