using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared.NEvent;

public static class JM_NShared_NEvent_OpenTechnoEnchant
{
	public static OpenTechnoEnchant Parse(string s)
	{
		return Parse(s, new OpenTechnoEnchant());
	}

	public static OpenTechnoEnchant Parse(string s, OpenTechnoEnchant o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static OpenTechnoEnchant Parse(IDictionary d)
	{
		return Parse(d, new OpenTechnoEnchant());
	}

	public static OpenTechnoEnchant Parse(IDictionary d, OpenTechnoEnchant o)
	{
		return o;
	}

	public static string EncodeJsonString(OpenTechnoEnchant o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(OpenTechnoEnchant o)
	{
		return new Dictionary<string, object>();
	}
}
