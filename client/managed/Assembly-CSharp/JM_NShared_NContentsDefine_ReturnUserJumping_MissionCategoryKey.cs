using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NContentsDefine;

public static class JM_NShared_NContentsDefine_ReturnUserJumping_MissionCategoryKey
{
	public static ReturnUserJumping.MissionCategoryKey Parse(string s)
	{
		return Parse(s, new ReturnUserJumping.MissionCategoryKey());
	}

	public static ReturnUserJumping.MissionCategoryKey Parse(string s, ReturnUserJumping.MissionCategoryKey o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static ReturnUserJumping.MissionCategoryKey Parse(IDictionary d)
	{
		return Parse(d, new ReturnUserJumping.MissionCategoryKey());
	}

	public static ReturnUserJumping.MissionCategoryKey Parse(IDictionary d, ReturnUserJumping.MissionCategoryKey o)
	{
		o.Main = BaseJsonMarshaler.ParseValueInt(d, "Main");
		o.Sub = BaseJsonMarshaler.ParseValueInt(d, "Sub");
		return o;
	}

	public static string EncodeJsonString(ReturnUserJumping.MissionCategoryKey o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(ReturnUserJumping.MissionCategoryKey o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("Main", o.Main.ToString());
		dictionary.Add("Sub", o.Sub.ToString());
		return dictionary;
	}
}
