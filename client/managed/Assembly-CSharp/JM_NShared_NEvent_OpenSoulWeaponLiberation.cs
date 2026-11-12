using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared.NEvent;

public static class JM_NShared_NEvent_OpenSoulWeaponLiberation
{
	public static OpenSoulWeaponLiberation Parse(string s)
	{
		return Parse(s, new OpenSoulWeaponLiberation());
	}

	public static OpenSoulWeaponLiberation Parse(string s, OpenSoulWeaponLiberation o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static OpenSoulWeaponLiberation Parse(IDictionary d)
	{
		return Parse(d, new OpenSoulWeaponLiberation());
	}

	public static OpenSoulWeaponLiberation Parse(IDictionary d, OpenSoulWeaponLiberation o)
	{
		return o;
	}

	public static string EncodeJsonString(OpenSoulWeaponLiberation o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(OpenSoulWeaponLiberation o)
	{
		return new Dictionary<string, object>();
	}
}
