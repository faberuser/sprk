using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared.NEvent;

public static class JM_NShared_NEvent_OpenMaze
{
	public static OpenMaze Parse(string s)
	{
		return Parse(s, new OpenMaze());
	}

	public static OpenMaze Parse(string s, OpenMaze o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static OpenMaze Parse(IDictionary d)
	{
		return Parse(d, new OpenMaze());
	}

	public static OpenMaze Parse(IDictionary d, OpenMaze o)
	{
		return o;
	}

	public static string EncodeJsonString(OpenMaze o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(OpenMaze o)
	{
		return new Dictionary<string, object>();
	}
}
