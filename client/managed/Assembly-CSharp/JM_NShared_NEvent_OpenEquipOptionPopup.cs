using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared.NEvent;

public static class JM_NShared_NEvent_OpenEquipOptionPopup
{
	public static OpenEquipOptionPopup Parse(string s)
	{
		return Parse(s, new OpenEquipOptionPopup());
	}

	public static OpenEquipOptionPopup Parse(string s, OpenEquipOptionPopup o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static OpenEquipOptionPopup Parse(IDictionary d)
	{
		return Parse(d, new OpenEquipOptionPopup());
	}

	public static OpenEquipOptionPopup Parse(IDictionary d, OpenEquipOptionPopup o)
	{
		return o;
	}

	public static string EncodeJsonString(OpenEquipOptionPopup o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(OpenEquipOptionPopup o)
	{
		return new Dictionary<string, object>();
	}
}
