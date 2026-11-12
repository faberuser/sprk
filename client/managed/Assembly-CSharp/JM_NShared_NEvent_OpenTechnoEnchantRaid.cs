using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NEvent;

public static class JM_NShared_NEvent_OpenTechnoEnchantRaid
{
	public static OpenTechnoEnchantRaid Parse(string s)
	{
		return Parse(s, new OpenTechnoEnchantRaid());
	}

	public static OpenTechnoEnchantRaid Parse(string s, OpenTechnoEnchantRaid o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static OpenTechnoEnchantRaid Parse(IDictionary d)
	{
		return Parse(d, new OpenTechnoEnchantRaid());
	}

	public static OpenTechnoEnchantRaid Parse(IDictionary d, OpenTechnoEnchantRaid o)
	{
		o.IsActive = BaseJsonMarshaler.ParseValueBool(d, "IsActive");
		return o;
	}

	public static string EncodeJsonString(OpenTechnoEnchantRaid o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(OpenTechnoEnchantRaid o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("IsActive", o.IsActive.ToString());
		return dictionary;
	}
}
