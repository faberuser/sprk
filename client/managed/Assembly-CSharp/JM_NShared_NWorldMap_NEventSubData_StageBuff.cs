using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared.NWorldMap.NEventSubData;

public static class JM_NShared_NWorldMap_NEventSubData_StageBuff
{
	public static StageBuff Parse(string s)
	{
		return Parse(s, new StageBuff());
	}

	public static StageBuff Parse(string s, StageBuff o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static StageBuff Parse(IDictionary d)
	{
		return Parse(d, new StageBuff());
	}

	public static StageBuff Parse(IDictionary d, StageBuff o)
	{
		return o;
	}

	public static string EncodeJsonString(StageBuff o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(StageBuff o)
	{
		return new Dictionary<string, object>();
	}
}
