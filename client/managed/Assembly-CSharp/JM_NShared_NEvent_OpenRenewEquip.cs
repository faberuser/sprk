using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared.NEvent;

public static class JM_NShared_NEvent_OpenRenewEquip
{
	public static OpenRenewEquip Parse(string s)
	{
		return Parse(s, new OpenRenewEquip());
	}

	public static OpenRenewEquip Parse(string s, OpenRenewEquip o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static OpenRenewEquip Parse(IDictionary d)
	{
		return Parse(d, new OpenRenewEquip());
	}

	public static OpenRenewEquip Parse(IDictionary d, OpenRenewEquip o)
	{
		return o;
	}

	public static string EncodeJsonString(OpenRenewEquip o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(OpenRenewEquip o)
	{
		return new Dictionary<string, object>();
	}
}
