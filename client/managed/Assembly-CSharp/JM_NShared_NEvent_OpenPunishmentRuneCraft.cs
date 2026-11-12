using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared.NEvent;

public static class JM_NShared_NEvent_OpenPunishmentRuneCraft
{
	public static OpenPunishmentRuneCraft Parse(string s)
	{
		return Parse(s, new OpenPunishmentRuneCraft());
	}

	public static OpenPunishmentRuneCraft Parse(string s, OpenPunishmentRuneCraft o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static OpenPunishmentRuneCraft Parse(IDictionary d)
	{
		return Parse(d, new OpenPunishmentRuneCraft());
	}

	public static OpenPunishmentRuneCraft Parse(IDictionary d, OpenPunishmentRuneCraft o)
	{
		return o;
	}

	public static string EncodeJsonString(OpenPunishmentRuneCraft o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(OpenPunishmentRuneCraft o)
	{
		return new Dictionary<string, object>();
	}
}
