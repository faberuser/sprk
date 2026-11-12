using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared.NContentsDefine;

public static class JM_NShared_NContentsDefine_Attendance
{
	public static Attendance Parse(string s)
	{
		return Parse(s, new Attendance());
	}

	public static Attendance Parse(string s, Attendance o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static Attendance Parse(IDictionary d)
	{
		return Parse(d, new Attendance());
	}

	public static Attendance Parse(IDictionary d, Attendance o)
	{
		return o;
	}

	public static string EncodeJsonString(Attendance o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(Attendance o)
	{
		return new Dictionary<string, object>();
	}
}
