using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.GetEquipGacha;

public static class JM_NShared_GetEquipGacha_Request
{
	public static NShared.GetEquipGacha.Request Parse(string s)
	{
		return Parse(s, new NShared.GetEquipGacha.Request());
	}

	public static NShared.GetEquipGacha.Request Parse(string s, NShared.GetEquipGacha.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.GetEquipGacha.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.GetEquipGacha.Request());
	}

	public static NShared.GetEquipGacha.Request Parse(IDictionary d, NShared.GetEquipGacha.Request o)
	{
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.GetEquipGacha.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.GetEquipGacha.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
